using VendingMachine.Domain.Entities;
using VendingMachine.Domain.Interfaces;
using VendingMachine.Domain.ValueObjects;

namespace VendingMachine.Infrastructure.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products;

    public InMemoryProductRepository()
    {
        _products = InitializeProducts();
    }

    public Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return Task.FromResult(_products.AsEnumerable());
    }

    public Task<Product?> GetProductByIdAsync(ProductId id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }

    public Task UpdateProductAsync(Product product)
    {
        var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id);
        if (existingProduct != null)
        {
            var index = _products.IndexOf(existingProduct);
            _products[index] = product;
        }
        return Task.CompletedTask;
    }

    private static List<Product> InitializeProducts()
    {
        return new List<Product>
        {
            new Product(new ProductId("COCACOLA"), "Coca Cola", new Money(800), 10),
            new Product(new ProductId("PEPSI"), "Pepsi", new Money(750), 8),
            new Product(new ProductId("FANTA"), "Fanta", new Money(950), 10),
            new Product(new ProductId("SPRITE"), "Sprite", new Money(975), 15)
        };
    }
}
