using RVR.Framework.Application.DTOs.Products;
using RVR.Framework.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace RVR.Framework.Api.IntegrationTests.Controllers;

/// <summary>
/// Tests d'intégration pour ProductsController
/// </summary>
public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public ProductsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remplacer le DbContext par une base de données en mémoire
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<RVRDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<RVRDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Créer la base de données
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<RVRDbContext>();
                db.Database.EnsureCreated();
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WithValidData()
    {
        // Arrange
        var newProduct = new CreateProductDto(
            "Integration Test Product",
            "Test Description",
            99.99m,
            10,
            "SKU-TEST",
            "TestCategory"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", newProduct);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdProduct = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(createdProduct);
        Assert.Equal("Integration Test Product", createdProduct.Name);
        Assert.Equal(99.99m, createdProduct.Price);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenProductExists()
    {
        // Arrange - Create a product first
        var newProduct = new CreateProductDto(
            "Test Product for Get",
            "Description",
            50m,
            5,
            null,
            null
        );

        var createResponse = await _client.PostAsJsonAsync("/api/products", newProduct);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        // Act
        var response = await _client.GetAsync($"/api/products/{createdProduct!.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(product);
        Assert.Equal(createdProduct.Id, product.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/products/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenProductExists()
    {
        // Arrange - Create a product first
        var newProduct = new CreateProductDto(
            "Product to Update",
            "Original Description",
            100m,
            10,
            null,
            null
        );

        var createResponse = await _client.PostAsJsonAsync("/api/products", newProduct);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var updateDto = new UpdateProductDto(
            "Updated Product Name",
            "Updated Description",
            150m,
            "NEW-SKU",
            "NewCategory"
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{createdProduct!.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedProduct = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(updatedProduct);
        Assert.Equal("Updated Product Name", updatedProduct.Name);
        Assert.Equal(150m, updatedProduct.Price);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenProductExists()
    {
        // Arrange - Create a product first
        var newProduct = new CreateProductDto(
            "Product to Delete",
            "Description",
            100m,
            10,
            null,
            null
        );

        var createResponse = await _client.PostAsJsonAsync("/api/products", newProduct);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/products/{createdProduct!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the product is deleted
        var getResponse = await _client.GetAsync($"/api/products/{createdProduct.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
