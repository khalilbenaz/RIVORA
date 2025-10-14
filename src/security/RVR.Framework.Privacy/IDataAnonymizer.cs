namespace RVR.Framework.Privacy;

/// <summary>
/// Service responsible for anonymizing personal data in entity objects.
/// Uses reflection to find properties marked with <see cref="Attributes.PersonalDataAttribute"/>
/// and replaces their values with anonymized equivalents.
/// </summary>
public interface IDataAnonymizer
{
    /// <summary>
    /// Anonymizes all properties marked with <see cref="Attributes.PersonalDataAttribute"/>
    /// or <see cref="Attributes.SensitiveDataAttribute"/> on the given entity.
    /// </summary>
    /// <typeparam name="T">The type of entity to anonymize.</typeparam>
    /// <param name="entity">The entity whose personal data should be anonymized.</param>
    /// <returns>The entity with personal data replaced by anonymized values.</returns>
    T AnonymizeEntity<T>(T entity) where T : class;
}
