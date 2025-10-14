using System.Reflection;

namespace RVR.Framework.Export;

/// <summary>
/// Utility for resolving and filtering properties based on <see cref="ExportOptions"/>.
/// </summary>
internal static class PropertyHelper
{
    /// <summary>
    /// Gets the public instance properties of <typeparamref name="T"/> filtered
    /// by the include/exclude rules in the given <see cref="ExportOptions"/>.
    /// </summary>
    public static PropertyInfo[] GetFilteredProperties<T>(ExportOptions options)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        if (options.IncludeColumns is { Length: > 0 })
        {
            var includeSet = new HashSet<string>(options.IncludeColumns, StringComparer.OrdinalIgnoreCase);
            properties = properties
                .Where(p => includeSet.Contains(p.Name))
                .OrderBy(p => Array.IndexOf(options.IncludeColumns!, p.Name))
                .ToArray();
        }
        else if (options.ExcludeColumns is { Length: > 0 })
        {
            var excludeSet = new HashSet<string>(options.ExcludeColumns, StringComparer.OrdinalIgnoreCase);
            properties = properties
                .Where(p => !excludeSet.Contains(p.Name))
                .ToArray();
        }

        return properties;
    }
}
