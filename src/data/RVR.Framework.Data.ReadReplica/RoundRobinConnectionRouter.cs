namespace RVR.Framework.Data.ReadReplica;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// A connection router that distributes read-only queries across read replicas
/// using a round-robin strategy.
/// </summary>
public class RoundRobinConnectionRouter : IDbConnectionRouter
{
    private readonly ReadReplicaOptions _options;
    private readonly ILogger<RoundRobinConnectionRouter> _logger;
    private int _currentIndex = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoundRobinConnectionRouter"/> class.
    /// </summary>
    /// <param name="options">The read replica configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public RoundRobinConnectionRouter(
        IOptions<ReadReplicaOptions> options,
        ILogger<RoundRobinConnectionRouter> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public string GetConnectionString(bool isReadOnly)
    {
        if (!isReadOnly || _options.ReadConnectionStrings.Length == 0)
        {
            _logger.LogDebug("Routing to primary (write) database.");
            return _options.WriteConnectionString;
        }

        var index = Interlocked.Increment(ref _currentIndex);
        var replicaIndex = ((index % _options.ReadConnectionStrings.Length) + _options.ReadConnectionStrings.Length)
                           % _options.ReadConnectionStrings.Length;

        _logger.LogDebug("Routing to read replica {ReplicaIndex} of {TotalReplicas}.",
            replicaIndex, _options.ReadConnectionStrings.Length);

        return _options.ReadConnectionStrings[replicaIndex];
    }
}
