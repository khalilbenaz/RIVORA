using Microsoft.AspNetCore.Mvc;
using RVR.Fintech.Payment.Services;
using RVR.Framework.ApiKeys.Models;
using RVR.Framework.ApiKeys.Services;

namespace RVR.Fintech.Payment.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MerchantsController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly IApiKeyService _apiKeyService;

    public MerchantsController(PaymentService paymentService, IApiKeyService apiKeyService)
    {
        _paymentService = paymentService;
        _apiKeyService = apiKeyService;
    }

    /// <summary>
    /// Enregistrer un nouveau marchand.
    /// </summary>
    [HttpPost]
    public IActionResult Register([FromBody] RegisterMerchantRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Le nom du marchand est requis." });

        var merchant = _paymentService.RegisterMerchant(
            request.Name, request.Email, request.WebhookUrl);

        return Created($"/api/merchants/{merchant.Id}", new
        {
            merchant.Id,
            merchant.Name,
            merchant.Email,
            merchant.WebhookUrl,
            merchant.CreatedAt
        });
    }

    /// <summary>
    /// Generer une cle API pour un marchand.
    /// La cle en clair n'est retournee qu'une seule fois.
    /// </summary>
    [HttpPost("{id:guid}/api-keys")]
    public async Task<IActionResult> GenerateApiKey(Guid id, CancellationToken ct)
    {
        var merchant = _paymentService.GetMerchant(id);
        if (merchant is null)
            return NotFound(new { error = "Marchand introuvable." });

        var keyResult = await _apiKeyService.GenerateKeyAsync(new ApiKeyCreateRequest
        {
            Name = $"merchant-{merchant.Name}",
            Scopes = ["payments:write", "payments:read"]
        }, ct);

        _paymentService.SetMerchantApiKeyId(id, keyResult.Id);

        return Created($"/api/merchants/{id}/api-keys", new
        {
            keyResult.Id,
            ApiKey = keyResult.PlainTextKey,
            keyResult.Name,
            keyResult.Scopes,
            keyResult.CreatedAt,
            Message = "Conservez cette cle en lieu sur. Elle ne sera plus affichee."
        });
    }

    /// <summary>
    /// Consulter le solde d'un marchand.
    /// </summary>
    [HttpGet("{id:guid}/balance")]
    public IActionResult GetBalance(Guid id)
    {
        var merchant = _paymentService.GetMerchant(id);
        if (merchant is null)
            return NotFound(new { error = "Marchand introuvable." });

        return Ok(new
        {
            merchant.Id,
            merchant.Name,
            merchant.Balance,
            Currency = "EUR"
        });
    }
}

public sealed record RegisterMerchantRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? WebhookUrl { get; init; }
}
