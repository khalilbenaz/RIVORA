namespace RVR.Framework.Domain.Entities.MultiTenancy;

/// <summary>
/// Chaîne de connexion spécifique à un tenant
/// </summary>
public class TenantConnectionString : Entity<Guid>
{
    /// <summary>
    /// Identifiant du tenant
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Nom de la chaîne de connexion (ex: Default, Reporting, etc.)
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Chaîne de connexion
    /// </summary>
    public string Value { get; private set; } = string.Empty;

    /// <summary>
    /// Tenant associé
    /// </summary>
    public virtual Tenant Tenant { get; private set; } = null!;

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private TenantConnectionString() { }

    /// <summary>
    /// Crée une nouvelle chaîne de connexion pour un tenant
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant</param>
    /// <param name="name">Nom de la chaîne de connexion</param>
    /// <param name="value">Valeur de la chaîne de connexion</param>
    public TenantConnectionString(Guid tenantId, string name, string value)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("L'identifiant du tenant ne peut pas être vide.", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom de la chaîne de connexion ne peut pas être vide.", nameof(name));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("La valeur de la chaîne de connexion ne peut pas être vide.", nameof(value));

        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name.Trim();
        Value = value.Trim();
    }

    /// <summary>
    /// Met à jour la valeur de la chaîne de connexion
    /// </summary>
    /// <param name="value">Nouvelle valeur</param>
    public void UpdateValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("La valeur de la chaîne de connexion ne peut pas être vide.", nameof(value));

        Value = value.Trim();
    }
}
