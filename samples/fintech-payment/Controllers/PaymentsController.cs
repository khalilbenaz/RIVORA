using Microsoft.AspNetCore.Mvc;
using RVR.Fintech.Payment.Services;
using RVR.Framework.ApiKeys.Services;

namespace RVR.Fintech.Payment.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly IApiKeyService _apiKeyService;

    public PaymentsController(PaymentService paymentService, IApiKeyService apiKeyService)
    {
        _paymentService = paymentService;
        _apiKeyService = apiKeyService;
    }

    /// <summary>
    /// Traiter un nouveau paiement.
    /// Requiert les en-tetes X-Api-Key et X-Idempotency-Key.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ProcessPayment(
        [FromBody] ProcessPaymentRequest request,
        CancellationToken ct)
    {
        var merchant = await AuthenticateMerchant(ct);
        if (merchant is null)
            return Unauthorized(new { error = "Cle API invalide ou manquante." });

        if (!Request.Headers.TryGetValue("X-Idempotency-Key", out var idempotencyKey)
            || string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest(new { error = "L'en-tete X-Idempotency-Key est requis." });

        if (request.Amount <= 0)
            return BadRequest(new { error = "Le montant doit etre positif." });

        var payment = await _paymentService.ProcessPaymentAsync(
            merchant.Id, request.Amount, request.Currency ?? "EUR",
            idempotencyKey!, request.Description, request.CustomerEmail, ct);

        return payment.Status == Domain.PaymentStatus.Completed
            ? Created($"/api/payments/{payment.Id}", payment)
            : StatusCode(402, payment);
    }

    /// <summary>
    /// Rembourser un paiement existant.
    /// </summary>
    [HttpPost("{id:guid}/refund")]
    public async Task<IActionResult> RefundPayment(Guid id, CancellationToken ct)
    {
        var merchant = await AuthenticateMerchant(ct);
        if (merchant is null)
            return Unauthorized(new { error = "Cle API invalide ou manquante." });

        try
        {
            var payment = await _paymentService.RefundPaymentAsync(id, merchant.Id, ct);
            return Ok(payment);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Paiement introuvable." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtenir le statut d'un paiement.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPayment(Guid id, CancellationToken ct)
    {
        var merchant = await AuthenticateMerchant(ct);
        if (merchant is null)
            return Unauthorized(new { error = "Cle API invalide ou manquante." });

        var payment = _paymentService.GetPayment(id);
        if (payment is null || payment.MerchantId != merchant.Id)
            return NotFound(new { error = "Paiement introuvable." });

        return Ok(payment);
    }

    /// <summary>
    /// Lister les paiements du marchand authentifie.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListPayments(CancellationToken ct)
    {
        var merchant = await AuthenticateMerchant(ct);
        if (merchant is null)
            return Unauthorized(new { error = "Cle API invalide ou manquante." });

        var payments = _paymentService.GetPaymentsForMerchant(merchant.Id);
        return Ok(payments);
    }

    private async Task<Domain.Merchant?> AuthenticateMerchant(CancellationToken ct)
    {
        if (!Request.Headers.TryGetValue("X-Api-Key", out var apiKey)
            || string.IsNullOrWhiteSpace(apiKey))
            return null;

        var keyResult = await _apiKeyService.ValidateKeyAsync(apiKey!, ct);
        if (keyResult is null || keyResult.IsRevoked)
            return null;

        return _paymentService.FindMerchantByApiKeyId(keyResult.Id);
    }
}

public sealed record ProcessPaymentRequest
{
    public decimal Amount { get; init; }
    public string? Currency { get; init; }
    public string? Description { get; init; }
    public string? CustomerEmail { get; init; }
}
