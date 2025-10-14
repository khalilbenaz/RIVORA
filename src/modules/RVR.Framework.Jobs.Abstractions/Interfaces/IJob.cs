namespace RVR.Framework.Jobs.Abstractions.Interfaces;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Base interface for all jobs in the RIVORA Framework.
/// </summary>
public interface IJob
{
    /// <summary>
    /// Executes the job asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the job execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
