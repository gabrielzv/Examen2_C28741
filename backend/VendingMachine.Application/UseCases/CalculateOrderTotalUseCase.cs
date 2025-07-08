using VendingMachine.Application.DTOs;
using VendingMachine.Domain.Interfaces;
using VendingMachine.Domain.ValueObjects;

namespace VendingMachine.Application.UseCases;

public interface ICalculateOrderTotalUseCase
{
    Task<OrderSummaryDto> ExecuteAsync(CartDto cart);
}

public class CalculateOrderTotalUseCase : ICalculateOrderTotalUseCase
{
    private readonly IProductRepository _productRepository;

    public CalculateOrderTotalUseCase(IProductRepository productRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<OrderSummaryDto> ExecuteAsync(CartDto cart)
    {
        var orderSummary = new OrderSummaryDto();
        var orderItems = new List<OrderItemDto>();
        decimal total = 0;

        foreach (var cartItem in cart.Items)
        {
            if (cartItem.Quantity <= 0)
            {
                orderSummary.Errors.Add($"La cantidad para {cartItem.ProductId} debe ser mayor a 0");
                continue;
            }

            var product = await _productRepository.GetProductByIdAsync(new ProductId(cartItem.ProductId));
            
            if (product == null)
            {
                orderSummary.Errors.Add($"Producto no encontrado: {cartItem.ProductId}");
                continue;
            }

            if (!product.IsAvailable)
            {
                orderSummary.Errors.Add($"Producto agotado: {product.Name}");
                continue;
            }

            if (cartItem.Quantity > product.Quantity)
            {
                orderSummary.Errors.Add($"No hay stock para {product.Name}. Disponible: {product.Quantity}, Solicitado: {cartItem.Quantity}");
                continue;
            }

            var lineTotal = product.Price.Amount * cartItem.Quantity;
            total += lineTotal;

            orderItems.Add(new OrderItemDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = cartItem.Quantity,
                UnitPrice = product.Price.Amount,
                LineTotal = lineTotal,
                FormattedUnitPrice = product.Price.ToString(),
                FormattedLineTotal = new Money(lineTotal).ToString()
            });
        }

        orderSummary.Items = orderItems;
        orderSummary.SubTotal = total;
        orderSummary.Total = total;
        orderSummary.FormattedTotal = new Money(total).ToString();
        orderSummary.IsValid = !orderSummary.Errors.Any();

        return orderSummary;
    }
}
