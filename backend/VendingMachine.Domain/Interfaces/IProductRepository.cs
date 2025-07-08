using VendingMachine.Domain.Entities;
using VendingMachine.Domain.ValueObjects;

namespace VendingMachine.Domain.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(ProductId id);
    Task UpdateProductAsync(Product product);
}
