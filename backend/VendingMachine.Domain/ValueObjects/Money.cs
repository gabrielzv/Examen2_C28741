namespace VendingMachine.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; init; }

    public Money(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        
        Amount = amount;
    }

    public static implicit operator decimal(Money money) => money.Amount;
    public static implicit operator Money(decimal amount) => new(amount);

    public override string ToString() => $"₡{Amount:N0}";
}
