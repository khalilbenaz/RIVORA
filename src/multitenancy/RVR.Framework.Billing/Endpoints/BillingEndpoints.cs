using RVR.Framework.Billing.Interfaces;
using RVR.Framework.Billing.Webhooks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace RVR.Framework.Billing.Endpoints;

/// <summary>
/// Minimal API endpoints for billing operations.
/// Implements the IMapEndpoints pattern used by the RIVORA Framework.
/// </summary>
public sealed class BillingEndpoints
{
    /// <summary>
    /// Maps all billing endpoints to the application's endpoint route builder.
    /// </summary>
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/billing")
            .WithTags("Billing");

        group.MapPost("/checkout", CreateCheckoutSessionAsync)
            .WithName("CreateCheckoutSession")
            .WithSummary("Create a Stripe checkout session for subscription");

        group.MapPost("/portal", CreatePortalSessionAsync)
            .WithName("CreatePortalSession")
            .WithSummary("Create a Stripe customer portal session");

        group.MapGet("/subscription", GetSubscriptionAsync)
            .WithName("GetSubscription")
            .WithSummary("Get the current subscription for a tenant");

        group.MapGet("/invoices", GetInvoicesAsync)
            .WithName("GetInvoices")
            .WithSummary("Get all invoices for a tenant");

        group.MapPost("/usage", RecordUsageAsync)
            .WithName("RecordUsage")
            .WithSummary("Record a usage event for metered billing");

        group.MapPost("/webhooks/stripe", HandleStripeWebhookAsync)
            .WithName("HandleStripeWebhook")
            .WithSummary("Handle incoming Stripe webhook events");
    }

    private static async Task<IResult> CreateCheckoutSessionAsync(
        [FromBody] CreateCheckoutRequest request,
        [FromServices] IBillingService billingService,
        CancellationToken cancellationToken)
    {
        var url = await billingService.CreateCheckoutSessionAsync(
            request.TenantId, request.PlanId, request.SuccessUrl, request.CancelUrl, cancellationToken);

        return Results.Ok(new { url });
    }

    private static async Task<IResult> CreatePortalSessionAsync(
        [FromBody] CreatePortalRequest request,
        [FromServices] IBillingService billingService,
        CancellationToken cancellationToken)
    {
        var url = await billingService.ManageSubscriptionAsync(
            request.TenantId, request.ReturnUrl, cancellationToken);

        return Results.Ok(new { url });
    }

    private static async Task<IResult> GetSubscriptionAsync(
        [FromQuery] string tenantId,
        [FromServices] IBillingService billingService,
        CancellationToken cancellationToken)
    {
        var subscription = await billingService.GetSubscriptionAsync(tenantId, cancellationToken);

        return subscription is not null
            ? Results.Ok(subscription)
            : Results.NotFound();
    }

    private static async Task<IResult> GetInvoicesAsync(
        [FromQuery] string tenantId,
        [FromServices] IBillingService billingService,
        CancellationToken cancellationToken)
    {
        var invoices = await billingService.GetInvoicesAsync(tenantId, cancellationToken);

        return Results.Ok(invoices);
    }

    private static async Task<IResult> RecordUsageAsync(
        [FromBody] RecordUsageRequest request,
        [FromServices] IBillingService billingService,
        CancellationToken cancellationToken)
    {
        await billingService.RecordUsageAsync(
            request.TenantId, request.MetricName, request.Quantity, cancellationToken);

        return Results.Accepted();
    }

    private static async Task<IResult> HandleStripeWebhookAsync(
        HttpContext httpContext,
        [FromServices] StripeWebhookHandler webhookHandler,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(httpContext.Request.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);

        var signature = httpContext.Request.Headers["Stripe-Signature"].ToString();

        if (string.IsNullOrEmpty(signature))
        {
            return Results.BadRequest("Missing Stripe-Signature header");
        }

        var success = await webhookHandler.HandleWebhookAsync(body, signature, cancellationToken);

        return success ? Results.Ok() : Results.BadRequest("Webhook processing failed");
    }
}

/// <summary>
/// Request to create a checkout session.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="PlanId">The plan identifier.</param>
/// <param name="SuccessUrl">The URL to redirect to after successful checkout.</param>
/// <param name="CancelUrl">The URL to redirect to if checkout is canceled.</param>
public sealed record CreateCheckoutRequest(string TenantId, Guid PlanId, string SuccessUrl, string CancelUrl);

/// <summary>
/// Request to create a customer portal session.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="ReturnUrl">The URL to redirect to when the portal session ends.</param>
public sealed record CreatePortalRequest(string TenantId, string ReturnUrl);

/// <summary>
/// Request to record a usage event.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="MetricName">The name of the usage metric.</param>
/// <param name="Quantity">The quantity consumed.</param>
public sealed record RecordUsageRequest(string TenantId, string MetricName, long Quantity);
