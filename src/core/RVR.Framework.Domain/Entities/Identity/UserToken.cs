namespace RVR.Framework.Domain.Entities.Identity;

/// <summary>
/// Token d'authentification d'un utilisateur
/// </summary>
public class UserToken : Entity<Guid>
{
    /// <summary>
    /// Identifiant du tenant
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Identifiant de l'utilisateur
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Fournisseur de login
    /// </summary>
    public string LoginProvider { get; private set; } = string.Empty;

    /// <summary>
    /// Nom du token
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Valeur du token
    /// </summary>
    public string? Value { get; private set; }

    /// <summary>
    /// Utilisateur
    /// </summary>
    public virtual User User { get; private set; } = null!;

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private UserToken() { }

    /// <summary>
    /// Crée un nouveau token utilisateur
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant</param>
    /// <param name="userId">Identifiant de l'utilisateur</param>
    /// <param name="loginProvider">Fournisseur de login</param>
    /// <param name="name">Nom du token</param>
    /// <param name="value">Valeur du token</param>
    public UserToken(Guid? tenantId, Guid userId, string loginProvider, string name, string? value = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("L'identifiant de l'utilisateur ne peut pas être vide.", nameof(userId));

        if (string.IsNullOrWhiteSpace(loginProvider))
            throw new ArgumentException("Le fournisseur de login ne peut pas être vide.", nameof(loginProvider));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom du token ne peut pas être vide.", nameof(name));

        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = userId;
        LoginProvider = loginProvider.Trim();
        Name = name.Trim();
        Value = value?.Trim();
    }
}
