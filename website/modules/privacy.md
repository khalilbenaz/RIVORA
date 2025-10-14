# GDPR Privacy Toolkit

The RIVORA Framework includes a built-in GDPR compliance toolkit that helps you mark, track, and manage personal data across your application.

## Data Classification Attributes

Mark entity properties with attributes to classify personal data:

```csharp
using RVR.Framework.Privacy;

public class Customer : Entity<Guid>
{
    [PersonalData]
    public string FirstName { get; private set; }

    [PersonalData]
    public string LastName { get; private set; }

    [SensitiveData(Classification = DataClassification.Health)]
    public string MedicalNotes { get; private set; }

    [PersonalData]
    [SensitiveData(Classification = DataClassification.Financial)]
    public string BankAccount { get; private set; }

    // Not personal data -- no attribute needed
    public DateTime CreatedAt { get; private set; }
}
```

### Available Classifications

| Classification | Description |
|---------------|-------------|
| `DataClassification.General` | General personal data (name, email) |
| `DataClassification.Financial` | Bank accounts, credit card numbers |
| `DataClassification.Health` | Medical records, health data |
| `DataClassification.Biometric` | Fingerprints, facial recognition data |
| `DataClassification.Genetic` | DNA, genetic markers |
| `DataClassification.RacialEthnic` | Racial or ethnic origin |
| `DataClassification.Political` | Political opinions |
| `DataClassification.Religious` | Religious or philosophical beliefs |

## DSAR Processing (Data Subject Access Requests)

The framework provides an `IDsarService` to handle data subject requests:

```csharp
public interface IDsarService
{
    Task<DsarReport> GenerateAccessReportAsync(Guid userId, CancellationToken ct = default);
    Task ExportPersonalDataAsync(Guid userId, Stream output, ExportFormat format, CancellationToken ct = default);
    Task AnonymizeUserDataAsync(Guid userId, CancellationToken ct = default);
    Task DeleteUserDataAsync(Guid userId, CancellationToken ct = default);
}
```

### Processing a data access request

```csharp
public class DsarController : ControllerBase
{
    private readonly IDsarService _dsarService;

    [HttpGet("my-data")]
    [Authorize]
    public async Task<IActionResult> GetMyData(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var report = await _dsarService.GenerateAccessReportAsync(userId, ct);
        return Ok(report);
    }

    [HttpGet("my-data/export")]
    [Authorize]
    public async Task<IActionResult> ExportMyData(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var stream = new MemoryStream();
        await _dsarService.ExportPersonalDataAsync(userId, stream, ExportFormat.Json, ct);
        stream.Position = 0;
        return File(stream, "application/json", "my-data.json");
    }

    [HttpDelete("my-data")]
    [Authorize]
    public async Task<IActionResult> DeleteMyData(CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _dsarService.DeleteUserDataAsync(userId, ct);
        return NoContent();
    }
}
```

## Consent Management

Track and manage user consent for data processing:

```csharp
public interface IConsentManager
{
    Task<ConsentRecord> RecordConsentAsync(Guid userId, string purpose, bool granted, CancellationToken ct = default);
    Task<IReadOnlyList<ConsentRecord>> GetConsentsAsync(Guid userId, CancellationToken ct = default);
    Task RevokeConsentAsync(Guid userId, string purpose, CancellationToken ct = default);
    Task<bool> HasConsentAsync(Guid userId, string purpose, CancellationToken ct = default);
}
```

### Recording consent

```csharp
// Record consent when user accepts terms
await _consentManager.RecordConsentAsync(
    userId: currentUser.Id,
    purpose: "marketing-emails",
    granted: true);

// Check consent before processing
if (await _consentManager.HasConsentAsync(userId, "marketing-emails"))
{
    await _emailService.SendMarketingEmailAsync(userId);
}

// Revoke consent
await _consentManager.RevokeConsentAsync(userId, "marketing-emails");
```

## Data Anonymization

Anonymize user data while preserving referential integrity:

```csharp
// Anonymize a specific user
await _dsarService.AnonymizeUserDataAsync(userId);

// After anonymization:
// - FirstName -> "ANONYMIZED"
// - LastName -> "ANONYMIZED"
// - Email -> "anonymized-{hash}@deleted.local"
// - All [SensitiveData] fields -> null or default
// - Audit trail is preserved with anonymized references
```

## Registration

```csharp
// In Program.cs or Startup.cs
builder.Services.AddRvrPrivacy(options =>
{
    options.DataRetentionDays = 365;
    options.AutoAnonymizeOnDeletion = true;
    options.ConsentRequired = true;
});
```

## Scanning for Personal Data

Use the CLI to scan your project for personal data:

```bash
rivora privacy scan --project ./src/Core/Domain
```

This produces a report of all entities with `[PersonalData]` and `[SensitiveData]` attributes, useful for maintaining your GDPR data registry.
