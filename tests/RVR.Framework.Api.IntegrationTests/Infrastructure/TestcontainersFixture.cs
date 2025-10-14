namespace RVR.Framework.Api.IntegrationTests.Infrastructure;

using Testcontainers.MsSql;
using Testcontainers.Redis;

public class TestcontainersFixture : IAsyncLifetime
{
    public MsSqlContainer SqlServer { get; } = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Test_P@ssw0rd!")
        .Build();

    public RedisContainer Redis { get; } = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    public string SqlConnectionString => SqlServer.GetConnectionString();
    public string RedisConnectionString => $"{Redis.Hostname}:{Redis.GetMappedPublicPort(6379)}";

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            SqlServer.StartAsync(),
            Redis.StartAsync());
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            SqlServer.DisposeAsync().AsTask(),
            Redis.DisposeAsync().AsTask());
    }
}
