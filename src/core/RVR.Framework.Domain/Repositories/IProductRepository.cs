using RVR.Framework.Domain.Entities.Products;

namespace RVR.Framework.Domain.Repositories;

/// <summary>
/// Repository pour les produits
/// </summary>
public interface IProductRepository : IRepository<Product, Guid>
{
    /// <summary>
    /// Récupère les produits actifs
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste des produits actifs</returns>
    Task<List<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Recherche des produits par nom
    /// </summary>
    /// <param name="name">Nom à rechercher</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste des produits correspondants</returns>
    Task<List<Product>> SearchByNameAsync(string name, CancellationToken cancellationToken = default);
}
