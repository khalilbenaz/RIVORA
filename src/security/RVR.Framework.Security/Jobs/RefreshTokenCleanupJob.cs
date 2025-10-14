namespace RVR.Framework.Security.Jobs;

using System;
using System.Threading;
using System.Threading.Tasks;
using RVR.Framework.Security.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Configuration options for the refresh token cleanup job.
/// </summary>
public class RefreshTokenCleanupOptions
{
    /// <summary>
    /// Gets or sets the interval between cleanup runs.
    /// Default is 1 hour.
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets or sets whether the cleanup job is enabled.
    /// Default is true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the initial delay before the first cleanup run.
    /// Default is 5 minutes.
    /// </summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMinutes(5);
}

/// <summary>
/// Background service that periodically cleans up expired and revoked refresh tokens.
/// </summary>
public class RefreshTokenCleanupJob : BackgroundService
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly RefreshTokenCleanupOptions _options;
    private readonly ILogger<RefreshTokenCleanupJob> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenCleanupJob"/> class.
    /// </summary>
    /// <param name="refreshTokenService">The refresh token service.</param>
    /// <param name="options">The cleanup options.</param>
    /// <param name="logger">The logger.</param>
    public RefreshTokenCleanupJob(
        IRefreshTokenService refreshTokenService,
        IOptions<RefreshTokenCleanupOptions> options,
        ILogger<RefreshTokenCleanupJob> logger)
    {
        _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(refreshTokenService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Refresh token cleanup job is disabled.");
            return;
        }

        _logger.LogInformation(
            "Refresh token cleanup job started. Running every {Interval}.",
            _options.CleanupInterval);

        // Initial delay
        await Task.Delay(_options.InitialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Starting refresh token cleanup cycle.");

                var cleanedCount = await _refreshTokenService.CleanupAsync(stoppingToken);

                _logger.LogInformation(
                    "Refresh token cleanup completed. Cleaned up {Count} tokens.",
                    cleanedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error during refresh token cleanup cycle.");
            }

            // Wait for the next cleanup interval
            await Task.Delay(_options.CleanupInterval, stoppingToken);
        }

        _logger.LogInformation("Refresh token cleanup job stopped.");
    }
}
