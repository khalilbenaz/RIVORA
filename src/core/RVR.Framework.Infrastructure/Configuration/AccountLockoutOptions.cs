namespace RVR.Framework.Infrastructure.Configuration;

/// <summary>
/// Options de configuration pour le verrouillage de compte (anti brute-force)
/// </summary>
public class AccountLockoutOptions
{
    /// <summary>
    /// Nom de la section dans appsettings.json
    /// </summary>
    public const string SectionName = "AccountLockout";

    /// <summary>
    /// Nombre maximum de tentatives échouées avant verrouillage
    /// </summary>
    public int MaxFailedAttempts { get; set; } = 5;

    /// <summary>
    /// Durée du verrouillage en minutes
    /// </summary>
    public int LockoutDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Indique si le verrouillage de compte est activé
    /// </summary>
    public bool EnableLockout { get; set; } = true;
}
