namespace RVR.Framework.Domain.Entities.Configuration;

/// <summary>
/// Paramètre de configuration du système
/// </summary>
public class Setting : Entity<Guid>
{
    /// <summary>
    /// Identifiant du tenant (null pour paramètres globaux)
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Nom du paramètre
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Valeur du paramètre
    /// </summary>
    public string? Value { get; private set; }

    /// <summary>
    /// Type de fournisseur (Global, Tenant, User)
    /// </summary>
    public string? ProviderName { get; private set; }

    /// <summary>
    /// Clé du fournisseur
    /// </summary>
    public string? ProviderKey { get; private set; }

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private Setting() { }

    /// <summary>
    /// Crée un nouveau paramètre
    /// </summary>
    /// <param name="name">Nom du paramètre</param>
    /// <param name="value">Valeur du paramètre</param>
    /// <param name="tenantId">Identifiant du tenant</param>
    /// <param name="providerName">Type de fournisseur</param>
    /// <param name="providerKey">Clé du fournisseur</param>
    public Setting(string name, string? value, Guid? tenantId = null, string? providerName = null, string? providerKey = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom du paramètre ne peut pas être vide.", nameof(name));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Value = value?.Trim();
        TenantId = tenantId;
        ProviderName = providerName?.Trim();
        ProviderKey = providerKey?.Trim();
    }

    /// <summary>
    /// Met à jour la valeur du paramètre
    /// </summary>
    public void UpdateValue(string? value)
    {
        Value = value?.Trim();
    }
}
