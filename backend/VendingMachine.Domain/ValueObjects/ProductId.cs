namespace VendingMachine.Domain.ValueObjects;

public record ProductId
{
    public string Value { get; init; }

    public ProductId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(value));
        
        Value = value.Trim().ToUpperInvariant();
    }

    public static implicit operator string(ProductId productId) => productId.Value;
    public static implicit operator ProductId(string value) => new(value);

    public override string ToString() => Value;
}
