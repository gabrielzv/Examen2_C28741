using VendingMachine.Domain.ValueObjects;

namespace VendingMachine.Domain.Entities;

public class Product
{
    public ProductId Id { get; private set; }
    public string Name { get; private set; }
    public Money Price { get; private set; }
    public int Quantity { get; private set; }

    public Product(ProductId id, string name, Money price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be null or empty", nameof(name));
        
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

        Id = id;
        Name = name.Trim();
        Price = price;
        Quantity = quantity;
    }

    public bool IsAvailable => Quantity > 0;

    public void DecreaseQuantity(int amount = 1)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
        
        if (Quantity < amount)
            throw new InvalidOperationException($"Not enough stock. Available: {Quantity}, Requested: {amount}");
        
        Quantity -= amount;
    }

    public void IncreaseQuantity(int amount = 1)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
        
        Quantity += amount;
    }
}
