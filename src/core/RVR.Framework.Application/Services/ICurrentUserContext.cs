namespace RVR.Framework.Application.Services;

/// <summary>
/// Interface pour accéder au contexte de l'utilisateur connecté
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>
    /// Obtient le TenantId de l'utilisateur connecté
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Obtient l'UserId de l'utilisateur connecté
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Obtient le nom d'utilisateur de l'utilisateur connecté
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Indique si un utilisateur est connecté
    /// </summary>
    bool IsAuthenticated { get; }
}
