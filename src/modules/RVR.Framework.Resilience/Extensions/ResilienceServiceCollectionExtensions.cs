using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace RVR.Framework.Resilience.Extensions;

/// <summary>
/// Extension methods for registering resilience policies.
/// </summary>
public static class ResilienceServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Rivora resilience pipeline with retry, circuit breaker, and timeout policies.
    /// Configures a default resilient HTTP client named "RvrResilient".
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="httpClientName">The named HTTP client to configure. Defaults to "RvrResilient".</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrResilience(
        this IServiceCollection services,
        string httpClientName = "RvrResilient")
    {
        services.AddHttpClient(httpClientName)
            .AddResilienceHandler("rvr-resilience", builder =>
            {
                // Retry: 3 attempts with exponential backoff
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    Delay = TimeSpan.FromSeconds(1)
                });

                // Circuit breaker
                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    FailureRatio = 0.5,
                    MinimumThroughput = 10,
                    BreakDuration = TimeSpan.FromSeconds(15)
                });

                // Timeout
                builder.AddTimeout(TimeSpan.FromSeconds(10));
            });

        return services;
    }
}
