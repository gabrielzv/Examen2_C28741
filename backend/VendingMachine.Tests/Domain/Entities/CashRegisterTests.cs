using VendingMachine.Domain.Entities;
using VendingMachine.Domain.ValueObjects;
using Xunit;

namespace VendingMachine.Tests.Domain.Entities;

public class CashRegisterTests
{
    [Fact]
    public void CashRegister_Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var cashRegister = new CashRegister();

        // Assert
        Assert.NotNull(cashRegister);
        var moneySlots = cashRegister.MoneySlots;
        Assert.NotEmpty(moneySlots);
    }

    [Fact]
    public void CashRegister_AddMoney_ShouldIncreaseQuantity()
    {
        // Arrange
        var cashRegister = new CashRegister();
        var initialQuantity = cashRegister.MoneySlots[CoinType.Coin100].Quantity;

        // Act
        cashRegister.AddMoney(CoinType.Coin100, 5);

        // Assert
        var finalQuantity = cashRegister.MoneySlots[CoinType.Coin100].Quantity;
        Assert.Equal(initialQuantity + 5, finalQuantity);
    }

    [Fact]
    public void CashRegister_CalculateChange_ShouldReturnCorrectAmount()
    {
        // Arrange
        var cashRegister = new CashRegister();
        var totalPaid = new Money(1000);
        var cost = new Money(750);

        // Act
        var change = cashRegister.CalculateChange(totalPaid, cost);

        // Assert
        Assert.Equal(250, change.Amount);
    }

    [Fact]
    public void CashRegister_CalculateChange_WithExactAmount_ShouldReturnZero()
    {
        // Arrange
        var cashRegister = new CashRegister();
        var totalPaid = new Money(800);
        var cost = new Money(800);

        // Act
        var change = cashRegister.CalculateChange(totalPaid, cost);

        // Assert
        Assert.Equal(0, change.Amount);
    }

    [Fact]
    public void CashRegister_GetChangeBreakdown_ShouldReturnValidBreakdown()
    {
        // Arrange
        var cashRegister = new CashRegister();
        var changeAmount = new Money(650); // 1x500 + 1x100 + 1x50

        // Act
        var breakdown = cashRegister.GetChangeBreakdown(changeAmount);

        // Assert
        Assert.NotEmpty(breakdown);
        var totalChange = breakdown.Sum(kvp => (int)kvp.Key * kvp.Value);
        Assert.Equal(650, totalChange);
    }

    [Fact]
    public void CashRegister_CanProvideChange_WithSufficientChange_ShouldReturnTrue()
    {
        // Arrange
        var cashRegister = new CashRegister();
        var changeAmount = new Money(100);

        // Act
        var canProvideChange = cashRegister.CanProvideChange(changeAmount);

        // Assert
        Assert.True(canProvideChange);
    }

    [Fact]
    public void CashRegister_IsOutOfService_WithSufficientChange_ShouldReturnFalse()
    {
        // Arrange
        var cashRegister = new CashRegister();
        var changeAmount = new Money(100);

        // Act
        var isOutOfService = cashRegister.IsOutOfService(changeAmount);

        // Assert
        Assert.False(isOutOfService);
    }

    [Fact]
    public void CashRegister_DispenseChange_ShouldReduceQuantities()
    {
        // Arrange
        var cashRegister = new CashRegister();
        var initialQuantity = cashRegister.MoneySlots[CoinType.Coin100].Quantity;
        
        var breakdown = new Dictionary<CoinType, int>
        {
            { CoinType.Coin100, 2 }
        };

        // Act
        cashRegister.DispenseChange(breakdown);

        // Assert
        var finalQuantity = cashRegister.MoneySlots[CoinType.Coin100].Quantity;
        Assert.Equal(initialQuantity - 2, finalQuantity);
    }
}
