using Moq;
using VendingMachine.Application.DTOs;
using VendingMachine.Application.UseCases;
using VendingMachine.Domain.Entities;
using VendingMachine.Domain.Interfaces;
using VendingMachine.Domain.ValueObjects;
using Xunit;

namespace VendingMachine.Tests.Application.UseCases;

public class GetProductsUseCaseTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly GetProductsUseCase _useCase;

    public GetProductsUseCaseTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _useCase = new GetProductsUseCase(_mockProductRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnProductDtos()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product(new ProductId("1"), "Coca Cola", new Money(800), 10),
            new Product(new ProductId("2"), "Sprite", new Money(750), 5)
        };

        _mockProductRepository.Setup(x => x.GetAllProductsAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        
        var productDtos = result.ToList();
        Assert.Equal("1", productDtos[0].Id);
        Assert.Equal("Coca Cola", productDtos[0].Name);
        Assert.Equal(800, productDtos[0].Price);
        Assert.Equal(10, productDtos[0].Quantity);
        
        Assert.Equal("2", productDtos[1].Id);
        Assert.Equal("Sprite", productDtos[1].Name);
        Assert.Equal(750, productDtos[1].Price);
        Assert.Equal(5, productDtos[1].Quantity);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyRepository_ShouldReturnEmptyList()
    {
        // Arrange
        _mockProductRepository.Setup(x => x.GetAllProductsAsync())
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryOnce()
    {
        // Arrange
        _mockProductRepository.Setup(x => x.GetAllProductsAsync())
            .ReturnsAsync(new List<Product>());

        // Act
        await _useCase.ExecuteAsync();

        // Assert
        _mockProductRepository.Verify(x => x.GetAllProductsAsync(), Times.Once);
    }
}
