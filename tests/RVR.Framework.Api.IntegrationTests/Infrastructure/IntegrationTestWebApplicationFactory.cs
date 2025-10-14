namespace RVR.Framework.Api.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.Infrastructure.Data;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IClassFixture<TestcontainersFixture>
{
    private readonly TestcontainersFixture _containers;

    public IntegrationTestWebApplicationFactory(TestcontainersFixture containers)
    {
        _containers = containers;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RVRDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Use Testcontainers SQL Server
            services.AddDbContext<RVRDbContext>(options =>
                options.UseSqlServer(_containers.SqlConnectionString));
        });
    }
}
