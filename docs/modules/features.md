# Feature Flags - KBA Framework

Système de feature flags avec multiples providers pour activer/désactiver des fonctionnalités dynamiquement.

## Table des Matières

- [Vue d'ensemble](#vue-densemble)
- [Providers](#providers)
- [Configuration](#configuration)
- [Usage](#usage)
- [Feature Gates](#feature-gates)
- [Dashboard](#dashboard)
- [Best Practices](#best-practices)

---

## Vue d'ensemble

KBA Framework supporte 4 providers de feature flags :

| Provider | Package | Use Case |
|----------|---------|----------|
| **Config** | Built-in | Configuration appsettings.json |
| **Database** | Built-in | Features stockées en database |
| **Azure** | Built-in | Azure App Configuration |
| **Custom** | IFeatureProvider | Provider personnalisé |

---

## Providers

### Config Provider

Features définies dans appsettings.json :

```json
{
  "FeatureFlags": {
    "ConfigFilePath": "features.json",
    "Features": {
      "NewDashboard": {
        "Enabled": true,
        "Description": "Nouveau dashboard utilisateur"
      },
      "BetaFeatures": {
        "Enabled": false,
        "Description": "Fonctionnalités beta"
      },
      "PremiumFeatures": {
        "Enabled": true,
        "Description": "Fonctionnalités premium",
        "Conditions": {
          "UserRole": ["Premium", "Admin"]
        }
      }
    }
  }
}
```

### Database Provider

Features stockées en base de données :

```csharp
builder.Services.AddFeatureFlagsWithDatabase(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Azure App Configuration Provider

Features gérées via Azure App Configuration :

```csharp
builder.Services.AddFeatureFlagsWithAzure(
    builder.Configuration["Azure:AppConfiguration:ConnectionString"],
    options =>
    {
        options.Endpoint = builder.Configuration["Azure:AppConfiguration:Endpoint"];
        options.RefreshInterval = TimeSpan.FromMinutes(5);
        options.UseFeatureFlags = true;
    });
```

### Multiple Providers

```csharp
builder.Services.AddFeatureFlagsWithProviders(builder.Configuration, builder =>
{
    builder
        .WithConfigProvider()
        .WithDatabaseProvider(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")))
        .WithAzureProvider(builder.Configuration["Azure:AppConfiguration:ConnectionString"]);
});
```

---

## Configuration

### Program.cs

```csharp
using KBA.Framework.Features.Extensions;

// Configuration simple avec config provider
builder.Services.AddFeatureFlags(builder.Configuration);

// Configuration avec options
builder.Services.AddFeatureFlags(options =>
{
    options.DefaultEnabledState = false;
    options.CacheExpiration = TimeSpan.FromMinutes(5);
});

// Configuration avec database
builder.Services.AddFeatureFlagsWithDatabase(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuration avec Azure
builder.Services.AddFeatureFlagsWithAzure(
    builder.Configuration["Azure:AppConfiguration:ConnectionString"]);
```

### appsettings.json

```json
{
  "FeatureFlags": {
    "DefaultEnabledState": false,
    "CacheExpiration": "00:05:00",
    "ConfigFilePath": "features.json",
    "Features": {
      "NewDashboard": {
        "Enabled": true,
        "Description": "Nouveau dashboard",
        "Tags": ["ui", "dashboard"]
      },
      "ExportToPdf": {
        "Enabled": false,
        "Description": "Export PDF des rapports",
        "Tags": ["reporting", "export"]
      }
    }
  }
}
```

---

## Usage

### IFeatureManager

```csharp
using KBA.Framework.Features.Core;

public class DashboardController : ControllerBase
{
    private readonly IFeatureManager _featureManager;

    public DashboardController(IFeatureManager featureManager)
    {
        _featureManager = featureManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        if (await _featureManager.IsEnabledAsync("NewDashboard"))
        {
            // Nouvelle version du dashboard
            return View("NewDashboard");
        }

        // Ancienne version
        return View("LegacyDashboard");
    }
}
```

### Feature Gate Attribute

```csharp
using KBA.Framework.Features.Attributes;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    [HttpGet("export/pdf")]
    [FeatureGate("ExportToPdf")]
    public async Task<IActionResult> ExportToPdf()
    {
        // Cette action n'est accessible que si ExportToPdf est activé
        var report = await GeneratePdfReport();
        return File(report, "application/pdf");
    }

    [HttpGet("export/csv")]
    [FeatureGate("ExportToCsv", FallbackEnabled = true)]
    public async Task<IActionResult> ExportToCsv()
    {
        // FallbackEnabled = true permet l'accès même si la feature est désactivée
        var report = await GenerateCsvReport();
        return File(report, "text/csv");
    }
}
```

### Feature Gate dans le code

```csharp
public class ReportService
{
    private readonly IFeatureManager _featureManager;

    public ReportService(IFeatureManager featureManager)
    {
        _featureManager = featureManager;
    }

    public async Task<Report> GenerateAsync(ReportType type)
    {
        if (type == ReportType.Pdf && !await _featureManager.IsEnabledAsync("ExportToPdf"))
        {
            throw new FeatureDisabledException("ExportToPdf");
        }

        // Generation logic
        return await GenerateReport(type);
    }
}
```

---

## Feature Gates

### Attribute Options

```csharp
[FeatureGate("FeatureName")]
[FeatureGate("FeatureName", FallbackEnabled = true)]
[FeatureGate("FeatureName", RedirectUrl = "/features/disabled")]
[FeatureGate("FeatureName", Policy = "AdminOnly")]
```

### Custom Feature Gate Handler

```csharp
public class CustomFeatureGateHandler : IFeatureGateHandler
{
    public async Task<bool> CanAccessAsync(string featureName, HttpContext context)
    {
        // Logique personnalisée
        var user = context.User;
        
        if (featureName == "PremiumFeatures")
        {
            return user.IsInRole("Premium");
        }

        return true;
    }
}
```

---

## Dashboard

### Activer le Dashboard

```csharp
builder.Services.AddFeatureFlagsDashboard();

// Dans Program.cs
app.MapRazorPages();
```

### Pages Razor

Le dashboard fournit les pages suivantes :

- `/features` - Liste de toutes les features
- `/features/{name}` - Détails d'une feature
- `/features/{name}/toggle` - Activer/désactiver

### API Dashboard

```csharp
// GET /api/features
// Retourne toutes les features

// GET /api/features/{name}
// Retourne les détails d'une feature

// POST /api/features/{name}/enable
// Active une feature

// POST /api/features/{name}/disable
// Désactive une feature
```

---

## Best Practices

### Naming Convention

```csharp
// Utiliser PascalCase
[FeatureGate("NewDashboard")]
[FeatureGate("ExportToPdf")]
[FeatureGate("BetaFeatures")]

// Préfixer par module
[FeatureGate("Reports.ExportToPdf")]
[FeatureGate("Users.SelfRegistration")]
```

### Feature Lifecycle

```csharp
// 1. Feature flag pour nouveau développement
[FeatureGate("NewCheckout")]
public async Task<IActionResult> Checkout() { }

// 2. Feature rollout progressif
[FeatureGate("NewCheckout", Percentage = 10)]
public async Task<IActionResult> Checkout() { }

// 3. Feature cleanup après validation
// Supprimer le feature flag quand la feature est stable
public async Task<IActionResult> Checkout() { }
```

### Feature Toggles vs Branching

```csharp
// ✅ Feature toggle - recommandé
[FeatureGate("NewFeature")]
public void NewFeature() { }

// ❌ Branching dans le code - à éviter
if (config.UseNewFeature)
{
    NewFeature();
}
else
{
    OldFeature();
}
```

---

## Voir aussi

- [Security](security.md) - Authorization basée sur les features
- [API Versioning](api-versioning.md) - Versioning avec features
- [Caching](caching.md) - Cache des configurations
