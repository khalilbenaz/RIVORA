namespace KBA.SaaS.Starter.Application.Queries;

using MediatR;
using KBA.SaaS.Starter.Application.DTOs;

// Tenant Queries
public record GetTenantByIdQuery(Guid Id) : IRequest<TenantDto?>;
public record GetTenantBySlugQuery(string Slug) : IRequest<TenantDto?>;
public record GetAllTenantsQuery() : IRequest<IEnumerable<TenantDto>>;

// User Queries
public record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;
public record GetUserByEmailQuery(string Email) : IRequest<UserDto?>;
public record GetUsersByTenantQuery(Guid TenantId) : IRequest<IEnumerable<UserDto>>;

// Product Queries
public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
public record GetProductsByTenantQuery(Guid TenantId, int Page = 1, int PageSize = 10) : IRequest<IEnumerable<ProductDto>>;
public record SearchProductsQuery(string SearchTerm, Guid TenantId) : IRequest<IEnumerable<ProductDto>>;

// Order Queries
public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDto?>;
public record GetOrdersByCustomerQuery(Guid CustomerId, int Page = 1, int PageSize = 10) : IRequest<IEnumerable<OrderDto>>;
public record GetOrdersByTenantQuery(Guid TenantId, int Page = 1, int PageSize = 10) : IRequest<IEnumerable<OrderDto>>;

// Feature Flag Queries
public record GetFeatureFlagByNameQuery(string Name, Guid? TenantId = null) : IRequest<FeatureFlagDto?>;
public record GetAllFeatureFlagsQuery(Guid? TenantId = null) : IRequest<IEnumerable<FeatureFlagDto>>;

// Audit Log Queries
public record GetAuditLogsByUserQuery(Guid UserId, int Page = 1, int PageSize = 50) : IRequest<IEnumerable<AuditLogDto>>;
public record GetAuditLogsByEntityQuery(string EntityName, Guid EntityId) : IRequest<IEnumerable<AuditLogDto>>;
