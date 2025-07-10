using VendingMachine.Application.DTOs;
using VendingMachine.Domain.Interfaces;

namespace VendingMachine.Application.UseCases;

public interface IGetProductsUseCase
{
    Task<IEnumerable<ProductDto>> ExecuteAsync();
}

public class GetProductsUseCase : IGetProductsUseCase
{
    private readonly IProductRepository _productRepository;

    public GetProductsUseCase(IProductRepository productRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<IEnumerable<ProductDto>> ExecuteAsync()
    {
        var products = await _productRepository.GetAllProductsAsync();
        
        return products.Select(product => new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Quantity = product.Quantity,
            IsAvailable = product.IsAvailable,
            FormattedPrice = product.Price.ToString()
        });
    }
}
