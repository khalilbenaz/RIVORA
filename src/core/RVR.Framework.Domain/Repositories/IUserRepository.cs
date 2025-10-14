using RVR.Framework.Domain.Entities.Identity;

namespace RVR.Framework.Domain.Repositories;

/// <summary>
/// Repository pour les utilisateurs
/// </summary>
public interface IUserRepository : IRepository<User, Guid>
{
    /// <summary>
    /// Récupère un utilisateur par nom d'utilisateur
    /// </summary>
    /// <param name="userName">Nom d'utilisateur</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>L'utilisateur ou null</returns>
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère un utilisateur par email
    /// </summary>
    /// <param name="email">Email</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>L'utilisateur ou null</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
