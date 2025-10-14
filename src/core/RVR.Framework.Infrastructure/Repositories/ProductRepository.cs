// ---------------------------------------------------------------------------
// EXAMPLE IMPLEMENTATION - NOT CORE FRAMEWORK CODE
// ---------------------------------------------------------------------------
// This repository is a sample/reference implementation provided as part of
// the Rivora Framework starter template. It is business-specific and should
// be overridden or replaced in your own application's Infrastructure layer.
//
// The generic Repository<TEntity, TKey> base class (see Repository.cs) is
// the actual framework code and is intended to be reused as-is.
// ---------------------------------------------------------------------------

using RVR.Framework.Domain.Entities.Products;
using RVR.Framework.Domain.Repositories;
using RVR.Framework.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace RVR.Framework.Infrastructure.Repositories;

/// <summary>
/// <b>Example implementation</b> – Repository for <see cref="Product"/> entities.
/// <para>
/// This class is provided as a reference/starter implementation and is <b>not</b>
/// part of the core Rivora Framework. Consumers of the framework should create
/// their own product repository in their application's Infrastructure project,
/// inheriting from <see cref="Repository{TEntity, TKey}"/> and implementing
/// <see cref="IProductRepository"/>.
/// </para>
/// </summary>
public class ProductRepository : Repository<Product, Guid>, IProductRepository
{
    #region Compiled Queries (business-specific, kept with the repository)

    private static readonly Func<RVRDbContext, IAsyncEnumerable<Product>> GetActiveProductsQuery =
        EF.CompileAsyncQuery((RVRDbContext ctx) =>
            ctx.Products.AsNoTracking().Where(p => p.IsActive).OrderBy(p => p.Name).AsQueryable());

    private static readonly Func<RVRDbContext, Guid, Task<Product?>> GetProductByIdQuery =
        EF.CompileAsyncQuery((RVRDbContext ctx, Guid id) =>
            ctx.Products.AsNoTracking().FirstOrDefault(p => p.Id == id));

    private static readonly Func<RVRDbContext, string, Task<Product?>> GetProductBySkuQuery =
        EF.CompileAsyncQuery((RVRDbContext ctx, string sku) =>
            ctx.Products.AsNoTracking().FirstOrDefault(p => p.SKU == sku));

    #endregion

    /// <summary>
    /// Constructeur
    /// </summary>
    public ProductRepository(RVRDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<List<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<Product>();
        await foreach (var product in GetActiveProductsQuery(_context).WithCancellation(cancellationToken))
        {
            result.Add(product);
        }
        return result;
    }

    /// <inheritdoc />
    public async Task<List<Product>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.Name.Contains(name))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a product by its SKU (read-only via compiled query).
    /// </summary>
    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await GetProductBySkuQuery(_context, sku);
    }

    /// <summary>
    /// Retrieves a product by its identifier (read-only via compiled query).
    /// </summary>
    public override async Task<Product?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await GetProductByIdQuery(_context, id);
    }
}
