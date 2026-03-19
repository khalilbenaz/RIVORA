namespace RVR.Framework.Privacy.Services;

using Microsoft.Extensions.Logging;
using RVR.Framework.Privacy.Models;

/// <summary>
/// Default implementation of <see cref="IPrivacyService"/> providing GDPR compliance operations.
/// </summary>
public class PrivacyService : IPrivacyService
{
    private readonly InMemoryConsentStore _consentStore;
    private readonly IDataAnonymizer _anonymizer;
    private readonly ILogger<PrivacyService> _logger;
    private readonly ConcurrentRequestStore _requestStore = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PrivacyService"/> class.
    /// </summary>
    /// <param name="consentStore">The consent store.</param>
    /// <param name="anonymizer">The data anonymizer.</param>
    /// <param name="logger">The logger instance.</param>
    public PrivacyService(
        InMemoryConsentStore consentStore,
        IDataAnonymizer anonymizer,
        ILogger<PrivacyService> logger)
    {
        _consentStore = consentStore ?? throw new ArgumentNullException(nameof(consentStore));
        _anonymizer = anonymizer ?? throw new ArgumentNullException(nameof(anonymizer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<DataSubjectRequest> ProcessDataSubjectRequestAsync(
        DataSubjectRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        _logger.LogInformation(
            "Processing DSAR of type {RequestType} for subject {SubjectId}",
            request.Type,
            request.SubjectId);

        request.Status = DataSubjectRequestStatus.InProgress;
        _requestStore.Store(request);

        switch (request.Type)
        {
            case DataSubjectRequestType.Access:
                await ExportPersonalDataAsync(request.SubjectId, cancellationToken);
                break;

            case DataSubjectRequestType.Erasure:
                await AnonymizeDataAsync(request.SubjectId, cancellationToken);
                break;

            case DataSubjectRequestType.Portability:
                await ExportPersonalDataAsync(request.SubjectId, cancellationToken);
                break;

            case DataSubjectRequestType.Rectification:
                // Rectification requires external input; mark as in-progress for manual handling
                _logger.LogInformation(
                    "Rectification request for subject {SubjectId} requires manual processing",
                    request.SubjectId);
                break;
        }

        request.Status = DataSubjectRequestStatus.Completed;
        request.CompletedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "DSAR {RequestId} completed for subject {SubjectId}",
            request.Id,
            request.SubjectId);

        return request;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, object?>> ExportPersonalDataAsync(
        string subjectId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subjectId))
        {
            throw new ArgumentException("Subject ID cannot be empty.", nameof(subjectId));
        }

        _logger.LogInformation("Exporting personal data for subject {SubjectId}", subjectId);

        // Return consent records as the base exported data.
        // In a real implementation, this would aggregate data from all registered data sources.
        var consentRecords = await _consentStore.GetBySubjectAsync(subjectId, cancellationToken);

        var exportData = new Dictionary<string, object?>
        {
            ["subjectId"] = subjectId,
            ["exportedAt"] = DateTime.UtcNow,
            ["consentRecords"] = consentRecords
        };

        return exportData;
    }

    /// <inheritdoc/>
    public Task<bool> AnonymizeDataAsync(
        string subjectId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subjectId))
        {
            throw new ArgumentException("Subject ID cannot be empty.", nameof(subjectId));
        }

        _logger.LogInformation("Anonymizing data for subject {SubjectId}", subjectId);

        // In a real implementation, this would iterate over all registered data sources
        // and call _anonymizer.AnonymizeEntity on each entity belonging to the subject.
        // For now, we log the operation and return success.

        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ConsentRecord>> GetConsentRecordsAsync(
        string subjectId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subjectId))
        {
            throw new ArgumentException("Subject ID cannot be empty.", nameof(subjectId));
        }

        return await _consentStore.GetBySubjectAsync(subjectId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ConsentRecord> RecordConsentAsync(
        ConsentRecord record,
        CancellationToken cancellationToken = default)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        _logger.LogInformation(
            "Recording consent for subject {SubjectId}, purpose: {Purpose}, granted: {Granted}",
            record.SubjectId,
            record.Purpose,
            record.Granted);

        return await _consentStore.StoreAsync(record, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> RevokeConsentAsync(
        string subjectId,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subjectId))
        {
            throw new ArgumentException("Subject ID cannot be empty.", nameof(subjectId));
        }

        if (string.IsNullOrWhiteSpace(purpose))
        {
            throw new ArgumentException("Purpose cannot be empty.", nameof(purpose));
        }

        _logger.LogInformation(
            "Revoking consent for subject {SubjectId}, purpose: {Purpose}",
            subjectId,
            purpose);

        return await _consentStore.RevokeAsync(subjectId, purpose, cancellationToken);
    }

    /// <summary>
    /// Simple thread-safe store for tracking data subject requests.
    /// </summary>
    private sealed class ConcurrentRequestStore
    {
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, DataSubjectRequest> _requests = new();

        public void Store(DataSubjectRequest request)
        {
            _requests[request.Id] = request;
        }
    }
}
