using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KBA.SaaS.Starter.Application.Commands;
using KBA.SaaS.Starter.Application.Queries;
using KBA.SaaS.Starter.Application.DTOs;
using KBA.SaaS.Starter.Identity;

namespace KBA.SaaS.Starter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var tenants = await _mediator.Send(new GetAllTenantsQuery());
        return Ok(tenants);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenant = await _mediator.Send(new GetTenantByIdQuery(id));
        return tenant == null ? NotFound() : Ok(tenant);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateTenantCommand command)
    {
        var tenant = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, tenant);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IdentityService _identityService;

    public UsersController(IMediator mediator, IdentityService identityService)
    {
        _mediator = mediator;
        _identityService = identityService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var user = await _mediator.Send(command);
        return Ok(user);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // TODO: Implement proper login with repository
        return Ok(new { token = "demo-token", message = "Login endpoint - implement with UserRepository" });
    }

    [HttpPost("{id}/2fa/enable")]
    public async Task<IActionResult> EnableTwoFactor(Guid id)
    {
        var secret = _identityService.GenerateTwoFactorSecret();
        // TODO: Save secret to user and return QR code URL
        return Ok(new { secret, qrCodeUrl = $"otpauth://totp/SaaS:{id}?secret={secret}&issuer=SaaS" });
    }

    [HttpPost("{id}/2fa/verify")]
    public async Task<IActionResult> VerifyTwoFactor(Guid id, [FromBody] string code)
    {
        // TODO: Implement verification
        return Ok(new { verified = true });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetByTenant([FromQuery] Guid tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var products = await _mediator.Send(new GetProductsByTenantQuery(tenantId, page, pageSize));
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id));
        return product == null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var product = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id) return BadRequest();
        var product = await _mediator.Send(command);
        return Ok(product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteProductCommand(id));
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery(id));
        return order == null ? NotFound() : Ok(order);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId, int page = 1, int pageSize = 10)
    {
        var orders = await _mediator.Send(new GetOrdersByCustomerQuery(customerId, page, pageSize));
        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
    {
        var order = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPost("{id}/ship")]
    public async Task<IActionResult> Ship(Guid id)
    {
        var order = await _mediator.Send(new ShipOrderCommand(id, DateTime.UtcNow));
        return Ok(order);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var order = await _mediator.Send(new CancelOrderCommand(id));
        return Ok(order);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeatureFlagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeatureFlagsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? tenantId)
    {
        var flags = await _mediator.Send(new GetAllFeatureFlagsQuery(tenantId));
        return Ok(flags);
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetByName(string name, [FromQuery] Guid? tenantId)
    {
        var flag = await _mediator.Send(new GetFeatureFlagByNameQuery(name, tenantId));
        return flag == null ? NotFound() : Ok(flag);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeatureFlagCommand command)
    {
        var flag = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetByName), new { name = flag.Name }, flag);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFeatureFlagCommand command)
    {
        if (id != command.Id) return BadRequest();
        var flag = await _mediator.Send(command);
        return Ok(flag);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditController : ControllerBase
{
    private readonly IRepository<KBA.SaaS.Starter.Domain.Entities.AuditLog> _repository;

    public AuditController(IRepository<KBA.SaaS.Starter.Domain.Entities.AuditLog> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] Guid? userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await _repository.GetAllAsync();
        if (userId.HasValue)
            logs = logs.Where(l => l.UserId == userId.Value);
        
        logs = logs.OrderByDescending(l => l.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize);
        
        return Ok(logs.Select(l => new
        {
            l.Id,
            l.Action,
            l.EntityName,
            l.EntityId,
            l.IpAddress,
            l.CreatedAt,
            UserName = "Unknown" // TODO: Include user name
        }));
    }
}
