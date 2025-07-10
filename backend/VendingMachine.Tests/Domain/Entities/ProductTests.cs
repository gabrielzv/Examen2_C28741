using VendingMachine.Domain.Entities;
using VendingMachine.Domain.ValueObjects;
using Xunit;

namespace VendingMachine.Tests.Domain.Entities;

public class ProductTests
{
    [Fact]
    public void Product_Constructor_ShouldCreateValidProduct()
    {
        // Arrange
        var id = new ProductId("1");
        var name = "Coca Cola";
        var price = new Money(800);
        var quantity = 10;

        // Act
        var product = new Product(id, name, price, quantity);

        // Assert
        Assert.Equal(id, product.Id);
        Assert.Equal(name, product.Name);
        Assert.Equal(price, product.Price);
        Assert.Equal(quantity, product.Quantity);
        Assert.True(product.IsAvailable);
    }

    [Fact]
    public void Product_Constructor_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var id = new ProductId("1");
        var name = "";
        var price = new Money(800);
        var quantity = 10;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Product(id, name, price, quantity));
    }

    [Fact]
    public void Product_Constructor_WithNegativeQuantity_ShouldThrowException()
    {
        // Arrange
        var id = new ProductId("1");
        var name = "Coca Cola";
        var price = new Money(800);
        var quantity = -1;

        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => new Product(id, name, price, quantity));
    }

    [Fact]
    public void Product_DecreaseQuantity_ShouldReduceQuantity()
    {
        // Arrange
        var product = new Product(new ProductId("1"), "Coca Cola", new Money(800), 10);

        // Act
        product.DecreaseQuantity(3);

        // Assert
        Assert.Equal(7, product.Quantity);
    }

    [Fact]
    public void Product_DecreaseQuantity_WithInsufficientStock_ShouldThrowException()
    {
        // Arrange
        var product = new Product(new ProductId("1"), "Coca Cola", new Money(800), 5);

        // Act
        // Assert
        Assert.Throws<InvalidOperationException>(() => product.DecreaseQuantity(10));
    }

    [Fact]
    public void Product_IncreaseQuantity_ShouldAddQuantity()
    {
        // Arrange
        var product = new Product(new ProductId("1"), "Coca Cola", new Money(800), 10);

        // Act
        product.IncreaseQuantity(5);

        // Assert
        Assert.Equal(15, product.Quantity);
    }

    [Fact]
    public void Product_IsAvailable_WithZeroQuantity_ShouldReturnFalse()
    {
        // Arrange
        var product = new Product(new ProductId("1"), "Coca Cola", new Money(800), 0);

        // Act
        // Assert
        Assert.False(product.IsAvailable);
    }

    [Fact]
    public void Product_IsAvailable_WithPositiveQuantity_ShouldReturnTrue()
    {
        // Arrange
        var product = new Product(new ProductId("1"), "Coca Cola", new Money(800), 1);

        // Act
        // Assert
        Assert.True(product.IsAvailable);
    }
}
