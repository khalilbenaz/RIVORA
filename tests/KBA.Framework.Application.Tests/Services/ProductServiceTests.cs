using KBA.Framework.Application.DTOs.Products;
using KBA.Framework.Application.Services;
using KBA.Framework.Domain.Entities.Products;
using KBA.Framework.Domain.Repositories;
using Moq;

namespace KBA.Framework.Application.Tests.Services;

/// <summary>
/// Tests pour ProductService
/// </summary>
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<ICurrentUserContext> _mockUserContext;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockUserContext = new Mock<ICurrentUserContext>();

        // Configuration par défaut du contexte utilisateur
        _mockUserContext.Setup(x => x.TenantId).Returns((Guid?)null);
        _mockUserContext.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);

        _service = new ProductService(_mockRepository.Object, _mockUserContext.Object);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product(null, "Test Product", 100m, 10);
        typeof(Product).GetProperty("Id")!.SetValue(product, productId);

        _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _service.GetAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(100m, result.Price);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _service.GetAsync(productId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateProduct_WithValidData()
    {
        // Arrange
        var dto = new CreateProductDto("New Product", "Description", 99.99m, 5, "SKU123", "Category1");

        _mockRepository.Setup(r => r.InsertAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken ct) => p);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Product", result.Name);
        Assert.Equal(99.99m, result.Price);
        Assert.Equal(5, result.Stock);
        _mockRepository.Verify(r => r.InsertAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product(null, "Old Name", 50m, 10);
        typeof(Product).GetProperty("Id")!.SetValue(product, productId);

        var dto = new UpdateProductDto("Updated Name", "New Description", 75m, "SKU456", "Category2");

        _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken ct) => p);

        // Act
        var result = await _service.UpdateAsync(productId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(75m, result.Price);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenProductNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var dto = new UpdateProductDto("Updated Name", "Description", 75m, null, null);

        _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(productId, dto));
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product(null, "Test Product", 100m, 10);
        typeof(Product).GetProperty("Id")!.SetValue(product, productId);

        _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(productId);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product(null, "Product 1", 100m, 10),
            new Product(null, "Product 2", 200m, 20)
        };

        _mockRepository.Setup(r => r.GetListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _service.GetListAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }
}
