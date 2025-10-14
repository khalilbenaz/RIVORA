using RVR.Framework.Application.DTOs.Users;

namespace RVR.Framework.Application.Services;

/// <summary>
/// Interface du service de gestion des utilisateurs
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Récupère un utilisateur par son identifiant
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>L'utilisateur ou null</returns>
    Task<UserDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère la liste de tous les utilisateurs
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste des utilisateurs</returns>
    Task<List<UserDto>> GetListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Crée un nouveau utilisateur
    /// </summary>
    /// <param name="dto">Données de création</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>L'utilisateur créé</returns>
    Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour un utilisateur
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur</param>
    /// <param name="dto">Données de mise à jour</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>L'utilisateur mis à jour</returns>
    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime un utilisateur
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère un utilisateur par nom d'utilisateur
    /// </summary>
    /// <param name="userName">Nom d'utilisateur</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>L'utilisateur ou null</returns>
    Task<UserDto?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
}
