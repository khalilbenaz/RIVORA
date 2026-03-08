using Microsoft.EntityFrameworkCore;
using KBA.SaaS.Starter.Domain.Entities;
using KBA.SaaS.Starter.Infrastructure.Data;

namespace KBA.SaaS.Starter.Infrastructure.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbSet.FindAsync(new object[] { id }, ct);

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.ToListAsync(ct);

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
}

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IEnumerable<Tenant>> GetActiveTenantsAsync(CancellationToken ct = default);
}

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(AppDbContext context) : base(context) { }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(t => t.Slug == slug, ct);

    public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync(CancellationToken ct = default)
        => await _dbSet.Where(t => t.IsActive).ToListAsync(ct);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IEnumerable<User>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<IEnumerable<User>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _dbSet.Where(u => u.TenantId == tenantId).ToListAsync(ct);
}

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByTenantAsync(Guid tenantId, int page, int pageSize, CancellationToken ct = default);
    Task<IEnumerable<Product>> SearchAsync(string searchTerm, Guid tenantId, CancellationToken ct = default);
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Product>> GetByTenantAsync(Guid tenantId, int page, int pageSize, CancellationToken ct = default)
        => await _dbSet.Where(p => p.TenantId == tenantId && p.IsActive)
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm, Guid tenantId, CancellationToken ct = default)
        => await _dbSet.Where(p => p.TenantId == tenantId && p.IsActive && 
            (p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)))
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
}

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId, int page, int pageSize, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetByTenantAsync(Guid tenantId, int page, int pageSize, CancellationToken ct = default);
}

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context) { }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
        => await _dbSet.Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);

    public async Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId, int page, int pageSize, CancellationToken ct = default)
        => await _dbSet.Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<IEnumerable<Order>> GetByTenantAsync(Guid tenantId, int page, int pageSize, CancellationToken ct = default)
        => await _dbSet.Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .Where(o => o.TenantId == tenantId)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
}
