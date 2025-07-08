using VendingMachine.Domain.ValueObjects;

namespace VendingMachine.Domain.Entities;

public class MoneySlot
{
    public CoinType CoinType { get; private set; }
    public int Quantity { get; private set; }
    public Money Value => new Money((int)CoinType);

    public MoneySlot(CoinType coinType, int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));
            
        CoinType = coinType;
        Quantity = quantity;
    }

    public void AddCoins(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));
        
        Quantity += quantity;
    }

    public bool TryRemoveCoins(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));
        
        if (Quantity >= quantity)
        {
            Quantity -= quantity;
            return true;
        }
        return false;
    }

    public Money GetTotalValue()
    {
        return new Money((int)CoinType * Quantity);
    }
}
