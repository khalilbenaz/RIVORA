using System.Globalization;
using Microsoft.Extensions.Localization;

namespace RVR.Framework.Localization.Dynamic.Services;

/// <summary>
/// An <see cref="IStringLocalizer"/> implementation that reads translations
/// from an <see cref="ILocalizationStore"/> (database-backed or in-memory).
/// </summary>
public class DatabaseStringLocalizer : IStringLocalizer
{
    private readonly ILocalizationStore _store;
    private readonly string _culture;

    /// <summary>
    /// Creates a new DatabaseStringLocalizer for the current UI culture.
    /// </summary>
    /// <param name="store">The localization store to read translations from.</param>
    public DatabaseStringLocalizer(ILocalizationStore store)
        : this(store, CultureInfo.CurrentUICulture.Name)
    {
    }

    /// <summary>
    /// Creates a new DatabaseStringLocalizer for a specific culture.
    /// </summary>
    /// <param name="store">The localization store.</param>
    /// <param name="culture">The culture code.</param>
    public DatabaseStringLocalizer(ILocalizationStore store, string culture)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _culture = culture;
    }

    /// <inheritdoc />
    public LocalizedString this[string name]
    {
        get
        {
            var value = _store.GetTranslationAsync(_culture, name)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            return new LocalizedString(name, value ?? name, resourceNotFound: value is null);
        }
    }

    /// <inheritdoc />
    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var value = _store.GetTranslationAsync(_culture, name)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            var formatted = value is not null
                ? string.Format(CultureInfo.CurrentCulture, value, arguments)
                : string.Format(CultureInfo.CurrentCulture, name, arguments);

            return new LocalizedString(name, formatted, resourceNotFound: value is null);
        }
    }

    /// <inheritdoc />
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var entries = _store.GetAllForCultureAsync(_culture)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        foreach (var entry in entries)
        {
            yield return new LocalizedString(entry.Key, entry.Value, resourceNotFound: false);
        }

        if (includeParentCultures)
        {
            var parentCulture = CultureInfo.GetCultureInfo(_culture).Parent;
            if (!string.IsNullOrEmpty(parentCulture.Name))
            {
                var parentEntries = _store.GetAllForCultureAsync(parentCulture.Name)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                // Only return parent entries whose keys are not already in the child culture
                var childKeys = entries.Select(e => e.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var entry in parentEntries)
                {
                    if (!childKeys.Contains(entry.Key))
                    {
                        yield return new LocalizedString(entry.Key, entry.Value, resourceNotFound: false);
                    }
                }
            }
        }
    }
}

/// <summary>
/// A typed <see cref="IStringLocalizer{T}"/> backed by the dynamic localization store.
/// </summary>
/// <typeparam name="T">The type to provide localization for.</typeparam>
public class DatabaseStringLocalizer<T> : IStringLocalizer<T>
{
    private readonly DatabaseStringLocalizer _inner;

    public DatabaseStringLocalizer(ILocalizationStore store)
    {
        _inner = new DatabaseStringLocalizer(store);
    }

    /// <inheritdoc />
    public LocalizedString this[string name] => _inner[name];

    /// <inheritdoc />
    public LocalizedString this[string name, params object[] arguments] => _inner[name, arguments];

    /// <inheritdoc />
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        _inner.GetAllStrings(includeParentCultures);
}
