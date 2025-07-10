using Moq;
using VendingMachine.Application.DTOs;
using VendingMachine.Application.UseCases;
using VendingMachine.Domain.Entities;
using VendingMachine.Domain.Interfaces;
using VendingMachine.Domain.ValueObjects;
using Xunit;

namespace VendingMachine.Tests.Application.UseCases;

public class ProcessPaymentUseCaseTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly CashRegister _cashRegister;
    private readonly ProcessPaymentUseCase _useCase;

    public ProcessPaymentUseCaseTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _cashRegister = new CashRegister();
        _useCase = new ProcessPaymentUseCase(_cashRegister, _mockProductRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithSufficientPayment_ShouldReturnSuccess()
    {
        // Arrange
        var product = new Product(new ProductId("1"), "Coca Cola", new Money(800), 10);
        _mockProductRepository.Setup(x => x.GetProductByIdAsync(new ProductId("1")))
            .ReturnsAsync(product);

        var payment = new PaymentDto
        {
            InsertedMoney = new Dictionary<string, int>
            {
                { "Coin500", 2 } //1000 colones
            },
            TotalInserted = 1000
        };

        var cart = new CartDto
        {
            Items = new List<CartItemDto>
            {
                new CartItemDto { ProductId = "1", Quantity = 1 } //800 colones
            }
        };

        // Act
        var result = await _useCase.ExecuteAsync(payment, 800, cart);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(1000, result.TotalPaid);
        Assert.Equal(800, result.TotalCost);
        Assert.Equal(200, result.ChangeAmount);
        Assert.Contains("Su vuelto es de 200 colones", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithInsufficientPayment_ShouldReturnFailure()
    {
        // Arrange
        var payment = new PaymentDto
        {
            InsertedMoney = new Dictionary<string, int>
            {
                { "Coin500", 1 } //500 colones
            },
            TotalInserted = 500
        };

        // Act
        var result = await _useCase.ExecuteAsync(payment, 800, null);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal("Pago insuficiente", result.Message);
        Assert.Equal(500, result.TotalPaid);
        Assert.Equal(800, result.TotalCost);
        Assert.Contains("Faltan ₡300", result.Errors.First());
    }

    [Fact]
    public async Task ExecuteAsync_WithExactPayment_ShouldReturnSuccessWithNoChange()
    {
        // Arrange
        var product = new Product(new ProductId("1"), "Coca Cola", new Money(800), 10);
        _mockProductRepository.Setup(x => x.GetProductByIdAsync(new ProductId("1")))
            .ReturnsAsync(product);

        var payment = new PaymentDto
        {
            InsertedMoney = new Dictionary<string, int>
            {
                { "Coin500", 1 },
                { "Coin100", 3 } //800 colones
            },
            TotalInserted = 800
        };

        var cart = new CartDto
        {
            Items = new List<CartItemDto>
            {
                new CartItemDto { ProductId = "1", Quantity = 1 }
            }
        };

        // Act
        var result = await _useCase.ExecuteAsync(payment, 800, cart);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ChangeAmount);
        Assert.Contains("No hay vuelto", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithCart_ShouldReduceProductStock()
    {
        // Arrange
        var product = new Product(new ProductId("1"), "Coca Cola", new Money(800), 10);
        _mockProductRepository.Setup(x => x.GetProductByIdAsync(new ProductId("1")))
            .ReturnsAsync(product);

        var payment = new PaymentDto
        {
            InsertedMoney = new Dictionary<string, int>
            {
                { "Bill1000", 1 } //1000 colones
            },
            TotalInserted = 1000
        };

        var cart = new CartDto
        {
            Items = new List<CartItemDto>
            {
                new CartItemDto { ProductId = "1", Quantity = 3 }
            }
        };

        // Act
        var result = await _useCase.ExecuteAsync(payment, 800, cart);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(7, product.Quantity);
        _mockProductRepository.Verify(x => x.UpdateProductAsync(product), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithChangeBreakdown_ShouldReturnCorrectBreakdown()
    {
        // Arrange
        var product = new Product(new ProductId("1"), "Coca Cola", new Money(800), 10);
        _mockProductRepository.Setup(x => x.GetProductByIdAsync(new ProductId("1")))
            .ReturnsAsync(product);

        var payment = new PaymentDto
        {
            InsertedMoney = new Dictionary<string, int>
            {
                { "Bill1000", 1 }// 1000 colones
            },
            TotalInserted = 1000
        };

        var cart = new CartDto
        {
            Items = new List<CartItemDto>
            {
                new CartItemDto { ProductId = "1", Quantity = 1 }// 800 colones
            }
        };

        // Act
        var result = await _useCase.ExecuteAsync(payment, 800, cart);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(200, result.ChangeAmount);
        Assert.NotEmpty(result.ChangeBreakdown);
        Assert.Contains("Desglose:", result.Message);
    }
}
