namespace RVR.Framework.NaturalQuery.Services;

using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using RVR.Framework.NaturalQuery.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Rule-based natural language parser that converts French and English
/// query strings into a structured <see cref="QueryPlan"/>.
/// </summary>
public sealed partial class NaturalLanguageParser
{
    private readonly ILogger<NaturalLanguageParser> _logger;
    private readonly NaturalQueryOptions _queryOptions;

    // ── Keyword mappings (French + English) ──────────────────────────

    private static readonly Dictionary<string, FilterOperator> OperatorKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        // French
        { "contenant", FilterOperator.Contains },
        { "contient", FilterOperator.Contains },
        { "commençant par", FilterOperator.StartsWith },
        { "commencant par", FilterOperator.StartsWith },
        { "finissant par", FilterOperator.EndsWith },
        { "égal à", FilterOperator.Equals },
        { "egal a", FilterOperator.Equals },
        { "différent de", FilterOperator.NotEquals },
        { "different de", FilterOperator.NotEquals },
        { "supérieur à", FilterOperator.GreaterThan },
        { "superieur a", FilterOperator.GreaterThan },
        { "supérieur ou égal à", FilterOperator.GreaterThanOrEqual },
        { "superieur ou egal a", FilterOperator.GreaterThanOrEqual },
        { "inférieur à", FilterOperator.LessThan },
        { "inferieur a", FilterOperator.LessThan },
        { "inférieur ou égal à", FilterOperator.LessThanOrEqual },
        { "inferieur ou egal a", FilterOperator.LessThanOrEqual },
        { "est null", FilterOperator.IsNull },
        { "est nul", FilterOperator.IsNull },
        { "n'est pas null", FilterOperator.IsNotNull },
        { "n'est pas nul", FilterOperator.IsNotNull },
        // English
        { "containing", FilterOperator.Contains },
        { "contains", FilterOperator.Contains },
        { "starting with", FilterOperator.StartsWith },
        { "starts with", FilterOperator.StartsWith },
        { "ending with", FilterOperator.EndsWith },
        { "ends with", FilterOperator.EndsWith },
        { "equal to", FilterOperator.Equals },
        { "equals", FilterOperator.Equals },
        { "not equal to", FilterOperator.NotEquals },
        { "not equals", FilterOperator.NotEquals },
        { "greater than or equal to", FilterOperator.GreaterThanOrEqual },
        { "greater than or equal", FilterOperator.GreaterThanOrEqual },
        { "greater than", FilterOperator.GreaterThan },
        { "less than or equal to", FilterOperator.LessThanOrEqual },
        { "less than or equal", FilterOperator.LessThanOrEqual },
        { "less than", FilterOperator.LessThan },
        { "is null", FilterOperator.IsNull },
        { "is not null", FilterOperator.IsNotNull },
    };

    private static readonly string[] SortKeywordsFr = ["trié par", "trie par", "ordonné par", "ordonne par"];
    private static readonly string[] SortKeywordsEn = ["sorted by", "ordered by", "order by", "sort by"];
    private static readonly string[] AllSortKeywords = [.. SortKeywordsFr, .. SortKeywordsEn];

    private static readonly string[] DescKeywords = ["décroissant", "decroissant", "desc", "descending"];
    private static readonly string[] AscKeywords = ["croissant", "asc", "ascending"];

    private static readonly string[] LimitKeywordsFr = ["top", "premiers", "limite", "limité à", "limite a"];
    private static readonly string[] LimitKeywordsEn = ["top", "first", "limit", "take"];
    private static readonly string[] AllLimitKeywords = [.. LimitKeywordsFr, .. LimitKeywordsEn];

    private static readonly string[] SkipKeywordsFr = ["sauter", "ignorer", "à partir de"];
    private static readonly string[] SkipKeywordsEn = ["skip", "offset"];
    private static readonly string[] AllSkipKeywords = [.. SkipKeywordsFr, .. SkipKeywordsEn];

    // Boolean property aliases (FR + EN)
    private static readonly Dictionary<string, (string[] PropertyCandidates, string Value)> BooleanAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        { "actif", (["IsActive", "Active", "Actif", "EstActif"], "true") },
        { "actifs", (["IsActive", "Active", "Actif", "EstActif"], "true") },
        { "active", (["IsActive", "Active", "Actif", "EstActif"], "true") },
        { "inactif", (["IsActive", "Active", "Actif", "EstActif"], "false") },
        { "inactifs", (["IsActive", "Active", "Actif", "EstActif"], "false") },
        { "inactive", (["IsActive", "Active", "Actif", "EstActif"], "false") },
        { "supprimé", (["IsDeleted", "Deleted", "Supprime", "EstSupprime"], "true") },
        { "supprime", (["IsDeleted", "Deleted", "Supprime", "EstSupprime"], "true") },
        { "supprimés", (["IsDeleted", "Deleted", "Supprime", "EstSupprime"], "true") },
        { "supprimes", (["IsDeleted", "Deleted", "Supprime", "EstSupprime"], "true") },
        { "deleted", (["IsDeleted", "Deleted", "Supprime", "EstSupprime"], "true") },
        { "non supprimé", (["IsDeleted", "Deleted", "Supprime", "EstSupprime"], "false") },
        { "non supprime", (["IsDeleted", "Deleted", "Supprime", "EstSupprime"], "false") },
        { "non supprimés", (["IsDeleted", "Deleted", "Supprime", "EstSupprime"], "false") },
        { "non supprimes", (["IsDeleted", "Deleted", "Supprime", "EstSupprime"], "false") },
        { "not deleted", (["IsDeleted", "Deleted", "Supprime", "EstSupprime"], "false") },
        { "enabled", (["IsEnabled", "Enabled"], "true") },
        { "disabled", (["IsEnabled", "Enabled"], "false") },
        { "activé", (["IsEnabled", "Enabled"], "true") },
        { "active_enabled", (["IsEnabled", "Enabled"], "true") },
        { "désactivé", (["IsEnabled", "Enabled"], "false") },
        { "desactive", (["IsEnabled", "Enabled"], "false") },
    };

    // Temporal keywords
    private static readonly Dictionary<string, Func<(DateTime Start, DateTime End)>> TemporalKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        { "aujourd'hui", () => { var t = DateTime.Today; return (t, t.AddDays(1).AddTicks(-1)); } },
        { "aujourd hui", () => { var t = DateTime.Today; return (t, t.AddDays(1).AddTicks(-1)); } },
        { "today", () => { var t = DateTime.Today; return (t, t.AddDays(1).AddTicks(-1)); } },
        { "hier", () => { var t = DateTime.Today.AddDays(-1); return (t, t.AddDays(1).AddTicks(-1)); } },
        { "yesterday", () => { var t = DateTime.Today.AddDays(-1); return (t, t.AddDays(1).AddTicks(-1)); } },
        { "ce mois", () => { var t = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); return (t, t.AddMonths(1).AddTicks(-1)); } },
        { "this month", () => { var t = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); return (t, t.AddMonths(1).AddTicks(-1)); } },
        { "cette semaine", () => { var t = DateTime.Today; var dow = (int)t.DayOfWeek; var start = t.AddDays(-(dow == 0 ? 6 : dow - 1)); return (start, start.AddDays(7).AddTicks(-1)); } },
        { "this week", () => { var t = DateTime.Today; var dow = (int)t.DayOfWeek; var start = t.AddDays(-(dow == 0 ? 6 : dow - 1)); return (start, start.AddDays(7).AddTicks(-1)); } },
        { "cette année", () => { var t = new DateTime(DateTime.Today.Year, 1, 1); return (t, t.AddYears(1).AddTicks(-1)); } },
        { "cette annee", () => { var t = new DateTime(DateTime.Today.Year, 1, 1); return (t, t.AddYears(1).AddTicks(-1)); } },
        { "this year", () => { var t = new DateTime(DateTime.Today.Year, 1, 1); return (t, t.AddYears(1).AddTicks(-1)); } },
        { "le mois dernier", () => { var t = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1); return (t, t.AddMonths(1).AddTicks(-1)); } },
        { "last month", () => { var t = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1); return (t, t.AddMonths(1).AddTicks(-1)); } },
        { "la semaine dernière", () => { var t = DateTime.Today; var dow = (int)t.DayOfWeek; var start = t.AddDays(-(dow == 0 ? 6 : dow - 1)).AddDays(-7); return (start, start.AddDays(7).AddTicks(-1)); } },
        { "la semaine derniere", () => { var t = DateTime.Today; var dow = (int)t.DayOfWeek; var start = t.AddDays(-(dow == 0 ? 6 : dow - 1)).AddDays(-7); return (start, start.AddDays(7).AddTicks(-1)); } },
        { "last week", () => { var t = DateTime.Today; var dow = (int)t.DayOfWeek; var start = t.AddDays(-(dow == 0 ? 6 : dow - 1)).AddDays(-7); return (start, start.AddDays(7).AddTicks(-1)); } },
    };

    // Date property candidates for temporal queries
    private static readonly string[] DatePropertyCandidates =
    [
        "CreatedAt", "DateCreated", "CreatedDate", "CreateDate", "Created",
        "DateCreation", "DateCréation",
        "UpdatedAt", "DateUpdated", "ModifiedAt", "ModifiedDate", "Updated",
        "DateModification",
    ];

    /// <summary>
    /// Initializes a new instance of <see cref="NaturalLanguageParser"/>.
    /// </summary>
    public NaturalLanguageParser(ILogger<NaturalLanguageParser> logger, IOptions<NaturalQueryOptions> options)
    {
        _logger = logger;
        _queryOptions = options.Value;
    }

    /// <summary>
    /// Parses a natural language query into a <see cref="QueryPlan"/> based on the target entity type.
    /// </summary>
    public QueryPlan Parse(string query, Type entityType)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(entityType);

        var plan = new QueryPlan();
        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var normalizedQuery = query.Trim();

        _logger.LogDebug("Parsing natural language query: '{Query}' for entity: {EntityType}", query, entityType.Name);

        // 1. Extract limit / top
        ExtractLimit(ref normalizedQuery, plan);

        // 2. Extract skip / offset
        ExtractSkip(ref normalizedQuery, plan);

        // 3. Extract sort clauses
        ExtractSorts(ref normalizedQuery, plan, properties);

        // 4. Extract temporal conditions
        ExtractTemporalConditions(ref normalizedQuery, plan, properties);

        // 5. Extract boolean alias conditions
        ExtractBooleanConditions(ref normalizedQuery, plan, properties);

        // 6. Extract operator-based conditions (e.g. "prix > 100", "name contains 'phone'")
        ExtractOperatorConditions(ref normalizedQuery, plan, properties);

        // 7. Extract comparison operators written as symbols
        ExtractSymbolOperatorConditions(ref normalizedQuery, plan, properties);

        // 8. Clamp Take and Skip to configured maximums
        if (plan.Take.HasValue && plan.Take.Value > _queryOptions.MaxTake)
        {
            _logger.LogWarning("Take value {Take} exceeds maximum {MaxTake}, clamping", plan.Take.Value, _queryOptions.MaxTake);
            plan.Take = _queryOptions.MaxTake;
        }

        if (plan.Skip.HasValue && plan.Skip.Value > _queryOptions.MaxSkip)
        {
            _logger.LogWarning("Skip value {Skip} exceeds maximum {MaxSkip}, clamping", plan.Skip.Value, _queryOptions.MaxSkip);
            plan.Skip = _queryOptions.MaxSkip;
        }

        // 9. Enforce property whitelist if configured
        if (_queryOptions.AllowedProperties is { Count: > 0 })
        {
            var rejectedFilters = plan.Filters
                .Where(f => !_queryOptions.AllowedProperties.Contains(f.PropertyName))
                .ToList();

            foreach (var rejected in rejectedFilters)
            {
                _logger.LogWarning("Filter on property '{Property}' rejected: not in allowed properties whitelist", rejected.PropertyName);
                plan.Filters.Remove(rejected);
            }

            var rejectedSorts = plan.Sorts
                .Where(s => !_queryOptions.AllowedProperties.Contains(s.PropertyName))
                .ToList();

            foreach (var rejected in rejectedSorts)
            {
                _logger.LogWarning("Sort on property '{Property}' rejected: not in allowed properties whitelist", rejected.PropertyName);
                plan.Sorts.Remove(rejected);
            }
        }

        _logger.LogDebug("Parsed QueryPlan: {FilterCount} filters, {SortCount} sorts, Take={Take}, Skip={Skip}",
            plan.Filters.Count, plan.Sorts.Count, plan.Take, plan.Skip);

        return plan;
    }

    // ── Limit extraction ─────────────────────────────────────────────

    private static void ExtractLimit(ref string query, QueryPlan plan)
    {
        // Pattern: "top N", "premiers N", "limite N", "first N", "take N", "limit N"
        foreach (var kw in AllLimitKeywords)
        {
            var pattern = $@"(?:^|\s){Regex.Escape(kw)}\s+(\d+)";
            var match = Regex.Match(query, pattern, RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var take))
            {
                plan.Take = take;
                query = query.Remove(match.Index, match.Length).Trim();
                return;
            }
        }

        // Also handle "N premiers/first" pattern
        var reverseMatch = Regex.Match(query, @"(\d+)\s+(?:premiers|premières|premieres|first)", RegexOptions.IgnoreCase);
        if (reverseMatch.Success && int.TryParse(reverseMatch.Groups[1].Value, out var takeReverse))
        {
            plan.Take = takeReverse;
            query = query.Remove(reverseMatch.Index, reverseMatch.Length).Trim();
        }
    }

    // ── Skip extraction ──────────────────────────────────────────────

    private static void ExtractSkip(ref string query, QueryPlan plan)
    {
        foreach (var kw in AllSkipKeywords)
        {
            var pattern = $@"(?:^|\s){Regex.Escape(kw)}\s+(\d+)";
            var match = Regex.Match(query, pattern, RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var skip))
            {
                plan.Skip = skip;
                query = query.Remove(match.Index, match.Length).Trim();
                return;
            }
        }
    }

    // ── Sort extraction ──────────────────────────────────────────────

    private void ExtractSorts(ref string query, QueryPlan plan, PropertyInfo[] properties)
    {
        foreach (var kw in AllSortKeywords)
        {
            var idx = query.IndexOf(kw, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) continue;

            var sortClause = query[(idx + kw.Length)..].Trim();
            query = query[..idx].Trim();

            // Parse sort clause: "prix décroissant" or "price desc, name asc"
            var sortParts = sortClause.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in sortParts)
            {
                var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0) continue;

                var propName = FuzzyMatchProperty(tokens[0], properties);
                if (propName is null)
                {
                    _logger.LogWarning("Could not match sort property '{Token}' to any entity property", tokens[0]);
                    continue;
                }

                var desc = tokens.Length > 1 && DescKeywords.Any(d => tokens[1].Equals(d, StringComparison.OrdinalIgnoreCase));
                plan.Sorts.Add(new SortCondition { PropertyName = propName, Descending = desc });
            }
            return;
        }
    }

    // ── Temporal extraction ──────────────────────────────────────────

    private void ExtractTemporalConditions(ref string query, QueryPlan plan, PropertyInfo[] properties)
    {
        // Sort by length descending to match longer phrases first
        foreach (var (keyword, rangeFactory) in TemporalKeywords.OrderByDescending(kv => kv.Key.Length))
        {
            var idx = query.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) continue;

            var (start, end) = rangeFactory();

            // Try to find a date property on the entity
            var dateProp = FindDateProperty(properties);
            if (dateProp is null)
            {
                _logger.LogWarning("Temporal keyword '{Keyword}' found but no date property on entity", keyword);
                continue;
            }

            plan.Filters.Add(new FilterCondition
            {
                PropertyName = dateProp,
                Operator = FilterOperator.GreaterThanOrEqual,
                Value = start.ToString("o", CultureInfo.InvariantCulture),
                LogicalOp = plan.Filters.Count > 0 ? LogicalOperator.And : LogicalOperator.And
            });
            plan.Filters.Add(new FilterCondition
            {
                PropertyName = dateProp,
                Operator = FilterOperator.LessThanOrEqual,
                Value = end.ToString("o", CultureInfo.InvariantCulture),
                LogicalOp = LogicalOperator.And
            });

            query = query.Remove(idx, keyword.Length).Trim();
            return; // Only one temporal keyword per query
        }
    }

    // ── Boolean alias extraction ─────────────────────────────────────

    private void ExtractBooleanConditions(ref string query, QueryPlan plan, PropertyInfo[] properties)
    {
        // Sort by length descending so "non supprimé" matches before "supprimé"
        foreach (var (keyword, (candidates, value)) in BooleanAliases.OrderByDescending(kv => kv.Key.Length))
        {
            // Use word boundary matching
            var pattern = $@"(?:^|\s){Regex.Escape(keyword)}(?:\s|$)";
            var match = Regex.Match(query, pattern, RegexOptions.IgnoreCase);
            if (!match.Success) continue;

            // Find matching property
            var propName = candidates
                .Select(c => FuzzyMatchProperty(c, properties))
                .FirstOrDefault(p => p is not null);

            if (propName is null)
            {
                _logger.LogDebug("Boolean keyword '{Keyword}' found but no matching property on entity", keyword);
                continue;
            }

            plan.Filters.Add(new FilterCondition
            {
                PropertyName = propName,
                Operator = FilterOperator.Equals,
                Value = value,
                LogicalOp = plan.Filters.Count > 0 ? LogicalOperator.And : LogicalOperator.And
            });

            query = query.Remove(match.Index, match.Length).Trim();
        }
    }

    // ── Keyword operator extraction ──────────────────────────────────

    private void ExtractOperatorConditions(ref string query, QueryPlan plan, PropertyInfo[] properties)
    {
        // Sort by key length descending so longer phrases match first
        foreach (var (keyword, op) in OperatorKeywords.OrderByDescending(kv => kv.Key.Length))
        {
            var idx = query.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) continue;

            // Text before keyword should end with property name
            var before = query[..idx].Trim();
            var after = query[(idx + keyword.Length)..].Trim();

            // Try to find property name from the tokens before the keyword
            var beforeTokens = before.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (beforeTokens.Length == 0) continue;

            // The property is typically the last meaningful token before the operator keyword
            // But we also need to handle "avec prix > 100" -> skip "avec"/"with"
            string? propName = null;
            for (var i = beforeTokens.Length - 1; i >= 0; i--)
            {
                propName = FuzzyMatchProperty(beforeTokens[i], properties);
                if (propName is not null) break;
            }

            if (propName is null) continue;

            // Extract value after operator
            var value = ExtractValue(after);
            if (string.IsNullOrEmpty(value) && op != FilterOperator.IsNull && op != FilterOperator.IsNotNull)
                continue;

            plan.Filters.Add(new FilterCondition
            {
                PropertyName = propName,
                Operator = op,
                Value = value,
                LogicalOp = plan.Filters.Count > 0 ? LogicalOperator.And : LogicalOperator.And
            });

            // Remove the matched portion
            var endIdx = idx + keyword.Length + (value.Length > 0 ? after.IndexOf(value, StringComparison.Ordinal) + value.Length : 0);
            if (endIdx > query.Length) endIdx = query.Length;
            // Find start of property token
            var propTokenStart = before.LastIndexOf(beforeTokens[^1], StringComparison.OrdinalIgnoreCase);
            if (propTokenStart < 0) propTokenStart = 0;

            query = (query[..propTokenStart] + query[endIdx..]).Trim();
        }
    }

    // ── Symbol operator extraction (e.g., "prix > 100") ─────────────

    private void ExtractSymbolOperatorConditions(ref string query, QueryPlan plan, PropertyInfo[] properties)
    {
        // Match patterns like: property >= value, property > value, property != value
        var symbolPattern = SymbolOperatorRegex();
        var matches = symbolPattern.Matches(query);

        // Process in reverse order to preserve indices when removing
        foreach (Match match in matches.Cast<Match>().Reverse())
        {
            var propToken = match.Groups[1].Value.Trim();
            var opSymbol = match.Groups[2].Value.Trim();
            var value = match.Groups[3].Value.Trim().Trim('\'', '"');

            var propName = FuzzyMatchProperty(propToken, properties);
            if (propName is null) continue;

            var op = opSymbol switch
            {
                ">=" => FilterOperator.GreaterThanOrEqual,
                "<=" => FilterOperator.LessThanOrEqual,
                "!=" => FilterOperator.NotEquals,
                ">" => FilterOperator.GreaterThan,
                "<" => FilterOperator.LessThan,
                "=" => FilterOperator.Equals,
                _ => FilterOperator.Equals
            };

            plan.Filters.Add(new FilterCondition
            {
                PropertyName = propName,
                Operator = op,
                Value = value,
                LogicalOp = plan.Filters.Count > 0 ? LogicalOperator.And : LogicalOperator.And
            });

            query = query.Remove(match.Index, match.Length).Trim();
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Fuzzy-matches a token against entity property names.
    /// Checks exact match, case-insensitive match, and Levenshtein distance.
    /// </summary>
    internal static string? FuzzyMatchProperty(string token, PropertyInfo[] properties)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        var cleaned = token.Trim('\'', '"', ',', '.', '!', '?');

        // 1. Exact match
        var exact = properties.FirstOrDefault(p => p.Name.Equals(cleaned, StringComparison.OrdinalIgnoreCase));
        if (exact is not null) return exact.Name;

        // 2. Normalized match (remove underscores, dashes)
        var normalized = cleaned.Replace("_", "").Replace("-", "");
        var normMatch = properties.FirstOrDefault(p =>
            p.Name.Replace("_", "").Replace("-", "").Equals(normalized, StringComparison.OrdinalIgnoreCase));
        if (normMatch is not null) return normMatch.Name;

        // 3. Prefix match (token is a prefix of property name, min 3 chars)
        if (cleaned.Length >= 3)
        {
            var prefixMatch = properties.FirstOrDefault(p =>
                p.Name.StartsWith(cleaned, StringComparison.OrdinalIgnoreCase));
            if (prefixMatch is not null) return prefixMatch.Name;
        }

        // 4. Contains match (property name contains the token, min 4 chars)
        if (cleaned.Length >= 4)
        {
            var containsMatch = properties.FirstOrDefault(p =>
                p.Name.Contains(cleaned, StringComparison.OrdinalIgnoreCase));
            if (containsMatch is not null) return containsMatch.Name;
        }

        // 5. Levenshtein distance (max distance 2, min token length 4)
        if (cleaned.Length >= 4)
        {
            var bestMatch = properties
                .Select(p => (Property: p, Distance: LevenshteinDistance(p.Name.ToLowerInvariant(), cleaned.ToLowerInvariant())))
                .Where(x => x.Distance <= 2)
                .OrderBy(x => x.Distance)
                .FirstOrDefault();

            if (bestMatch.Property is not null)
                return bestMatch.Property.Name;
        }

        return null;
    }

    private static string? FindDateProperty(PropertyInfo[] properties)
    {
        // Check standard date property candidates
        foreach (var candidate in DatePropertyCandidates)
        {
            var prop = properties.FirstOrDefault(p =>
                p.Name.Equals(candidate, StringComparison.OrdinalIgnoreCase) &&
                IsDateType(p.PropertyType));
            if (prop is not null) return prop.Name;
        }

        // Fallback: find any DateTime property
        var anyDate = properties.FirstOrDefault(p => IsDateType(p.PropertyType));
        return anyDate?.Name;
    }

    private static bool IsDateType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset) || underlying == typeof(DateOnly);
    }

    private static string ExtractValue(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        // Handle quoted values: 'phone' or "phone"
        var quotedMatch = Regex.Match(text, @"^['""](.+?)['""]");
        if (quotedMatch.Success)
            return quotedMatch.Groups[1].Value;

        // Take first token as value
        var tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length > 0)
        {
            // Skip noise words
            string[] noise = ["dans", "le", "la", "les", "du", "de", "in", "the", "a", "an", "of"];
            foreach (var t in tokens)
            {
                if (!noise.Contains(t, StringComparer.OrdinalIgnoreCase))
                    return t.Trim('\'', '"');
            }
        }

        return text.Trim();
    }

    /// <summary>
    /// Computes the Levenshtein distance between two strings.
    /// </summary>
    internal static int LevenshteinDistance(string s, string t)
    {
        var n = s.Length;
        var m = t.Length;
        var d = new int[n + 1, m + 1];

        for (var i = 0; i <= n; i++) d[i, 0] = i;
        for (var j = 0; j <= m; j++) d[0, j] = j;

        for (var i = 1; i <= n; i++)
        {
            for (var j = 1; j <= m; j++)
            {
                var cost = s[i - 1] == t[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[n, m];
    }

    [GeneratedRegex(@"(\w+)\s*(>=|<=|!=|>|<|=)\s*(['""]?[\w.,]+['""]?)")]
    private static partial Regex SymbolOperatorRegex();
}
