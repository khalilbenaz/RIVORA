using KBA.Framework.Domain.Entities.Products;
using KBA.Framework.Domain.Repositories;
using KBA.Framework.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KBA.Framework.Infrastructure.Repositories;

/// <summary>
/// Repository pour les produits
/// </summary>
public class ProductRepository : Repository<Product, Guid>, IProductRepository
{
    private static readonly Func<KBADbContext, IAsyncEnumerable<Product>> _getActiveProductsQuery =
        EF.CompileAsyncQuery((KBADbContext context) =>
            context.Set<Product>()
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name).AsQueryable());

    /// <summary>
    /// Constructeur
    /// </summary>
    public ProductRepository(KBADbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<List<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<Product>();
        await foreach (var product in _getActiveProductsQuery(_context).WithCancellation(cancellationToken))
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
}
