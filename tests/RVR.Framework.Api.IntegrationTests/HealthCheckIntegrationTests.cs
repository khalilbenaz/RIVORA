namespace RVR.Framework.Api.IntegrationTests;

using Infrastructure;

[Collection("Testcontainers")]
public class HealthCheckIntegrationTests : IClassFixture<TestcontainersFixture>
{
    private readonly HttpClient _client;

    public HealthCheckIntegrationTests(TestcontainersFixture containers)
    {
        var factory = new IntegrationTestWebApplicationFactory(containers);
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health/live");
        response.EnsureSuccessStatusCode();
    }
}
