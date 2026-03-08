namespace KBA.SaaS.Starter.Application.Commands;

using MediatR;
using KBA.SaaS.Starter.Application.DTOs;

// Tenant Commands
public record CreateTenantCommand(string Name, string Slug, string PlanType = "Free") : IRequest<TenantDto>;
public record UpdateTenantCommand(Guid Id, string Name, string PlanType, bool IsActive) : IRequest<TenantDto>;
public record DeleteTenantCommand(Guid Id) : IRequest<Unit>;

// User Commands
public record RegisterUserCommand(string Email, string Password, string FirstName, string LastName, Guid TenantId, string Role = "User") : IRequest<UserDto>;
public record UpdateUserCommand(Guid Id, string FirstName, string LastName, string? PhoneNumber) : IRequest<UserDto>;
public record EnableTwoFactorCommand(Guid UserId) : IRequest<string>;
public record VerifyTwoFactorCommand(Guid UserId, string Code) : IRequest<bool>;
public record DeleteUserCommand(Guid Id) : IRequest<Unit>;

// Product Commands
public record CreateProductCommand(string Name, string Description, decimal Price, int Stock, string Sku, Guid TenantId) : IRequest<ProductDto>;
public record UpdateProductCommand(Guid Id, string Name, string Description, decimal Price, int Stock, bool IsActive) : IRequest<ProductDto>;
public record DeleteProductCommand(Guid Id) : IRequest<Unit>;

// Order Commands
public record CreateOrderCommand(Guid CustomerId, List<OrderItemCommand> Items, string? ShippingAddress) : IRequest<OrderDto>;
public record OrderItemCommand(Guid ProductId, int Quantity);
public record UpdateOrderStatusCommand(Guid Id, string Status) : IRequest<OrderDto>;
public record ShipOrderCommand(Guid Id, DateTime shippedDate) : IRequest<OrderDto>;
public record CancelOrderCommand(Guid Id) : IRequest<OrderDto>;

// Feature Flag Commands
public record CreateFeatureFlagCommand(string Name, string Description, bool Enabled, Guid? TenantId = null) : IRequest<FeatureFlagDto>;
public record UpdateFeatureFlagCommand(Guid Id, bool Enabled) : IRequest<FeatureFlagDto>;
public record DeleteFeatureFlagCommand(Guid Id) : IRequest<Unit>;
