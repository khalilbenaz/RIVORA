using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using RVR.Framework.Domain.Entities.Products;
using RVR.Framework.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.Benchmarks;

/// <summary>
/// Benchmarks for Entity Framework Core operations including compiled queries,
/// bulk inserts, tracking behavior, and DbContext lifecycle patterns.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class EfCoreBenchmarks
{
    private DbContextOptions<RVRDbContext> _options = null!;
    private DbContextOptions<RVRDbContext> _pooledOptions = null!;
    private IDbContextFactory<RVRDbContext> _contextFactory = null!;

    // Compiled query: retrieve a product by name
    private static readonly Func<RVRDbContext, string, IAsyncEnumerable<Product>> _compiledGetProductByName =
        EF.CompileAsyncQuery((RVRDbContext ctx, string name) =>
            ctx.Products.Where(p => p.Name == name));

    [GlobalSetup]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<RVRDbContext>()
            .UseInMemoryDatabase(databaseName: $"Benchmark_{Guid.NewGuid()}")
            .Options;

        _pooledOptions = new DbContextOptionsBuilder<RVRDbContext>()
            .UseInMemoryDatabase(databaseName: $"BenchmarkPooled_{Guid.NewGuid()}")
            .Options;

        // Seed data for read benchmarks
        using var ctx = new RVRDbContext(_options);
        for (int i = 0; i < 1000; i++)
        {
            ctx.Products.Add(new Product(null, $"Product_{i}", 9.99m + i, 100 + i));
        }
        ctx.SaveChanges();

        // Build a factory for pooling benchmarks
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddPooledDbContextFactory<RVRDbContext>(opts =>
            opts.UseInMemoryDatabase(databaseName: $"BenchmarkPool_{Guid.NewGuid()}"));
        var provider = services.BuildServiceProvider();
        _contextFactory = provider.GetRequiredService<IDbContextFactory<RVRDbContext>>();

        // Seed pooled database
        using var pooledCtx = _contextFactory.CreateDbContext();
        for (int i = 0; i < 100; i++)
        {
            pooledCtx.Products.Add(new Product(null, $"PooledProduct_{i}", 19.99m + i, 50 + i));
        }
        pooledCtx.SaveChanges();
    }

    // ─── Compiled Query vs Dynamic Query ───────────────────────────────

    [Benchmark(Description = "CompiledQuery - GetProductByName")]
    public async Task<Product?> CompiledQuery()
    {
        using var ctx = new RVRDbContext(_options);
        await foreach (var product in _compiledGetProductByName(ctx, "Product_500"))
        {
            return product;
        }
        return null;
    }

    [Benchmark(Description = "DynamicQuery - GetProductByName")]
    public async Task<Product?> DynamicQuery()
    {
        using var ctx = new RVRDbContext(_options);
        return await ctx.Products
            .Where(p => p.Name == "Product_500")
            .FirstOrDefaultAsync();
    }

    // ─── Bulk Insert vs Single Insert ──────────────────────────────────

    [Benchmark(Description = "SingleInsert - 100 records")]
    public async Task SingleInsert_100()
    {
        var options = new DbContextOptionsBuilder<RVRDbContext>()
            .UseInMemoryDatabase($"SingleInsert100_{Guid.NewGuid()}")
            .Options;
        using var ctx = new RVRDbContext(options);
        for (int i = 0; i < 100; i++)
        {
            ctx.Products.Add(new Product(null, $"SingleProduct_{i}", 5.99m, 10));
        }
        await ctx.SaveChangesAsync();
    }

    [Benchmark(Description = "BulkInsert (AddRange) - 100 records")]
    public async Task BulkInsert_100()
    {
        var options = new DbContextOptionsBuilder<RVRDbContext>()
            .UseInMemoryDatabase($"BulkInsert100_{Guid.NewGuid()}")
            .Options;
        using var ctx = new RVRDbContext(options);
        var products = Enumerable.Range(0, 100)
            .Select(i => new Product(null, $"BulkProduct_{i}", 5.99m, 10))
            .ToList();
        await ctx.Products.AddRangeAsync(products);
        await ctx.SaveChangesAsync();
    }

    [Benchmark(Description = "SingleInsert - 1000 records")]
    public async Task SingleInsert_1000()
    {
        var options = new DbContextOptionsBuilder<RVRDbContext>()
            .UseInMemoryDatabase($"SingleInsert1000_{Guid.NewGuid()}")
            .Options;
        using var ctx = new RVRDbContext(options);
        for (int i = 0; i < 1000; i++)
        {
            ctx.Products.Add(new Product(null, $"SingleProduct_{i}", 5.99m, 10));
        }
        await ctx.SaveChangesAsync();
    }

    [Benchmark(Description = "BulkInsert (AddRange) - 1000 records")]
    public async Task BulkInsert_1000()
    {
        var options = new DbContextOptionsBuilder<RVRDbContext>()
            .UseInMemoryDatabase($"BulkInsert1000_{Guid.NewGuid()}")
            .Options;
        using var ctx = new RVRDbContext(options);
        var products = Enumerable.Range(0, 1000)
            .Select(i => new Product(null, $"BulkProduct_{i}", 5.99m, 10))
            .ToList();
        await ctx.Products.AddRangeAsync(products);
        await ctx.SaveChangesAsync();
    }

    // ─── AsNoTracking vs Tracking ──────────────────────────────────────

    [Benchmark(Description = "Tracking - Read 100 products")]
    public async Task<List<Product>> TrackingRead()
    {
        using var ctx = new RVRDbContext(_options);
        return await ctx.Products.Take(100).ToListAsync();
    }

    [Benchmark(Description = "AsNoTracking - Read 100 products")]
    public async Task<List<Product>> AsNoTrackingRead()
    {
        using var ctx = new RVRDbContext(_options);
        return await ctx.Products.AsNoTracking().Take(100).ToListAsync();
    }

    // ─── DbContext Pooling vs New DbContext ─────────────────────────────

    [Benchmark(Description = "NewDbContext - create and query")]
    public async Task<int> NewDbContext()
    {
        using var ctx = new RVRDbContext(_options);
        return await ctx.Products.CountAsync();
    }

    [Benchmark(Description = "PooledDbContext - create and query")]
    public async Task<int> PooledDbContext()
    {
        using var ctx = _contextFactory.CreateDbContext();
        return await ctx.Products.CountAsync();
    }
}
