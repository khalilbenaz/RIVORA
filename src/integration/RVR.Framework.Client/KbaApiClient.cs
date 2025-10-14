namespace RVR.Framework.Client;

/// <summary>
/// Deprecated: Use <see cref="RivoraApiClient"/> instead.
/// </summary>
[System.Obsolete("Use RivoraApiClient instead. This class will be removed in a future version.")]
public class KbaApiClient : RivoraApiClient
{
    /// <summary>
    /// Initializes a new instance of <see cref="KbaApiClient"/>.
    /// </summary>
    public KbaApiClient(HttpClient httpClient) : base(httpClient) { }
}
