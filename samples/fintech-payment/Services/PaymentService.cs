using System.Collections.Concurrent;
using RVR.Fintech.Payment.Domain;
using RVR.Framework.Idempotency.Services;
using RVR.Framework.Webhooks;
using RVR.Framework.Webhooks.Models;

namespace RVR.Fintech.Payment.Services;

public sealed class PaymentService
{
    private readonly ConcurrentDictionary<Guid, PaymentTransaction> _payments = new();
    private readonly ConcurrentDictionary<Guid, Merchant> _merchants = new();
    private readonly ConcurrentDictionary<string, Guid> _idempotencyIndex = new();
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly IWebhookService _webhookService;
    private readonly ILogger<PaymentService> _logger;
    private readonly decimal _feePercent;
    private readonly decimal _fixedFee;

    public PaymentService(
        IIdempotencyStore idempotencyStore,
        IWebhookService webhookService,
        IConfiguration configuration,
        ILogger<PaymentService> logger)
    {
        _idempotencyStore = idempotencyStore;
        _webhookService = webhookService;
        _logger = logger;
        _feePercent = configuration.GetValue<decimal>("Billing:TransactionFeePercent", 2.9m);
        _fixedFee = configuration.GetValue<decimal>("Billing:FixedFeeAmount", 0.30m);
    }

    // -- Merchant operations --

    public Merchant RegisterMerchant(string name, string? email, string? webhookUrl)
    {
        var merchant = new Merchant
        {
            Name = name,
            Email = email,
            WebhookUrl = webhookUrl
        };
        _merchants[merchant.Id] = merchant;
        _logger.LogInformation("Marchand enregistre: {MerchantId} ({Name})", merchant.Id, name);
        return merchant;
    }

    public Merchant? GetMerchant(Guid id) =>
        _merchants.GetValueOrDefault(id);

    public void SetMerchantApiKeyId(Guid merchantId, Guid apiKeyId)
    {
        if (_merchants.TryGetValue(merchantId, out var merchant))
            merchant.ApiKeyId = apiKeyId;
    }

    public Merchant? FindMerchantByApiKeyId(Guid apiKeyId) =>
        _merchants.Values.FirstOrDefault(m => m.ApiKeyId == apiKeyId);

    // -- Payment operations --

    public async Task<PaymentTransaction> ProcessPaymentAsync(
        Guid merchantId, decimal amount, string currency,
        string idempotencyKey, string? description, string? customerEmail,
        CancellationToken ct = default)
    {
        // Check idempotency: return existing payment if key was already used
        if (_idempotencyIndex.TryGetValue(idempotencyKey, out var existingId)
            && _payments.TryGetValue(existingId, out var existing))
        {
            _logger.LogInformation(
                "Idempotency hit: cle {Key} -> paiement {PaymentId}",
                idempotencyKey, existingId);
            return existing;
        }

        var merchant = _merchants.GetValueOrDefault(merchantId)
            ?? throw new InvalidOperationException($"Marchand {merchantId} introuvable.");

        var payment = new PaymentTransaction
        {
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            MerchantId = merchantId,
            IdempotencyKey = idempotencyKey,
            Description = description,
            CustomerEmail = customerEmail
        };

        // Simulate payment processing
        var success = SimulateGateway(amount);
        payment.Status = success ? PaymentStatus.Completed : PaymentStatus.Failed;

        if (success)
        {
            payment.CompletedAt = DateTime.UtcNow;
            var fee = CalculateFee(amount);
            merchant.Balance += amount - fee;
            _logger.LogInformation(
                "Paiement {PaymentId} complete: {Amount} {Currency} (frais: {Fee})",
                payment.Id, amount, currency, fee);
        }
        else
        {
            _logger.LogWarning("Paiement {PaymentId} echoue", payment.Id);
        }

        _payments[payment.Id] = payment;
        _idempotencyIndex[idempotencyKey] = payment.Id;

        // Register idempotency in the store
        await _idempotencyStore.SetAsync(idempotencyKey, new IdempotencyEntry
        {
            StatusCode = success ? 201 : 402,
            Body = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(payment),
            ContentType = "application/json"
        }, ct);

        // Fire webhook for status change
        await FireWebhookAsync(
            success ? "payment.completed" : "payment.failed",
            payment, ct);

        return payment;
    }

    public async Task<PaymentTransaction> RefundPaymentAsync(
        Guid paymentId, Guid merchantId, CancellationToken ct = default)
    {
        if (!_payments.TryGetValue(paymentId, out var payment))
            throw new KeyNotFoundException($"Paiement {paymentId} introuvable.");

        if (payment.MerchantId != merchantId)
            throw new UnauthorizedAccessException("Ce paiement n'appartient pas a ce marchand.");

        if (payment.Status != PaymentStatus.Completed)
            throw new InvalidOperationException(
                $"Impossible de rembourser un paiement au statut {payment.Status}.");

        var merchant = _merchants.GetValueOrDefault(merchantId)!;
        var fee = CalculateFee(payment.Amount);

        payment.Status = PaymentStatus.Refunded;
        merchant.Balance -= payment.Amount - fee;

        _logger.LogInformation("Paiement {PaymentId} rembourse", paymentId);

        await FireWebhookAsync("payment.refunded", payment, ct);
        return payment;
    }

    public PaymentTransaction? GetPayment(Guid paymentId) =>
        _payments.GetValueOrDefault(paymentId);

    public IReadOnlyList<PaymentTransaction> GetPaymentsForMerchant(Guid merchantId) =>
        _payments.Values.Where(p => p.MerchantId == merchantId)
            .OrderByDescending(p => p.CreatedAt).ToList();

    // -- Private helpers --

    private decimal CalculateFee(decimal amount) =>
        Math.Round(amount * _feePercent / 100m + _fixedFee, 2);

    private static bool SimulateGateway(decimal amount) =>
        amount > 0 && amount < 1_000_000m;

    private async Task FireWebhookAsync(
        string eventType, PaymentTransaction payment, CancellationToken ct)
    {
        try
        {
            var merchant = _merchants.GetValueOrDefault(payment.MerchantId);
            if (merchant?.WebhookUrl is null) return;

            // Ensure a subscription exists for this merchant/event
            var subs = await _webhookService.GetSubscriptionsAsync(eventType, ct: ct);
            if (!subs.Any(s => s.CallbackUrl == merchant.WebhookUrl))
            {
                await _webhookService.SubscribeAsync(new WebhookSubscription
                {
                    EventType = eventType,
                    CallbackUrl = merchant.WebhookUrl,
                    IsActive = true
                }, ct);
            }

            await _webhookService.PublishAsync(eventType, new
            {
                PaymentId = payment.Id,
                payment.Amount,
                payment.Currency,
                Status = payment.Status.ToString(),
                payment.MerchantId,
                Timestamp = DateTime.UtcNow
            }, ct: ct);

            _logger.LogInformation(
                "Webhook {EventType} envoye pour paiement {PaymentId}",
                eventType, payment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Echec envoi webhook {EventType}", eventType);
        }
    }
}
