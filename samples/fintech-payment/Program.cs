using RVR.Fintech.Payment.Services;
using RVR.Framework.ApiKeys.Extensions;
using RVR.Framework.Billing.Extensions;
using RVR.Framework.Idempotency.Extensions;
using RVR.Framework.Webhooks.Extensions;

var builder = WebApplication.CreateBuilder(args);

// --- RIVORA Framework modules ---

builder.Services.AddRvrIdempotency();

builder.Services.AddRvrApiKeys();

builder.Services.AddRvrWebhooks(options =>
{
    options.DefaultMaxRetries = builder.Configuration.GetValue("Webhooks:MaxRetries", 3);
    options.TimeoutSeconds = builder.Configuration.GetValue("Webhooks:TimeoutSeconds", 30);
    options.AllowedSchemes = ["https", "http"]; // Allow HTTP for local dev
    options.BlockPrivateNetworks = false;
});

builder.Services.AddRvrBilling(options =>
{
    options.StripeApiKey = builder.Configuration["Billing:StripeApiKey"] ?? "";
    options.StripeWebhookSecret = builder.Configuration["Billing:StripeWebhookSecret"] ?? "";
    options.DefaultCurrency = builder.Configuration["Billing:DefaultCurrency"] ?? "eur";
});

// --- Application services ---

builder.Services.AddSingleton<PaymentService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RVR Fintech Payment API", Version = "v1" });
});

var app = builder.Build();

// --- Middleware pipeline ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseIdempotency();
app.MapControllers();

app.Logger.LogInformation("RVR Fintech Payment API demarree");
app.Run();
