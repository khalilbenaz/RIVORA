namespace RVR.Framework.Privacy;

using RVR.Framework.Privacy.Models;

/// <summary>
/// Core privacy service providing GDPR compliance operations including
/// data subject request processing, consent management, and data anonymization.
/// </summary>
public interface IPrivacyService
{
    /// <summary>
    /// Processes a data subject request (access, rectification, erasure, or portability).
    /// </summary>
    /// <param name="request">The data subject request to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated request with processing results.</returns>
    Task<DataSubjectRequest> ProcessDataSubjectRequestAsync(
        DataSubjectRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports all personal data associated with a data subject in a portable format.
    /// Supports GDPR Article 20 (Right to data portability).
    /// </summary>
    /// <param name="subjectId">The identifier of the data subject.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A dictionary containing the exported personal data.</returns>
    Task<Dictionary<string, object?>> ExportPersonalDataAsync(
        string subjectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Anonymizes all personal data associated with a data subject.
    /// Supports GDPR Article 17 (Right to erasure).
    /// </summary>
    /// <param name="subjectId">The identifier of the data subject.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if anonymization was successful.</returns>
    Task<bool> AnonymizeDataAsync(
        string subjectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all consent records for a data subject.
    /// </summary>
    /// <param name="subjectId">The identifier of the data subject.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of consent records.</returns>
    Task<IEnumerable<ConsentRecord>> GetConsentRecordsAsync(
        string subjectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records consent given by a data subject for a specific purpose.
    /// </summary>
    /// <param name="record">The consent record to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The stored consent record.</returns>
    Task<ConsentRecord> RecordConsentAsync(
        ConsentRecord record,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes consent previously given by a data subject for a specific purpose.
    /// </summary>
    /// <param name="subjectId">The identifier of the data subject.</param>
    /// <param name="purpose">The purpose for which consent is being revoked.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if consent was found and revoked.</returns>
    Task<bool> RevokeConsentAsync(
        string subjectId,
        string purpose,
        CancellationToken cancellationToken = default);
}
