namespace RVR.Framework.Privacy.Services;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using RVR.Framework.Privacy.Attributes;

/// <summary>
/// Default implementation of <see cref="IDataAnonymizer"/> that uses reflection
/// to find and replace personal data properties with anonymized values.
/// </summary>
public class DataAnonymizer : IDataAnonymizer
{
    private readonly ILogger<DataAnonymizer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataAnonymizer"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public DataAnonymizer(ILogger<DataAnonymizer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public T AnonymizeEntity<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T entity) where T : class
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        var type = entity.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanWrite)
            {
                continue;
            }

            var isPersonalData = property.GetCustomAttribute<PersonalDataAttribute>() != null;
            var isSensitiveData = property.GetCustomAttribute<SensitiveDataAttribute>() != null;

            if (!isPersonalData && !isSensitiveData)
            {
                continue;
            }

            var anonymizedValue = GetAnonymizedValue(property);
            if (anonymizedValue != null || !property.PropertyType.IsValueType)
            {
                property.SetValue(entity, anonymizedValue);
                _logger.LogDebug(
                    "Anonymized property {PropertyName} on entity type {EntityType}",
                    property.Name,
                    type.Name);
            }
        }

        return entity;
    }

    private static object? GetAnonymizedValue(PropertyInfo property)
    {
        var propertyType = property.PropertyType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType);

        if (underlyingType != null)
        {
            // For nullable types, return null
            return null;
        }

        if (propertyType == typeof(string))
        {
            return GetAnonymizedString(property.Name);
        }

        if (propertyType == typeof(DateTime))
        {
            return DateTime.MinValue;
        }

        if (propertyType == typeof(DateTimeOffset))
        {
            return DateTimeOffset.MinValue;
        }

        if (propertyType == typeof(int) || propertyType == typeof(long))
        {
            return Convert.ChangeType(0, propertyType);
        }

        if (propertyType == typeof(bool))
        {
            return false;
        }

        if (propertyType == typeof(Guid))
        {
            return Guid.Empty;
        }

        // For other reference types, return null
        if (!propertyType.IsValueType)
        {
            return null;
        }

        // For unknown value types, return default (AOT-safe)
        return RuntimeHelpers.GetUninitializedObject(propertyType);
    }

    private static string GetAnonymizedString(string propertyName)
    {
        var lowerName = propertyName.ToLowerInvariant();

        if (lowerName.Contains("email"))
        {
            return "anonymized@example.com";
        }

        if (lowerName.Contains("phone") || lowerName.Contains("mobile") || lowerName.Contains("fax"))
        {
            return "***-***-****";
        }

        if (lowerName.Contains("address") || lowerName.Contains("street") || lowerName.Contains("city") ||
            lowerName.Contains("zip") || lowerName.Contains("postal"))
        {
            return "*** ANONYMIZED ***";
        }

        if (lowerName.Contains("name") || lowerName.Contains("first") || lowerName.Contains("last") ||
            lowerName.Contains("surname"))
        {
            return "***";
        }

        if (lowerName.Contains("ip"))
        {
            return "0.0.0.0";
        }

        if (lowerName.Contains("ssn") || lowerName.Contains("social") || lowerName.Contains("national"))
        {
            return "***-**-****";
        }

        return "***";
    }
}
