using RVR.Framework.Application.DTOs.Tenants;

namespace RVR.Framework.Application.Services;

/// <summary>
/// Interface du service de gestion des tenants
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Récupère un tenant par son identifiant
    /// </summary>
    /// <param name="id">Identifiant du tenant</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Le tenant ou null</returns>
    Task<TenantDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère la liste de tous les tenants
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste des tenants</returns>
    Task<List<TenantDto>> GetListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Crée un nouveau tenant
    /// </summary>
    /// <param name="dto">Données de création</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Le tenant créé</returns>
    Task<TenantDto> CreateAsync(CreateTenantDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime un tenant
    /// </summary>
    /// <param name="id">Identifiant du tenant</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
