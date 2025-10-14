namespace RVR.Framework.Privacy.Services;

using System.Collections.Concurrent;
using RVR.Framework.Privacy.Models;

/// <summary>
/// In-memory implementation of consent record storage.
/// Suitable for development and testing; use a persistent store in production.
/// </summary>
public class InMemoryConsentStore
{
    private readonly ConcurrentDictionary<string, List<ConsentRecord>> _records = new();
    private readonly object _lock = new();

    /// <summary>
    /// Stores a consent record.
    /// </summary>
    /// <param name="record">The consent record to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The stored consent record.</returns>
    public Task<ConsentRecord> StoreAsync(ConsentRecord record, CancellationToken cancellationToken = default)
    {
        if (record == null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        lock (_lock)
        {
            var records = _records.GetOrAdd(record.SubjectId, _ => new List<ConsentRecord>());
            records.Add(record);
        }

        return Task.FromResult(record);
    }

    /// <summary>
    /// Retrieves all consent records for a given data subject.
    /// </summary>
    /// <param name="subjectId">The identifier of the data subject.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of consent records.</returns>
    public Task<IEnumerable<ConsentRecord>> GetBySubjectAsync(
        string subjectId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subjectId))
        {
            throw new ArgumentException("Subject ID cannot be empty.", nameof(subjectId));
        }

        if (_records.TryGetValue(subjectId, out var records))
        {
            lock (_lock)
            {
                return Task.FromResult<IEnumerable<ConsentRecord>>(records.ToList());
            }
        }

        return Task.FromResult<IEnumerable<ConsentRecord>>(Array.Empty<ConsentRecord>());
    }

    /// <summary>
    /// Revokes consent for a specific subject and purpose.
    /// </summary>
    /// <param name="subjectId">The identifier of the data subject.</param>
    /// <param name="purpose">The purpose for which consent is being revoked.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a matching active consent was found and revoked.</returns>
    public Task<bool> RevokeAsync(
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

        if (!_records.TryGetValue(subjectId, out var records))
        {
            return Task.FromResult(false);
        }

        lock (_lock)
        {
            var activeConsent = records.FirstOrDefault(r =>
                r.Purpose == purpose && r.IsActive);

            if (activeConsent == null)
            {
                return Task.FromResult(false);
            }

            activeConsent.RevokedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }
    }
}
