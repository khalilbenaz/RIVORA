# GDPR Compliance Guide

This guide walks through implementing GDPR compliance in your RIVORA Framework application using the built-in Privacy Toolkit.

## Step 1: Mark Personal Data with Attributes

Annotate all entity properties that contain personal data:

```csharp
using RVR.Framework.Privacy;

public class Customer : Entity<Guid>
{
    [PersonalData]
    public string FirstName { get; private set; }

    [PersonalData]
    public string LastName { get; private set; }

    [PersonalData]
    public string Email { get; private set; }

    [PersonalData]
    public string? PhoneNumber { get; private set; }

    [SensitiveData(Classification = DataClassification.Financial)]
    public string? BankAccountNumber { get; private set; }

    [SensitiveData(Classification = DataClassification.Health)]
    public string? MedicalNotes { get; private set; }

    // Not personal data
    public DateTime CreatedAt { get; private set; }
    public Guid TenantId { get; private set; }
}
```

::: tip
Use `[PersonalData]` for general PII (names, emails, phone numbers) and `[SensitiveData]` for special categories under GDPR Article 9 (health, biometric, genetic, etc.).
:::

## Step 2: Register the Privacy Toolkit

```csharp
// In Program.cs
builder.Services.AddRvrPrivacy(options =>
{
    options.DataRetentionDays = 365;           // Auto-delete after 1 year
    options.AutoAnonymizeOnDeletion = true;    // Anonymize instead of hard-delete
    options.ConsentRequired = true;            // Require consent tracking
    options.DsarResponseDeadlineDays = 30;     // GDPR mandates 30-day response
});
```

## Step 3: Implement Consent Collection

Collect user consent before processing personal data:

```csharp
[HttpPost("consent")]
[Authorize]
public async Task<IActionResult> GrantConsent(
    [FromBody] ConsentDto dto, CancellationToken ct)
{
    var userId = User.GetUserId();

    await _consentManager.RecordConsentAsync(
        userId: userId,
        purpose: dto.Purpose,
        granted: dto.Granted,
        ct: ct);

    return Ok(new { message = "Consent recorded." });
}
```

Common consent purposes:

| Purpose | Description |
|---------|-------------|
| `essential` | Core functionality (cannot be refused) |
| `analytics` | Usage analytics and telemetry |
| `marketing-emails` | Marketing communications |
| `third-party-sharing` | Sharing data with partners |
| `profiling` | Automated decision-making |

### Check consent before processing

```csharp
public async Task SendNewsletterAsync(Guid userId, CancellationToken ct)
{
    if (!await _consentManager.HasConsentAsync(userId, "marketing-emails", ct))
    {
        _logger.LogInformation("User {UserId} has not consented to marketing emails", userId);
        return;
    }

    await _emailService.SendNewsletterAsync(userId, ct);
}
```

## Step 4: Handle Data Subject Access Requests (DSARs)

### Right of Access (Article 15)

```csharp
[HttpGet("my-data")]
[Authorize]
public async Task<IActionResult> AccessMyData(CancellationToken ct)
{
    var userId = User.GetUserId();
    var report = await _dsarService.GenerateAccessReportAsync(userId, ct);

    // Report contains ALL personal data linked to this user
    // across all entities marked with [PersonalData]
    return Ok(report);
}
```

### Right to Data Portability (Article 20)

```csharp
[HttpGet("my-data/export")]
[Authorize]
public async Task<IActionResult> ExportMyData(
    [FromQuery] string format = "json", CancellationToken ct = default)
{
    var userId = User.GetUserId();
    var stream = new MemoryStream();

    var exportFormat = format.ToLower() switch
    {
        "json" => ExportFormat.Json,
        "csv" => ExportFormat.Csv,
        "xml" => ExportFormat.Xml,
        _ => ExportFormat.Json
    };

    await _dsarService.ExportPersonalDataAsync(userId, stream, exportFormat, ct);
    stream.Position = 0;

    return File(stream, $"application/{format}", $"personal-data-{userId}.{format}");
}
```

### Right to Erasure (Article 17)

```csharp
[HttpDelete("my-data")]
[Authorize]
public async Task<IActionResult> DeleteMyData(CancellationToken ct)
{
    var userId = User.GetUserId();

    // This anonymizes all [PersonalData] fields
    // and nullifies all [SensitiveData] fields
    // while preserving referential integrity
    await _dsarService.AnonymizeUserDataAsync(userId, ct);

    // Revoke all sessions
    await _sessionManager.RevokeAllSessionsAsync(userId, ct);

    return Ok(new { message = "Your personal data has been anonymized." });
}
```

### Right to Rectification (Article 16)

```csharp
[HttpPut("my-data")]
[Authorize]
public async Task<IActionResult> UpdateMyData(
    [FromBody] UpdatePersonalDataDto dto, CancellationToken ct)
{
    var userId = User.GetUserId();

    await _userService.UpdatePersonalInfoAsync(userId, new
    {
        dto.FirstName,
        dto.LastName,
        dto.PhoneNumber,
        dto.Email
    }, ct);

    return Ok(new { message = "Personal data updated." });
}
```

## Step 5: Data Retention Policies

Configure automatic data cleanup:

```csharp
// Background job for data retention
public class DataRetentionJob : IRecurringJob
{
    private readonly IDsarService _dsarService;
    private readonly IUserService _userService;

    public string CronExpression => "0 2 * * *"; // Daily at 2 AM

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var retentionDate = DateTime.UtcNow.AddDays(-365);

        // Find inactive users past retention period
        var expiredUsers = await _userService.GetInactiveUsersSinceAsync(retentionDate, ct);

        foreach (var user in expiredUsers)
        {
            await _dsarService.AnonymizeUserDataAsync(user.Id, ct);
            _logger.LogInformation("Anonymized data for expired user {UserId}", user.Id);
        }
    }
}
```

## Step 6: Audit Trail

All DSAR operations are automatically logged in the audit trail:

```csharp
// The framework logs:
// - Who requested the data export
// - When the request was made
// - What data was included
// - Whether anonymization or deletion was performed
```

## GDPR Compliance Checklist

- [ ] All personal data fields marked with `[PersonalData]`
- [ ] Special category data marked with `[SensitiveData]`
- [ ] Consent collection implemented for non-essential processing
- [ ] Data access endpoint available (Article 15)
- [ ] Data export endpoint available (Article 20)
- [ ] Data deletion/anonymization endpoint available (Article 17)
- [ ] Data rectification endpoint available (Article 16)
- [ ] Data retention policy configured and automated
- [ ] Audit trail enabled for all DSAR operations
- [ ] Privacy policy updated to reflect data processing activities
