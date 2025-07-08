using VendingMachine.Domain.ValueObjects;

namespace VendingMachine.Domain.Entities;

public class CashRegister
{
    private readonly Dictionary<CoinType, MoneySlot> _moneySlots;

    public CashRegister()
    {
        _moneySlots = new Dictionary<CoinType, MoneySlot>
        {
            { CoinType.Coin25, new MoneySlot(CoinType.Coin25, 25) },
            { CoinType.Coin50, new MoneySlot(CoinType.Coin50, 50) },
            { CoinType.Coin100, new MoneySlot(CoinType.Coin100, 30) },
            { CoinType.Coin500, new MoneySlot(CoinType.Coin500, 20) },
            { CoinType.Bill1000, new MoneySlot(CoinType.Bill1000, 0) }
        };
    }

    public IReadOnlyDictionary<CoinType, MoneySlot> MoneySlots => _moneySlots;

    public void AddMoney(CoinType coinType, int quantity)
    {
        if (_moneySlots.ContainsKey(coinType))
        {
            _moneySlots[coinType].AddCoins(quantity);
        }
        else
        {
            throw new ArgumentException($"Unsupported coin type: {coinType}");
        }
    }

    public Money CalculateChange(Money totalPaid, Money totalCost)
    {
        var changeAmount = totalPaid.Amount - totalCost.Amount;
        if (changeAmount < 0)
            throw new InvalidOperationException("Insufficient payment");

        return new Money(changeAmount);
    }

    public Dictionary<CoinType, int> GetChangeBreakdown(Money changeAmount)
    {
        var result = new Dictionary<CoinType, int>();
        var remainingChange = changeAmount.Amount;

        // Ordenar las cantidades de mayor a menor
        var sortedCoinTypes = new[] { CoinType.Coin500, CoinType.Coin100, CoinType.Coin50, CoinType.Coin25 };

        foreach (var coinType in sortedCoinTypes)
        {
            var coinValue = (int)coinType;
            var neededCoins = (int)(remainingChange / coinValue);
            var availableCoins = _moneySlots[coinType].Quantity;
            
            var coinsToGive = Math.Min(neededCoins, availableCoins);
            
            if (coinsToGive > 0)
            {
                result[coinType] = coinsToGive;
                remainingChange -= coinsToGive * coinValue;
            }
        }

        if (remainingChange > 0)
        {
            throw new InvalidOperationException("Out of service");
        }

        return result;
    }

    public bool CanProvideChange(Money changeAmount)
    {
        try
        {
            GetChangeBreakdown(changeAmount);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public void DispenseChange(Dictionary<CoinType, int> changeBreakdown)
    {
        foreach (var kvp in changeBreakdown)
        {
            if (!_moneySlots[kvp.Key].TryRemoveCoins(kvp.Value))
            {
                throw new InvalidOperationException($"Insufficient {kvp.Key} coins for change");
            }
        }
    }

    public bool IsOutOfService(Money changeAmount)
    {
        return changeAmount.Amount > 0 && !CanProvideChange(changeAmount);
    }
}
