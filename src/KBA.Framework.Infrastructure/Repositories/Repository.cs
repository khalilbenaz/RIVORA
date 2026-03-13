using EFCore.BulkExtensions;
using KBA.Framework.Core.Specifications;
using KBA.Framework.Domain.Entities;
using KBA.Framework.Domain.Repositories;
using KBA.Framework.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KBA.Framework.Infrastructure.Repositories;

/// <summary>
/// Implémentation générique du repository
/// </summary>
/// <typeparam name="TEntity">Type de l'entité</typeparam>
/// <typeparam name="TKey">Type de la clé primaire</typeparam>
public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : Entity<TKey>
{
    protected readonly KBADbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Constructeur
    /// </summary>
    public Repository(KBADbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        // FindAsync utilise le cache de premier niveau, pas de AsNoTracking nécessaire
        return await _dbSet.FindAsync(new object[] { id! }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Récupère une entité par son ID sans tracking (lecture seule)
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsNoTrackingAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default)
    {
        // Utiliser AsNoTracking pour les requêtes en lecture seule améliore les performances
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<List<TEntity>> GetListAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).ToListAsync(cancellationToken);
    }

    protected IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec)
    {
        return SpecificationEvaluator<TEntity, TKey>.GetQuery(_dbSet.AsQueryable(), spec);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task BulkInsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _context.BulkInsertAsync(entities.ToList(), cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task BulkUpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _context.BulkUpdateAsync(entities.ToList(), cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task BulkDeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _context.BulkDeleteAsync(entities.ToList(), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Récupère une liste paginée d'entités
    /// </summary>
    public virtual async Task<List<TEntity>> GetPagedListAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Vérifie si une entité existe
    /// </summary>
    public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.Id!.Equals(id), cancellationToken);
    }
}
