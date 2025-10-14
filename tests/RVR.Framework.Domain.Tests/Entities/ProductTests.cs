using RVR.Framework.Domain.Entities.Products;

namespace RVR.Framework.Domain.Tests.Entities;

/// <summary>
/// Tests pour l'entité Product
/// </summary>
public class ProductTests
{
    [Fact]
    public void Constructor_ShouldCreateProduct_WithValidParameters()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var name = "Test Product";
        var price = 99.99m;
        var stock = 10;

        // Act
        var product = new Product(tenantId, name, price, stock);

        // Assert
        Assert.Equal(tenantId, product.TenantId);
        Assert.Equal(name, product.Name);
        Assert.Equal(price, product.Price);
        Assert.Equal(stock, product.Stock);
        Assert.True(product.IsActive);
        Assert.NotEqual(Guid.Empty, product.Id);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameIsEmpty()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var name = "";
        var price = 99.99m;
        var stock = 10;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Product(tenantId, name, price, stock));
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenPriceIsNegative()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var name = "Test Product";
        var price = -10m;
        var stock = 10;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Product(tenantId, name, price, stock));
    }

    [Fact]
    public void AdjustStock_ShouldIncreaseStock_WhenQuantityIsPositive()
    {
        // Arrange
        var product = new Product(null, "Test Product", 100m, 10);
        var quantityToAdd = 5;

        // Act
        product.AdjustStock(quantityToAdd);

        // Assert
        Assert.Equal(15, product.Stock);
    }

    [Fact]
    public void AdjustStock_ShouldDecreaseStock_WhenQuantityIsNegative()
    {
        // Arrange
        var product = new Product(null, "Test Product", 100m, 10);
        var quantityToRemove = -5;

        // Act
        product.AdjustStock(quantityToRemove);

        // Assert
        Assert.Equal(5, product.Stock);
    }

    [Fact]
    public void AdjustStock_ShouldThrowException_WhenResultingStockIsNegative()
    {
        // Arrange
        var product = new Product(null, "Test Product", 100m, 5);
        var quantityToRemove = -10;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => product.AdjustStock(quantityToRemove));
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var product = new Product(null, "Test Product", 100m, 10);
        product.Deactivate();

        // Act
        product.Activate();

        // Assert
        Assert.True(product.IsActive);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var product = new Product(null, "Test Product", 100m, 10);

        // Act
        product.Deactivate();

        // Assert
        Assert.False(product.IsActive);
    }

    [Fact]
    public void Update_ShouldModifyProductProperties()
    {
        // Arrange
        var product = new Product(null, "Test Product", 100m, 10);
        var newName = "Updated Product";
        var newDescription = "New Description";
        var newPrice = 150m;

        // Act
        product.Update(newName, newDescription, newPrice);

        // Assert
        Assert.Equal(newName, product.Name);
        Assert.Equal(newDescription, product.Description);
        Assert.Equal(newPrice, product.Price);
    }
}
