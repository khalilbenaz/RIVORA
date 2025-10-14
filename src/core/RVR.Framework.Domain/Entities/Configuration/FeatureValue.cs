namespace RVR.Framework.Domain.Entities.Configuration;

/// <summary>
/// Valeur d'une fonctionnalité pour un tenant ou utilisateur
/// </summary>
public class FeatureValue : Entity<Guid>
{
    /// <summary>
    /// Identifiant du tenant
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Nom de la fonctionnalité
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Valeur de la fonctionnalité
    /// </summary>
    public string? Value { get; private set; }

    /// <summary>
    /// Type de fournisseur (Tenant, Edition, etc.)
    /// </summary>
    public string? ProviderName { get; private set; }

    /// <summary>
    /// Clé du fournisseur
    /// </summary>
    public string? ProviderKey { get; private set; }

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private FeatureValue() { }

    /// <summary>
    /// Crée une nouvelle valeur de fonctionnalité
    /// </summary>
    /// <param name="name">Nom de la fonctionnalité</param>
    /// <param name="value">Valeur</param>
    /// <param name="tenantId">Identifiant du tenant</param>
    /// <param name="providerName">Type de fournisseur</param>
    /// <param name="providerKey">Clé du fournisseur</param>
    public FeatureValue(string name, string? value, Guid? tenantId = null, string? providerName = null, string? providerKey = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom de la fonctionnalité ne peut pas être vide.", nameof(name));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Value = value?.Trim();
        TenantId = tenantId;
        ProviderName = providerName?.Trim();
        ProviderKey = providerKey?.Trim();
    }

    /// <summary>
    /// Met à jour la valeur
    /// </summary>
    public void UpdateValue(string? value)
    {
        Value = value?.Trim();
    }
}
