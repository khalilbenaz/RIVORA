using MediatR;
using KBA.SaaS.Starter.Application.Commands;
using KBA.SaaS.Starter.Application.DTOs;
using KBA.SaaS.Starter.Application.Queries;
using KBA.SaaS.Starter.Domain.Entities;
using KBA.SaaS.Starter.Infrastructure.Repositories;
using KBA.SaaS.Starter.Identity;

namespace KBA.SaaS.Starter.Application.Handlers;

// Tenant Handlers
public class CreateTenantHandler : IRequestHandler<CreateTenantCommand, TenantDto>
{
    private readonly ITenantRepository _tenantRepository;

    public CreateTenantHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken ct)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = request.Slug.ToLowerInvariant(),
            PlanType = request.PlanType,
            SubscriptionStartDate = DateTime.UtcNow,
            IsActive = true
        };

        await _tenantRepository.AddAsync(tenant, ct);

        return MapToDto(tenant);
    }

    private static TenantDto MapToDto(Tenant tenant) => new()
    {
        Id = tenant.Id,
        Name = tenant.Name,
        Slug = tenant.Slug,
        IsActive = tenant.IsActive,
        PlanType = tenant.PlanType,
        SubscriptionStartDate = tenant.SubscriptionStartDate,
        SubscriptionEndDate = tenant.SubscriptionEndDate
    };
}

public class GetAllTenantsHandler : IRequestHandler<GetAllTenantsQuery, IEnumerable<TenantDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetAllTenantsHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<IEnumerable<TenantDto>> Handle(GetAllTenantsQuery request, CancellationToken ct)
    {
        var tenants = await _tenantRepository.GetActiveTenantsAsync(ct);
        return tenants.Select(t => new TenantDto
        {
            Id = t.Id,
            Name = t.Name,
            Slug = t.Slug,
            IsActive = t.IsActive,
            PlanType = t.PlanType,
            SubscriptionStartDate = t.SubscriptionStartDate,
            SubscriptionEndDate = t.SubscriptionEndDate
        });
    }
}

// User Handlers
public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IdentityService _identityService;

    public RegisterUserHandler(IUserRepository userRepository, IdentityService identityService)
    {
        _userRepository = userRepository;
        _identityService = identityService;
    }

    public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = _identityService.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role,
            TenantId = request.TenantId,
            EmailConfirmed = false
        };

        await _userRepository.AddAsync(user, ct);

        return MapToDto(user);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        PhoneNumber = user.PhoneNumber,
        Role = user.Role,
        TwoFactorEnabled = user.TwoFactorEnabled,
        TenantId = user.TenantId
    };
}

// Product Handlers
public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;

    public CreateProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            Sku = request.Sku,
            TenantId = request.TenantId,
            IsActive = true
        };

        await _productRepository.AddAsync(product, ct);

        return MapToDto(product);
    }

    private static ProductDto MapToDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Stock = product.Stock,
        Sku = product.Sku,
        IsActive = product.IsActive,
        TenantId = product.TenantId
    };
}

public class GetProductsByTenantHandler : IRequestHandler<GetProductsByTenantQuery, IEnumerable<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsByTenantHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetProductsByTenantQuery request, CancellationToken ct)
    {
        var products = await _productRepository.GetByTenantAsync(request.TenantId, request.Page, request.PageSize, ct);
        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            Sku = p.Sku,
            IsActive = p.IsActive,
            TenantId = p.TenantId
        });
    }
}

// Order Handlers
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;

    public CreateOrderHandler(IOrderRepository orderRepository, IProductRepository productRepository, IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var customer = await _userRepository.GetByIdAsync(request.CustomerId, ct)
            ?? throw new InvalidOperationException("Customer not found");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            CustomerId = request.CustomerId,
            TenantId = customer.TenantId,
            TotalAmount = 0,
            Status = "Pending",
            OrderDate = DateTime.UtcNow,
            ShippingAddress = request.ShippingAddress
        };

        decimal total = 0;
        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, ct)
                ?? throw new InvalidOperationException($"Product {item.ProductId} not found");

            if (product.Stock < item.Quantity)
                throw new InvalidOperationException($"Insufficient stock for {product.Name}");

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            };

            order.OrderItems.Add(orderItem);
            total += product.Price * item.Quantity;
            product.Stock -= item.Quantity;

            await _productRepository.UpdateAsync(product, ct);
        }

        order.TotalAmount = total;

        await _orderRepository.AddAsync(order, ct);

        return MapToDto(order);
    }

    private static OrderDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        CustomerId = order.CustomerId,
        TotalAmount = order.TotalAmount,
        Status = order.Status,
        OrderDate = order.OrderDate,
        Items = order.OrderItems.Select(oi => new OrderItemDto
        {
            ProductId = oi.ProductId,
            Quantity = oi.Quantity,
            UnitPrice = oi.UnitPrice
        }).ToList()
    };
}

// Feature Flag Handlers
public class GetFeatureFlagByNameHandler : IRequestHandler<GetFeatureFlagByNameQuery, FeatureFlagDto?>
{
    private readonly IRepository<FeatureFlag> _repository;

    public GetFeatureFlagByNameHandler(IRepository<FeatureFlag> repository)
    {
        _repository = repository;
    }

    public async Task<FeatureFlagDto?> Handle(GetFeatureFlagByNameQuery request, CancellationToken ct)
    {
        var flags = await _repository.GetAllAsync(ct);
        var flag = flags.FirstOrDefault(f => 
            f.Name == request.Name && 
            (request.TenantId == null || f.TenantId == request.TenantId || f.TenantId == null) &&
            (f.ExpiresAt == null || f.ExpiresAt > DateTime.UtcNow));

        return flag == null ? null : new FeatureFlagDto
        {
            Id = flag.Id,
            Name = flag.Name,
            Description = flag.Description,
            Enabled = flag.Enabled,
            ExpiresAt = flag.ExpiresAt
        };
    }
}
