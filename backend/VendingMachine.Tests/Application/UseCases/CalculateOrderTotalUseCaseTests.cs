using Moq;
using VendingMachine.Application.DTOs;
using VendingMachine.Application.UseCases;
using VendingMachine.Domain.Entities;
using VendingMachine.Domain.Interfaces;
using VendingMachine.Domain.ValueObjects;
using Xunit;

namespace VendingMachine.Tests.Application.UseCases;

public class CalculateOrderTotalUseCaseTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly CalculateOrderTotalUseCase _useCase;

    public CalculateOrderTotalUseCaseTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _useCase = new CalculateOrderTotalUseCase(_mockProductRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCart_ShouldReturnCorrectTotal()
    {
        // Arrange
        var product1 = new Product(new ProductId("1"), "Coca Cola", new Money(800), 10);
        var product2 = new Product(new ProductId("2"), "Sprite", new Money(750), 5);

        _mockProductRepository.Setup(x => x.GetProductByIdAsync(new ProductId("1")))
            .ReturnsAsync(product1);
        _mockProductRepository.Setup(x => x.GetProductByIdAsync(new ProductId("2")))
            .ReturnsAsync(product2);

        var cart = new CartDto
        {
            Items = new List<CartItemDto>
            {
                new CartItemDto { ProductId = "1", Quantity = 2 },// 2*800 = 1600
                new CartItemDto { ProductId = "2", Quantity = 1 } // 1*750 = 750
            }
        };

        // Act
        var result = await _useCase.ExecuteAsync(cart);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(2350, result.Total); // 1600+750
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_WithInsufficientStock_ShouldReturnErrors()
    {
        // Arrange
        var product = new Product(new ProductId("1"), "Coca Cola", new Money(800), 3);

        _mockProductRepository.Setup(x => x.GetProductByIdAsync(new ProductId("1")))
            .ReturnsAsync(product);

        var cart = new CartDto
        {
            Items = new List<CartItemDto>
            {
                new CartItemDto { ProductId = "1", Quantity = 5 } // mas de lo que hay en stock
            }
        };

        // Act
        var result = await _useCase.ExecuteAsync(cart);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(0, result.Total);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("No hay stock para", result.Errors.First());
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentProduct_ShouldReturnErrors()
    {
        // Arrange
        _mockProductRepository.Setup(x => x.GetProductByIdAsync(new ProductId("999")))
            .ReturnsAsync((Product?)null);

        var cart = new CartDto
        {
            Items = new List<CartItemDto>
            {
                new CartItemDto { ProductId = "999", Quantity = 1 }
            }
        };

        // Act
        var result = await _useCase.ExecuteAsync(cart);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(0, result.Total);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("Producto no encontrado: 999", result.Errors.First());
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyCart_ShouldReturnZeroTotal()
    {
        // Arrange
        var cart = new CartDto
        {
            Items = new List<CartItemDto>()
        };

        // Act
        var result = await _useCase.ExecuteAsync(cart);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(0, result.Total);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroQuantity_ShouldReturnError()
    {
        // Arrange
        var product = new Product(new ProductId("1"), "Coca Cola", new Money(800), 10);

        _mockProductRepository.Setup(x => x.GetProductByIdAsync(new ProductId("1")))
            .ReturnsAsync(product);

        var cart = new CartDto
        {
            Items = new List<CartItemDto>
            {
                new CartItemDto { ProductId = "1", Quantity = 0 }
            }
        };

        // Act
        var result = await _useCase.ExecuteAsync(cart);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(0, result.Total);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("debe ser mayor a 0", result.Errors.First());
    }
}
