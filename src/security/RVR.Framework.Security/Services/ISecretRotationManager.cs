namespace RVR.Framework.Security.Services;

/// <summary>
/// Service pour la rotation automatique des secrets (4.7)
/// </summary>
public interface ISecretRotationManager
{
    /// <summary>
    /// Récupère le secret actuel pour une clé donnée
    /// </summary>
    Task<string> GetCurrentSecretAsync(string keyName);

    /// <summary>
    /// Force la rotation d'un secret
    /// </summary>
    Task RotateSecretAsync(string keyName);

    /// <summary>
    /// Récupère tous les secrets valides (actuel + précédents pour la période de grâce)
    /// </summary>
    Task<IEnumerable<string>> GetValidSecretsAsync(string keyName);
}
