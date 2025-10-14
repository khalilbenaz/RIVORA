using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;
using EFCore.BulkExtensions;
using RVR.Framework.Core.Pagination;
using RVR.Framework.Core.Specifications;
using RVR.Framework.Domain.Entities;
using RVR.Framework.Domain.Repositories;
using RVR.Framework.Infrastructure.Data;
using RVR.Framework.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace RVR.Framework.Infrastructure.Repositories;

/// <summary>
/// Implémentation générique du repository
/// </summary>
/// <typeparam name="TEntity">Type de l'entité</typeparam>
/// <typeparam name="TKey">Type de la clé primaire</typeparam>
public class Repository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : Entity<TKey>
{
    protected readonly RVRDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    private readonly ITenantProvider? _tenantProvider;

    /// <summary>
    /// Constructeur
    /// </summary>
    public Repository(RVRDbContext context, ITenantProvider? tenantProvider = null)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
        _tenantProvider = tenantProvider;

        // For tenant-scoped entities, ensure a TenantId is set in the current context
        EnsureTenantContext();
    }

    /// <summary>
    /// Ensures that a TenantId is available in the current context for tenant-scoped entities.
    /// Throws <see cref="InvalidOperationException"/> if the entity implements <see cref="ITenantId"/>
    /// but no tenant is set.
    /// </summary>
    private void EnsureTenantContext()
    {
        if (typeof(ITenantId).IsAssignableFrom(typeof(TEntity)))
        {
            var tenantInfo = _tenantProvider?.GetCurrentTenant();
            if (tenantInfo == null || string.IsNullOrEmpty(tenantInfo.Id))
            {
                throw new InvalidOperationException(
                    $"A TenantId must be set in the current context to access tenant-scoped entity '{typeof(TEntity).Name}'. " +
                    "Ensure the tenant middleware is configured and the request contains valid tenant information.");
            }
        }
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
        return entity;
    }

    /// <inheritdoc />
    public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
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

    /// <inheritdoc />
    public virtual async Task<PagedResult<TEntity>> GetPagedAsync(
        CursorPageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var sortProperty = typeof(TEntity).GetProperty(request.SortBy)
            ?? typeof(TEntity).GetProperty("Id")
            ?? throw new InvalidOperationException($"Sort property '{request.SortBy}' not found on {typeof(TEntity).Name}.");

        // Build the sort key expression: entity => entity.<SortBy>
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var propertyAccess = Expression.Property(parameter, sortProperty);
        var keySelector = Expression.Lambda(propertyAccess, parameter);

        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        // Apply cursor filter (keyset pagination)
        if (!string.IsNullOrEmpty(request.Cursor))
        {
            var cursorValue = DecodeCursor(request.Cursor, sortProperty.PropertyType);
            if (cursorValue is not null)
            {
                // Build: entity => entity.<SortBy> > cursorValue (or < for descending)
                var constant = Expression.Constant(cursorValue, sortProperty.PropertyType);
                var comparison = request.Descending
                    ? Expression.LessThan(propertyAccess, constant)
                    : Expression.GreaterThan(propertyAccess, constant);
                var predicate = Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
                query = query.Where(predicate);
            }
        }

        // Get total count (without cursor filter for accurate total)
        var totalCount = await _dbSet.CountAsync(cancellationToken);

        // Apply ordering using the dynamic key selector
        var orderByMethod = request.Descending ? "OrderByDescending" : "OrderBy";
        var orderByCall = Expression.Call(
            typeof(Queryable),
            orderByMethod,
            new[] { typeof(TEntity), sortProperty.PropertyType },
            query.Expression,
            Expression.Quote(keySelector));
        query = query.Provider.CreateQuery<TEntity>(orderByCall);

        // Fetch one extra to determine if there are more pages
        var items = await query
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > pageSize;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        // Build cursors from the sort property of first/last items
        string? nextCursor = null;
        string? previousCursor = null;

        if (items.Count > 0)
        {
            var lastValue = sortProperty.GetValue(items[^1]);
            if (lastValue is not null && hasMore)
            {
                nextCursor = EncodeCursor(lastValue);
            }

            var firstValue = sortProperty.GetValue(items[0]);
            if (firstValue is not null && request.Cursor is not null)
            {
                previousCursor = EncodeCursor(firstValue);
            }
        }

        return new PagedResult<TEntity>
        {
            Items = items.AsReadOnly(),
            NextCursor = nextCursor,
            PreviousCursor = previousCursor,
            HasMore = hasMore,
            TotalCount = totalCount
        };
    }

    private static string EncodeCursor(object value)
    {
        var raw = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    private static object? DecodeCursor(string cursor, Type targetType)
    {
        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            if (targetType == typeof(Guid))
                return Guid.Parse(raw);
            if (targetType == typeof(DateTime))
                return DateTime.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);
            if (targetType == typeof(int))
                return int.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);
            if (targetType == typeof(long))
                return long.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);
            if (targetType == typeof(string))
                return raw;
            return Convert.ChangeType(raw, targetType, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }
}
