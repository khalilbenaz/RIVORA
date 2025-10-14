# Dynamic Localization

The RIVORA Framework provides a database-backed localization system with hot reload, supporting multi-culture content management without redeployment.

## Core Abstractions

### ILocalizationStore

The store reads and writes localization entries from a persistent backend:

```csharp
public interface ILocalizationStore
{
    Task<string?> GetStringAsync(string key, string culture, CancellationToken ct = default);
    Task SetStringAsync(string key, string culture, string value, CancellationToken ct = default);
    Task<IReadOnlyDictionary<string, string>> GetAllStringsAsync(string culture, CancellationToken ct = default);
    Task DeleteStringAsync(string key, string culture, CancellationToken ct = default);
}
```

### DatabaseStringLocalizer

Implements `IStringLocalizer` backed by the database, with in-memory caching and automatic invalidation:

```csharp
public class DatabaseStringLocalizer : IStringLocalizer
{
    // Automatically resolves culture from CultureInfo.CurrentUICulture
    // Falls back to parent culture, then to default culture
    // Caches results in-memory with configurable TTL
}
```

## Registration

```csharp
builder.Services.AddRvrLocalization(options =>
{
    options.DefaultCulture = "fr-FR";
    options.SupportedCultures = new[] { "fr-FR", "en-US", "ar-MA", "de-DE" };
    options.CacheDuration = TimeSpan.FromMinutes(30);
    options.EnableHotReload = true;
    options.FallbackToDefaultCulture = true;
});
```

## Usage in Controllers

```csharp
public class ProductsController : ControllerBase
{
    private readonly IStringLocalizer _localizer;

    public ProductsController(IStringLocalizer localizer)
    {
        _localizer = localizer;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productService.GetAsync(id);
        if (product == null)
            return NotFound(_localizer["Product.NotFound"]);

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        if (dto.Price < 0)
            return BadRequest(_localizer["Product.InvalidPrice"]);

        var product = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = product.Id },
            new { product, message = _localizer["Product.Created"] });
    }
}
```

## Usage in Razor/Blazor Views

```razor
@inject IStringLocalizer Localizer

<h1>@Localizer["Dashboard.Title"]</h1>
<p>@Localizer["Dashboard.Welcome", userName]</p>
```

## Managing Translations at Runtime

Add or update translations without redeploying:

```csharp
public class LocalizationAdminController : ControllerBase
{
    private readonly ILocalizationStore _store;

    [HttpPost("translations")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetTranslation(
        [FromBody] SetTranslationDto dto, CancellationToken ct)
    {
        await _store.SetStringAsync(dto.Key, dto.Culture, dto.Value, ct);
        return Ok();
    }

    [HttpGet("translations/{culture}")]
    public async Task<IActionResult> GetAll(string culture, CancellationToken ct)
    {
        var strings = await _store.GetAllStringsAsync(culture, ct);
        return Ok(strings);
    }
}
```

## Hot Reload

When `EnableHotReload` is `true`, the localizer watches for changes in the database and invalidates its cache automatically. Changes made via the admin API or directly in the database are reflected within the `CacheDuration` window.

## Culture Resolution

The framework resolves the current culture in this order:

1. `Accept-Language` HTTP header
2. `culture` query string parameter
3. User profile preference (if authenticated)
4. Default culture from configuration

```csharp
// In Program.cs
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("fr-FR"),
    SupportedCultures = new[] { new CultureInfo("fr-FR"), new CultureInfo("en-US") },
    SupportedUICultures = new[] { new CultureInfo("fr-FR"), new CultureInfo("en-US") }
});
```
