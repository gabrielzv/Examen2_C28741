using VendingMachine.Domain.ValueObjects;
using Xunit;

namespace VendingMachine.Tests.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Money_Constructor_WithValidAmount_ShouldCreateMoney()
    {
        // Arrange
        var amount = 100m;

        // Act
        var money = new Money(amount);

        // Assert
        Assert.Equal(amount, money.Amount);
    }

    [Fact]
    public void Money_Constructor_WithNegativeAmount_ShouldThrowException()
    {
        // Arrange
        var amount = -100m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Money(amount));
    }

    [Fact]
    public void Money_Equality_WithSameAmount_ShouldBeEqual()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(100);

        // Act & Assert
        Assert.Equal(money1, money2);
        Assert.True(money1 == money2);
        Assert.False(money1 != money2);
    }

    [Fact]
    public void Money_Equality_WithDifferentAmount_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(200);

        // Act 
        // Assert
        Assert.NotEqual(money1, money2);
        Assert.False(money1 == money2);
        Assert.True(money1 != money2);
    }

    [Fact]
    public void Money_Addition_ShouldReturnCorrectSum()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(200);

        // Act
        var result = new Money(money1.Amount + money2.Amount);

        // Assert
        Assert.Equal(300, result.Amount);
    }

    [Fact]
    public void Money_Subtraction_ShouldReturnCorrectDifference()
    {
        // Arrange
        var money1 = new Money(300);
        var money2 = new Money(100);

        // Act
        var result = new Money(money1.Amount - money2.Amount);

        // Assert
        Assert.Equal(200, result.Amount);
    }

    [Fact]
    public void Money_Multiplication_ShouldReturnCorrectProduct()
    {
        // Arrange
        var money = new Money(100);
        var multiplier = 3;

        // Act
        var result = new Money(money.Amount * multiplier);

        // Assert
        Assert.Equal(300, result.Amount);
    }

    [Fact]
    public void Money_ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var money = new Money(1500);

        // Act
        var result = money.ToString();

        // Assert
        Assert.Equal("₡1,500", result);
    }

    [Fact]
    public void Money_GetHashCode_WithSameAmount_ShouldBeSame()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(100);

        // Act & Assert
        Assert.Equal(money1.GetHashCode(), money2.GetHashCode());
    }
}
