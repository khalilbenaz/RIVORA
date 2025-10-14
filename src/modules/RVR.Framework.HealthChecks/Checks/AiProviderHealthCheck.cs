namespace RVR.Framework.HealthChecks.Checks;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Health check for AI provider services (OpenAI, Anthropic, Google, etc.).
/// </summary>
public class AiProviderHealthCheck : IHealthCheck
{
    internal readonly string _providerName;
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly HttpClient _httpClient;
    private readonly TimeSpan _timeout;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiProviderHealthCheck"/> class.
    /// </summary>
    /// <param name="providerName">The name of the AI provider (e.g., "OpenAI", "Anthropic").</param>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="endpoint">The API endpoint URL.</param>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    /// <param name="timeout">The timeout for health check requests.</param>
    public AiProviderHealthCheck(
        string providerName,
        string apiKey,
        string endpoint,
        HttpClient httpClient,
        TimeSpan? timeout = null)
    {
        _providerName = providerName ?? throw new ArgumentNullException(nameof(providerName));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _timeout = timeout ?? TimeSpan.FromSeconds(10);
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_timeout);

        try
        {
            var isHealthy = await TestProviderAsync(cts.Token);

            if (isHealthy)
            {
                return HealthCheckResult.Healthy(
                    $"AI provider {_providerName} is accessible and responsive",
                    new Dictionary<string, object> { ["Provider"] = _providerName, ["Endpoint"] = _endpoint });
            }

            return HealthCheckResult.Degraded(
                $"AI provider {_providerName} is accessible but not responding as expected",
                null,
                new Dictionary<string, object> { ["Provider"] = _providerName });
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy(
                $"AI provider {_providerName} health check timed out",
                null,
                new Dictionary<string, object> { ["Provider"] = _providerName, ["Timeout"] = _timeout });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                $"AI provider {_providerName} health check failed",
                ex,
                new Dictionary<string, object> { ["Provider"] = _providerName });
        }
    }

    private async Task<bool> TestProviderAsync(CancellationToken cancellationToken)
    {
        // Default implementation - performs a simple connectivity test
        // In real scenarios, this would make an actual API call to the provider

        var request = new HttpRequestMessage(HttpMethod.Get, _endpoint);
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.SendAsync(request, cancellationToken);

        // Consider 2xx and 429 (rate limited but service is up) as healthy
        return ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300) ||
               response.StatusCode == System.Net.HttpStatusCode.TooManyRequests;
    }
}

/// <summary>
/// Health check for multiple AI providers.
/// </summary>
public class MultiAiProviderHealthCheck : IHealthCheck
{
    private readonly IEnumerable<AiProviderHealthCheck> _providerChecks;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiAiProviderHealthCheck"/> class.
    /// </summary>
    /// <param name="providerChecks">The individual provider health checks.</param>
    public MultiAiProviderHealthCheck(IEnumerable<AiProviderHealthCheck> providerChecks)
    {
        _providerChecks = providerChecks ?? throw new ArgumentNullException(nameof(providerChecks));
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, bool>();
        var healthyCount = 0;
        var totalCount = 0;

        foreach (var check in _providerChecks)
        {
            totalCount++;
            var result = await check.CheckHealthAsync(context, cancellationToken);
            results[check._providerName] = result.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy;

            if (result.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy)
            {
                healthyCount++;
            }
        }

        if (healthyCount == totalCount)
        {
            return HealthCheckResult.Healthy(
                $"All {totalCount} AI providers are healthy",
                new Dictionary<string, object> { ["Providers"] = results });
        }

        if (healthyCount > 0)
        {
            return HealthCheckResult.Degraded(
                $"{healthyCount}/{totalCount} AI providers are healthy",
                null,
                new Dictionary<string, object> { ["Providers"] = results, ["HealthyCount"] = healthyCount, ["TotalCount"] = totalCount });
        }

        return HealthCheckResult.Unhealthy(
            $"No AI providers are healthy (0/{totalCount})",
            null,
            new Dictionary<string, object> { ["Providers"] = results });
    }
}
