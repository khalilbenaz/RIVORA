using RVR.Framework.Domain.Entities.Organization;

namespace RVR.Framework.Domain.Entities.Identity;

/// <summary>
/// Représente un utilisateur dans le système
/// </summary>
public class User : FullAuditedEntity<Guid>
{
    /// <summary>
    /// Identifiant du tenant (null pour les utilisateurs host)
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Nom d'utilisateur
    /// </summary>
    public string UserName { get; private set; } = string.Empty;

    /// <summary>
    /// Nom d'utilisateur normalisé
    /// </summary>
    public string NormalizedUserName { get; private set; } = string.Empty;

    /// <summary>
    /// Adresse email
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Email normalisé
    /// </summary>
    public string NormalizedEmail { get; private set; } = string.Empty;

    /// <summary>
    /// Hash du mot de passe
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Indique si l'email est confirmé
    /// </summary>
    public bool EmailConfirmed { get; private set; }

    /// <summary>
    /// Numéro de téléphone
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// Indique si le numéro de téléphone est confirmé
    /// </summary>
    public bool PhoneNumberConfirmed { get; private set; }

    /// <summary>
    /// Indique si l'authentification à deux facteurs est activée
    /// </summary>
    public bool TwoFactorEnabled { get; private set; }

    /// <summary>
    /// Date et heure de fin de verrouillage
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; private set; }

    /// <summary>
    /// Date et heure UTC de fin de verrouillage (pour le système de lockout anti brute-force)
    /// </summary>
    public DateTime? LockoutEndUtc { get; private set; }

    /// <summary>
    /// Indique si le verrouillage est activé
    /// </summary>
    public bool LockoutEnabled { get; private set; }

    /// <summary>
    /// Nombre d'échecs d'accès
    /// </summary>
    public int AccessFailedCount { get; private set; }

    /// <summary>
    /// Nombre de tentatives de connexion échouées consécutives (anti brute-force)
    /// </summary>
    public int FailedLoginAttempts { get; private set; }

    /// <summary>
    /// Indique si l'utilisateur est actif
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Prénom
    /// </summary>
    public string? FirstName { get; private set; }

    /// <summary>
    /// Nom de famille
    /// </summary>
    public string? LastName { get; private set; }

    /// <summary>
    /// Nom complet
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Rôles de l'utilisateur
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    /// <summary>
    /// Claims de l'utilisateur
    /// </summary>
    public virtual ICollection<UserClaim> UserClaims { get; private set; } = new List<UserClaim>();

    /// <summary>
    /// Logins externes de l'utilisateur
    /// </summary>
    public virtual ICollection<UserLogin> UserLogins { get; private set; } = new List<UserLogin>();

    /// <summary>
    /// Tokens de l'utilisateur
    /// </summary>
    public virtual ICollection<UserToken> UserTokens { get; private set; } = new List<UserToken>();

    /// <summary>
    /// Unités organisationnelles de l'utilisateur
    /// </summary>
    public virtual ICollection<UserOrganizationUnit> UserOrganizationUnits { get; private set; } = new List<UserOrganizationUnit>();

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private User() { }

    /// <summary>
    /// Crée un nouveau utilisateur
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant</param>
    /// <param name="userName">Nom d'utilisateur</param>
    /// <param name="email">Adresse email</param>
    /// <param name="userId">Identifiant de l'utilisateur créateur</param>
    public User(Guid? tenantId, string userName, string email, Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Le nom d'utilisateur ne peut pas être vide.", nameof(userName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("L'email ne peut pas être vide.", nameof(email));

        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserName = userName.Trim();
        NormalizedUserName = userName.Trim().ToUpperInvariant();
        Email = email.Trim();
        NormalizedEmail = email.Trim().ToUpperInvariant();
        IsActive = true;
        LockoutEnabled = true;
        SetCreationInfo(userId);
    }

    /// <summary>
    /// Définit le hash du mot de passe
    /// </summary>
    /// <param name="passwordHash">Hash du mot de passe</param>
    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash ?? string.Empty;
    }

    /// <summary>
    /// Confirme l'email
    /// </summary>
    public void ConfirmEmail()
    {
        EmailConfirmed = true;
    }

    /// <summary>
    /// Confirme le numéro de téléphone
    /// </summary>
    public void ConfirmPhoneNumber()
    {
        PhoneNumberConfirmed = true;
    }

    /// <summary>
    /// Active l'authentification à deux facteurs
    /// </summary>
    public void EnableTwoFactor()
    {
        TwoFactorEnabled = true;
    }

    /// <summary>
    /// Désactive l'authentification à deux facteurs
    /// </summary>
    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
    }

    /// <summary>
    /// Active l'utilisateur
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur qui active</param>
    public void Activate(Guid? userId = null)
    {
        IsActive = true;
        SetModificationInfo(userId);
    }

    /// <summary>
    /// Désactive l'utilisateur
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur qui désactive</param>
    public void Deactivate(Guid? userId = null)
    {
        IsActive = false;
        SetModificationInfo(userId);
    }

    /// <summary>
    /// Met à jour les informations personnelles
    /// </summary>
    /// <param name="firstName">Prénom</param>
    /// <param name="lastName">Nom de famille</param>
    /// <param name="phoneNumber">Numéro de téléphone</param>
    /// <param name="userId">Identifiant de l'utilisateur qui modifie</param>
    public void UpdatePersonalInfo(string? firstName, string? lastName, string? phoneNumber, Guid? userId = null)
    {
        FirstName = firstName?.Trim();
        LastName = lastName?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        SetModificationInfo(userId);
    }

    /// <summary>
    /// Incrémente le compteur de tentatives de connexion échouées
    /// </summary>
    public void IncrementFailedLogins()
    {
        FailedLoginAttempts++;
    }

    /// <summary>
    /// Réinitialise le compteur de tentatives de connexion échouées et la date de verrouillage
    /// </summary>
    public void ResetFailedLogins()
    {
        FailedLoginAttempts = 0;
        LockoutEndUtc = null;
    }

    /// <summary>
    /// Vérifie si le compte est actuellement verrouillé
    /// </summary>
    /// <returns>True si le compte est verrouillé</returns>
    public bool IsLockedOut()
    {
        return LockoutEndUtc.HasValue && LockoutEndUtc.Value > DateTime.UtcNow;
    }

    /// <summary>
    /// Verrouille le compte jusqu'à la date spécifiée
    /// </summary>
    /// <param name="until">Date et heure UTC de fin de verrouillage</param>
    public void LockUntil(DateTime until)
    {
        LockoutEndUtc = until;
    }
}
