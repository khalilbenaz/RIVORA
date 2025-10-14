namespace RVR.Framework.Dapr.Models;

/// <summary>
/// Configuration options for the RIVORA Framework Dapr integration.
/// </summary>
public class DaprOptions
{
    /// <summary>
    /// The HTTP endpoint for the Dapr sidecar. Defaults to "http://localhost:3500".
    /// </summary>
    public string HttpEndpoint { get; set; } = "http://localhost:3500";

    /// <summary>
    /// The gRPC endpoint for the Dapr sidecar. Defaults to "http://localhost:50001".
    /// </summary>
    public string GrpcEndpoint { get; set; } = "http://localhost:50001";
}
