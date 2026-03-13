using KBA.Framework.Domain.Entities;
using KBA.Framework.Core.Specifications;

namespace KBA.Framework.Domain.Repositories;

/// <summary>
/// Interface de repository générique
/// </summary>
/// <typeparam name="TEntity">Type de l'entité</typeparam>
/// <typeparam name="TKey">Type de la clé primaire</typeparam>
public interface IRepository<TEntity, TKey> where TEntity : Entity<TKey>
{
    /// <summary>
    /// Récupère une entité par son identifiant
    /// </summary>
    /// <param name="id">Identifiant</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>L'entité ou null si non trouvée</returns>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère une entité selon une spécification
    /// </summary>
    Task<TEntity?> GetAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère une liste d'entités
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste des entités</returns>
    Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère une liste d'entités selon une spécification
    /// </summary>
    Task<List<TEntity>> GetListAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);

    /// <summary>
    /// Insère une nouvelle entité
    /// </summary>
    /// <param name="entity">Entité à insérer</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>L'entité insérée</returns>
    Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour une entité
    /// </summary>
    /// <param name="entity">Entité à mettre à jour</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>L'entité mise à jour</returns>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime une entité
    /// </summary>
    /// <param name="entity">Entité à supprimer</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compte le nombre d'entités
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Nombre d'entités</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Insère un lot d'entités de manière performante
    /// </summary>
    Task BulkInsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour un lot d'entités de manière performante
    /// </summary>
    Task BulkUpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime un lot d'entités de manière performante
    /// </summary>
    Task BulkDeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
