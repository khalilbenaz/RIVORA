using RVR.Framework.Application.DTOs.Products;

namespace RVR.Framework.Application.Services;

/// <summary>
/// Interface du service de gestion des produits
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Récupère un produit par son identifiant
    /// </summary>
    /// <param name="id">Identifiant du produit</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Le produit ou null</returns>
    Task<ProductDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère la liste de tous les produits
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste des produits</returns>
    Task<List<ProductDto>> GetListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère la liste des produits actifs
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste des produits actifs</returns>
    Task<List<ProductDto>> GetActiveListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Crée un nouveau produit
    /// </summary>
    /// <param name="dto">Données de création</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Le produit créé</returns>
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour un produit
    /// </summary>
    /// <param name="id">Identifiant du produit</param>
    /// <param name="dto">Données de mise à jour</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Le produit mis à jour</returns>
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime un produit
    /// </summary>
    /// <param name="id">Identifiant du produit</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recherche des produits par nom
    /// </summary>
    /// <param name="name">Nom à rechercher</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste des produits correspondants</returns>
    Task<List<ProductDto>> SearchByNameAsync(string name, CancellationToken cancellationToken = default);
}
