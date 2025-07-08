using VendingMachine.Domain.ValueObjects;
using Xunit;

namespace VendingMachine.Tests.Domain.ValueObjects;

public class ProductIdTests
{
    [Fact]
    public void ProductId_Constructor_WithValidId_ShouldCreateProductId()
    {
        // Arrange
        var id = "1";

        // Act
        var productId = new ProductId(id);

        // Assert
        Assert.Equal(id.ToUpperInvariant(), productId.Value);
    }

    [Fact]
    public void ProductId_Constructor_WithEmptyId_ShouldThrowException()
    {
        // Arrange
        var id = "";

        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => new ProductId(id));
    }

    [Fact]
    public void ProductId_Constructor_WithNullId_ShouldThrowException()
    {
        // Arrange
        string id = null!;

        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => new ProductId(id));
    }

    [Fact]
    public void ProductId_Equality_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var productId1 = new ProductId("1");
        var productId2 = new ProductId("1");

        // Act & Assert
        Assert.Equal(productId1, productId2);
        Assert.True(productId1 == productId2);
        Assert.False(productId1 != productId2);
    }

    [Fact]
    public void ProductId_Equality_WithDifferentId_ShouldNotBeEqual()
    {
        // Arrange
        var productId1 = new ProductId("1");
        var productId2 = new ProductId("2");

        // Act & Assert
        Assert.NotEqual(productId1, productId2);
        Assert.False(productId1 == productId2);
        Assert.True(productId1 != productId2);
    }

    [Fact]
    public void ProductId_GetHashCode_WithSameId_ShouldBeSame()
    {
        // Arrange
        var productId1 = new ProductId("1");
        var productId2 = new ProductId("1");

        // Act & Assert
        Assert.Equal(productId1.GetHashCode(), productId2.GetHashCode());
    }

    [Fact]
    public void ProductId_ToString_ShouldReturnIdValue()
    {
        // Arrange
        var productId = new ProductId("123");

        // Act
        var result = productId.ToString();

        // Assert
        Assert.Equal("123", result);
    }
}
