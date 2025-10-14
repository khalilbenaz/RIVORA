namespace RVR.CLI.Analyzers;

using RVR.CLI.Analyzers.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

/// <summary>
/// Detects common security vulnerabilities in C# / ASP.NET Core projects.
/// </summary>
public sealed partial class SecurityAnalyzer : IAnalyzer
{
    public string Name => "Security Vulnerabilities";

    public async Task<AnalysisResult> AnalyzeAsync(string projectPath, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var findings = new List<AnalysisFinding>();

        var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                     && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                     && !f.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}"));

        int fileCount = 0;
        foreach (var file in csFiles)
        {
            ct.ThrowIfCancellationRequested();
            var lines = await File.ReadAllLinesAsync(file, ct);
            var relativePath = Path.GetRelativePath(projectPath, file);
            fileCount++;

            var context = new FileContext(lines, relativePath);

            CheckForSqlInjection(lines, relativePath, findings);
            CheckForXss(lines, relativePath, findings);
            CheckForHardcodedSecrets(lines, relativePath, findings);
            CheckForInsecureDeserialization(lines, relativePath, findings);
            CheckForMissingAuthentication(context, findings);
            CheckForInsecureCryptography(lines, relativePath, findings);
            CheckForCorsMisconfiguration(lines, relativePath, findings);
            CheckForMissingInputValidation(context, findings);
            CheckForInformationDisclosure(lines, relativePath, findings);
            CheckForInsecureHttp(lines, relativePath, findings);
            CheckForMissingHttpsRedirect(lines, relativePath, findings);
            CheckForJwtMisconfiguration(lines, relativePath, findings);
        }

        sw.Stop();
        return new AnalysisResult
        {
            AnalyzerName = Name,
            Findings = findings,
            Duration = sw.Elapsed,
            FilesScanned = fileCount
        };
    }

    // ---------------------------------------------------------------
    // SEC001 – SQL injection
    // ---------------------------------------------------------------
    private static void CheckForSqlInjection(string[] lines, string file, List<AnalysisFinding> findings)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if (RawSqlConcatRegex().IsMatch(line) || ExecuteSqlRawInterpolationRegex().IsMatch(line))
            {
                findings.Add(new AnalysisFinding
                {
                    RuleId = "SEC001",
                    Message = "Potential SQL injection",
                    Severity = FindingSeverity.Critical,
                    FilePath = file,
                    LineNumber = i + 1,
                    CodeSnippet = line.Trim(),
                    Suggestion = "Never concatenate or interpolate user input into SQL strings. " +
                                 "Use parameterized queries (FromSqlInterpolated / ExecuteSqlInterpolated) or stored procedures."
                });
            }
        }
    }

    // ---------------------------------------------------------------
    // SEC002 – XSS via Html.Raw
    // ---------------------------------------------------------------
    private static void CheckForXss(string[] lines, string file, List<AnalysisFinding> findings)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (HtmlRawRegex().IsMatch(line))
            {
                findings.Add(new AnalysisFinding
                {
                    RuleId = "SEC002",
                    Message = "Potential XSS via Html.Raw()",
                    Severity = FindingSeverity.Error,
                    FilePath = file,
                    LineNumber = i + 1,
                    CodeSnippet = line.Trim(),
                    Suggestion = "Avoid @Html.Raw() with user-supplied data. Use HTML-encoded output or a " +
                                 "sanitization library (e.g., HtmlSanitizer) if raw HTML is truly required."
                });
            }
        }
    }

    // ---------------------------------------------------------------
    // SEC003 – Hardcoded secrets
    // ---------------------------------------------------------------
    private static void CheckForHardcodedSecrets(string[] lines, string file, List<AnalysisFinding> findings)
    {
        // Skip migration and designer files
        if (file.EndsWith(".Designer.cs") || file.Contains("Migrations"))
            return;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Skip comments
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("//") || trimmed.StartsWith("*") || trimmed.StartsWith("///"))
                continue;

            if (HardcodedSecretRegex().IsMatch(line))
            {
                findings.Add(new AnalysisFinding
                {
                    RuleId = "SEC003",
                    Message = "Possible hardcoded secret",
                    Severity = FindingSeverity.Critical,
                    FilePath = file,
                    LineNumber = i + 1,
                    CodeSnippet = MaskSecretInSnippet(line.Trim()),
                    Suggestion = "Move secrets to a secure store (Azure Key Vault, AWS Secrets Manager, " +
                                 "user-secrets, or environment variables). Never commit secrets to source control."
                });
            }
        }
    }

    // ---------------------------------------------------------------
    // SEC004 – Insecure deserialization
    // ---------------------------------------------------------------
    private static void CheckForInsecureDeserialization(string[] lines, string file, List<AnalysisFinding> findings)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (InsecureDeserializationRegex().IsMatch(line))
            {
                findings.Add(new AnalysisFinding
                {
                    RuleId = "SEC004",
                    Message = "Insecure deserialization",
                    Severity = FindingSeverity.Critical,
                    FilePath = file,
                    LineNumber = i + 1,
                    CodeSnippet = line.Trim(),
                    Suggestion = "BinaryFormatter is banned in .NET 8+. Use System.Text.Json or " +
                                 "MessagePack with strict type handling. If using Newtonsoft.Json, " +
                                 "set TypeNameHandling = TypeNameHandling.None."
                });
            }
        }
    }

    // ---------------------------------------------------------------
    // SEC005 – Missing authentication on controllers
    // ---------------------------------------------------------------
    private static void CheckForMissingAuthentication(FileContext ctx, List<AnalysisFinding> findings)
    {
        if (!ctx.IsController)
            return;

        bool hasClassLevelAuth = false;
        bool hasAllowAnonymous = false;

        for (int i = 0; i < ctx.Lines.Length; i++)
        {
            var line = ctx.Lines[i];
            if (AuthorizeAttributeRegex().IsMatch(line))
                hasClassLevelAuth = true;
            if (line.Contains("[AllowAnonymous]"))
                hasAllowAnonymous = true;
        }

        if (!hasClassLevelAuth && !hasAllowAnonymous)
        {
            // Find the class declaration line
            for (int i = 0; i < ctx.Lines.Length; i++)
            {
                if (ControllerClassRegex().IsMatch(ctx.Lines[i]))
                {
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "SEC005",
                        Message = "Controller missing [Authorize] attribute",
                        Severity = FindingSeverity.Error,
                        FilePath = ctx.File,
                        LineNumber = i + 1,
                        CodeSnippet = ctx.Lines[i].Trim(),
                        Suggestion = "Add [Authorize] at the class level and use [AllowAnonymous] only on " +
                                     "specific actions that should be publicly accessible."
                    });
                    break;
                }
            }
        }
    }

    // ---------------------------------------------------------------
    // SEC006 – Insecure cryptography
    // ---------------------------------------------------------------
    private static void CheckForInsecureCryptography(string[] lines, string file, List<AnalysisFinding> findings)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (InsecureCryptoRegex().IsMatch(line))
            {
                findings.Add(new AnalysisFinding
                {
                    RuleId = "SEC006",
                    Message = "Insecure cryptographic algorithm",
                    Severity = FindingSeverity.Critical,
                    FilePath = file,
                    LineNumber = i + 1,
                    CodeSnippet = line.Trim(),
                    Suggestion = "Use SHA-256/SHA-512 for hashing, AES-256 for encryption, and BCrypt/Argon2 " +
                                 "for password hashing. MD5, SHA1, DES, and RC2 are cryptographically broken."
                });
            }
        }
    }

    // ---------------------------------------------------------------
    // SEC007 – CORS misconfiguration
    // ---------------------------------------------------------------
    private static void CheckForCorsMisconfiguration(string[] lines, string file, List<AnalysisFinding> findings)
    {
        var fullText = string.Join('\n', lines);
        if (fullText.Contains("AllowAnyOrigin") && fullText.Contains("AllowCredentials"))
        {
            // Find the AllowAnyOrigin line
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("AllowAnyOrigin"))
                {
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "SEC007",
                        Message = "CORS misconfiguration: AllowAnyOrigin with AllowCredentials",
                        Severity = FindingSeverity.Critical,
                        FilePath = file,
                        LineNumber = i + 1,
                        CodeSnippet = lines[i].Trim(),
                        Suggestion = "AllowAnyOrigin() with AllowCredentials() is forbidden by the CORS spec " +
                                     "and can expose credentials. Specify explicit allowed origins instead."
                    });
                    break;
                }
            }
        }
    }

    // ---------------------------------------------------------------
    // SEC008 – Missing input validation on controller parameters
    // ---------------------------------------------------------------
    private static void CheckForMissingInputValidation(FileContext ctx, List<AnalysisFinding> findings)
    {
        if (!ctx.IsController)
            return;

        var fullText = string.Join('\n', ctx.Lines);
        bool hasApiController = fullText.Contains("[ApiController]");

        // [ApiController] auto-validates [FromBody] models, so only flag if absent
        if (hasApiController)
            return;

        for (int i = 0; i < ctx.Lines.Length; i++)
        {
            var line = ctx.Lines[i];
            if (ActionMethodRegex().IsMatch(line) && FromBodyRegex().IsMatch(line))
            {
                // Check next few lines for ModelState validation
                bool hasValidation = false;
                int lookahead = Math.Min(ctx.Lines.Length - 1, i + 8);
                for (int j = i; j <= lookahead; j++)
                {
                    if (ctx.Lines[j].Contains("ModelState") || ctx.Lines[j].Contains("IsValid"))
                    {
                        hasValidation = true;
                        break;
                    }
                }

                if (!hasValidation)
                {
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "SEC008",
                        Message = "Missing input validation for [FromBody] parameter",
                        Severity = FindingSeverity.Warning,
                        FilePath = ctx.File,
                        LineNumber = i + 1,
                        CodeSnippet = line.Trim(),
                        Suggestion = "Add [ApiController] to enable automatic model validation, or check " +
                                     "ModelState.IsValid explicitly before processing the request."
                    });
                }
            }
        }
    }

    // ---------------------------------------------------------------
    // SEC009 – Information disclosure via exception details
    // ---------------------------------------------------------------
    private static void CheckForInformationDisclosure(string[] lines, string file, List<AnalysisFinding> findings)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (ExceptionDisclosureRegex().IsMatch(line))
            {
                // Check if it is in a return / response context
                bool inResponse = false;
                int lookback = Math.Max(0, i - 3);
                for (int j = lookback; j <= i; j++)
                {
                    if (ResponseContextRegex().IsMatch(lines[j]))
                    {
                        inResponse = true;
                        break;
                    }
                }

                if (inResponse)
                {
                    findings.Add(new AnalysisFinding
                    {
                        RuleId = "SEC009",
                        Message = "Information disclosure via exception details",
                        Severity = FindingSeverity.Error,
                        FilePath = file,
                        LineNumber = i + 1,
                        CodeSnippet = line.Trim(),
                        Suggestion = "Never expose Exception.StackTrace or Exception.ToString() in API responses. " +
                                     "Log the full exception server-side and return a generic error message to the client."
                    });
                }
            }
        }
    }

    // ---------------------------------------------------------------
    // SEC010 – Insecure HTTP URLs
    // ---------------------------------------------------------------
    private static void CheckForInsecureHttp(string[] lines, string file, List<AnalysisFinding> findings)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Skip comments and XML doc
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("//") || trimmed.StartsWith("*") || trimmed.StartsWith("///"))
                continue;

            if (InsecureHttpRegex().IsMatch(line))
            {
                // Allow localhost and well-known non-sensitive URLs
                if (line.Contains("http://localhost") || line.Contains("http://127.0.0.1")
                    || line.Contains("http://schemas") || line.Contains("http://www.w3.org"))
                    continue;

                findings.Add(new AnalysisFinding
                {
                    RuleId = "SEC010",
                    Message = "Insecure HTTP URL",
                    Severity = FindingSeverity.Warning,
                    FilePath = file,
                    LineNumber = i + 1,
                    CodeSnippet = line.Trim(),
                    Suggestion = "Use HTTPS instead of HTTP to protect data in transit. " +
                                 "Ensure all external API calls use TLS."
                });
            }
        }
    }

    // ---------------------------------------------------------------
    // SEC011 – Missing HTTPS redirect
    // ---------------------------------------------------------------
    private static void CheckForMissingHttpsRedirect(string[] lines, string file, List<AnalysisFinding> findings)
    {
        // Only check Program.cs or Startup.cs
        var fileName = Path.GetFileName(file);
        if (fileName != "Program.cs" && fileName != "Startup.cs")
            return;

        var fullText = string.Join('\n', lines);
        bool hasUseHttpsRedirection = fullText.Contains("UseHttpsRedirection");
        bool isWebApp = fullText.Contains("WebApplication") || fullText.Contains("UseRouting")
                     || fullText.Contains("MapControllers") || fullText.Contains("MapGet");

        if (isWebApp && !hasUseHttpsRedirection)
        {
            findings.Add(new AnalysisFinding
            {
                RuleId = "SEC011",
                Message = "Missing HTTPS redirect middleware",
                Severity = FindingSeverity.Error,
                FilePath = file,
                LineNumber = 1,
                CodeSnippet = "(middleware pipeline)",
                Suggestion = "Add app.UseHttpsRedirection() to the middleware pipeline to redirect " +
                             "HTTP requests to HTTPS automatically."
            });
        }
    }

    // ---------------------------------------------------------------
    // SEC012 – JWT misconfiguration
    // ---------------------------------------------------------------
    private static void CheckForJwtMisconfiguration(string[] lines, string file, List<AnalysisFinding> findings)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if (line.Contains("RequireHttpsMetadata") && line.Contains("false"))
            {
                findings.Add(new AnalysisFinding
                {
                    RuleId = "SEC012",
                    Message = "JWT RequireHttpsMetadata disabled",
                    Severity = FindingSeverity.Error,
                    FilePath = file,
                    LineNumber = i + 1,
                    CodeSnippet = line.Trim(),
                    Suggestion = "Set RequireHttpsMetadata = true in production to ensure JWT tokens are " +
                                 "only issued and validated over HTTPS."
                });
            }

            if (WeakSigningKeyRegex().IsMatch(line))
            {
                findings.Add(new AnalysisFinding
                {
                    RuleId = "SEC012",
                    Message = "Potentially weak JWT signing key",
                    Severity = FindingSeverity.Warning,
                    FilePath = file,
                    LineNumber = i + 1,
                    CodeSnippet = MaskSecretInSnippet(line.Trim()),
                    Suggestion = "Use a signing key of at least 256 bits (32 bytes). " +
                                 "Store it in a secure configuration store, not in source code."
                });
            }
        }
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------

    /// <summary>Masks potential secret values in a code snippet for safe display.</summary>
    private static string MaskSecretInSnippet(string snippet)
    {
        // Replace quoted string values longer than 4 chars with masked version
        return SecretValueRegex().Replace(snippet, m =>
        {
            var value = m.Groups[1].Value;
            return value.Length > 4
                ? $"\"{value[..2]}***{value[^2..]}\""
                : m.Value;
        });
    }

    /// <summary>Lightweight context about the file being analyzed.</summary>
    private sealed record FileContext(string[] Lines, string File)
    {
        public bool IsController { get; } =
            File.Contains("Controller", StringComparison.OrdinalIgnoreCase)
            || string.Join('\n', Lines).Contains(": ControllerBase")
            || string.Join('\n', Lines).Contains(": Controller");
    }

    // ---------------------------------------------------------------
    // Compiled Regex patterns
    // ---------------------------------------------------------------

    [GeneratedRegex(@"(\$""|""[^""]*\+).*\b(SELECT|INSERT|UPDATE|DELETE|DROP|FROM|WHERE)\b", RegexOptions.IgnoreCase)]
    private static partial Regex RawSqlConcatRegex();

    [GeneratedRegex(@"ExecuteSqlRaw\s*\(\s*\$""")]
    private static partial Regex ExecuteSqlRawInterpolationRegex();

    [GeneratedRegex(@"Html\.Raw\s*\(")]
    private static partial Regex HtmlRawRegex();

    [GeneratedRegex(@"(password|secret|apikey|api_key|connectionstring|token|private.?key)\s*=\s*""[^""]{4,}""", RegexOptions.IgnoreCase)]
    private static partial Regex HardcodedSecretRegex();

    [GeneratedRegex(@"\b(BinaryFormatter|TypeNameHandling\s*\.\s*(All|Auto|Objects|Arrays))")]
    private static partial Regex InsecureDeserializationRegex();

    [GeneratedRegex(@"\[(Authorize|AllowAnonymous)\b")]
    private static partial Regex AuthorizeAttributeRegex();

    [GeneratedRegex(@"class\s+\w+.*Controller\b")]
    private static partial Regex ControllerClassRegex();

    [GeneratedRegex(@"\b(MD5|SHA1)\.(Create|ComputeHash)|new\s+(DESCryptoServiceProvider|RC2CryptoServiceProvider|TripleDESCryptoServiceProvider)")]
    private static partial Regex InsecureCryptoRegex();

    [GeneratedRegex(@"public\s+(async\s+)?.*\b(IActionResult|ActionResult|Task<.*Result>)\b.*\(")]
    private static partial Regex ActionMethodRegex();

    [GeneratedRegex(@"\[FromBody\]")]
    private static partial Regex FromBodyRegex();

    [GeneratedRegex(@"\.(StackTrace|ToString\(\))\b")]
    private static partial Regex ExceptionDisclosureRegex();

    [GeneratedRegex(@"\b(return|Ok\(|BadRequest\(|StatusCode\(|Content\(|Json\(|new\s+\w*Response)")]
    private static partial Regex ResponseContextRegex();

    [GeneratedRegex(@"""http://[^""]+""")]
    private static partial Regex InsecureHttpRegex();

    [GeneratedRegex(@"SymmetricSecurityKey\s*\(\s*Encoding\.\w+\.GetBytes\s*\(\s*""[^""]{1,15}""")]
    private static partial Regex WeakSigningKeyRegex();

    [GeneratedRegex(@"""([^""]+)""")]
    private static partial Regex SecretValueRegex();
}
