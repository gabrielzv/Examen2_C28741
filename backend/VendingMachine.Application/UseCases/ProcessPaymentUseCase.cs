using VendingMachine.Application.DTOs;
using VendingMachine.Domain.Entities;
using VendingMachine.Domain.Interfaces;
using VendingMachine.Domain.ValueObjects;

namespace VendingMachine.Application.UseCases;

public interface IProcessPaymentUseCase
{
    Task<PaymentResultDto> ExecuteAsync(PaymentDto payment, decimal totalCost, CartDto? cart = null);
}

public class ProcessPaymentUseCase : IProcessPaymentUseCase
{
    private readonly CashRegister _cashRegister;
    private readonly IProductRepository _productRepository;

    public ProcessPaymentUseCase(CashRegister cashRegister, IProductRepository productRepository)
    {
        _cashRegister = cashRegister ?? throw new ArgumentNullException(nameof(cashRegister));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<PaymentResultDto> ExecuteAsync(PaymentDto payment, decimal totalCost, CartDto? cart = null)
    {
        try
        {
            var totalPaid = new Money(payment.TotalInserted);
            var cost = new Money(totalCost);

            if (totalPaid.Amount < cost.Amount)
            {
                return new PaymentResultDto
                {
                    IsSuccessful = false,
                    Message = "Pago insuficiente",
                    TotalPaid = totalPaid.Amount,
                    TotalCost = cost.Amount,
                    Errors = { $"Faltan ₡{cost.Amount - totalPaid.Amount}" }
                };
            }

            var changeAmount = _cashRegister.CalculateChange(totalPaid, cost);

            if (changeAmount.Amount > 0 && _cashRegister.IsOutOfService(changeAmount))
            {
                return new PaymentResultDto
                {
                    IsSuccessful = false,
                    Message = "Fallo al realizar la compra",
                    IsOutOfService = true,
                    TotalPaid = totalPaid.Amount,
                    TotalCost = cost.Amount,
                    ChangeAmount = changeAmount.Amount,
                    Errors = { "La maquina no tiene suficientes monedas para dar el cambio exacto" }
                };
            }

            // Meter el dinero que inserto el usuario
            foreach (var money in payment.InsertedMoney)
            {
                if (Enum.TryParse<CoinType>(money.Key, out var coinType))
                {
                    _cashRegister.AddMoney(coinType, money.Value);
                }
            }

            // Reducir stock
            if (cart != null)
            {
                foreach (var item in cart.Items)
                {
                    var product = await _productRepository.GetProductByIdAsync(new ProductId(item.ProductId));
                    if (product != null)
                    {
                        product.DecreaseQuantity(item.Quantity);
                        await _productRepository.UpdateProductAsync(product);
                    }
                }
            }

            var changeBreakdown = new Dictionary<string, int>();
            if (changeAmount.Amount > 0)
            {
                var breakdown = _cashRegister.GetChangeBreakdown(changeAmount);
                _cashRegister.DispenseChange(breakdown);
                
                changeBreakdown = breakdown.ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value
                );
            }

            var successMessage = FormatSuccessMessage(changeAmount.Amount, changeBreakdown);

            return new PaymentResultDto
            {
                IsSuccessful = true,
                Message = successMessage,
                TotalPaid = totalPaid.Amount,
                TotalCost = cost.Amount,
                ChangeAmount = changeAmount.Amount,
                ChangeBreakdown = changeBreakdown
            };
        }
        catch (Exception ex)
        {
            return new PaymentResultDto
            {
                IsSuccessful = false,
                Message = "Error al procesar el pago",
                TotalPaid = payment.TotalInserted,
                TotalCost = totalCost,
                Errors = { ex.Message }
            };
        }
    }

    private string FormatSuccessMessage(decimal changeAmount, Dictionary<string, int> changeBreakdown)
    {
        if (changeAmount == 0)
        {
            return "Compra realizada exitosamente. No hay vuelto.";
        }

        var message = $"Su vuelto es de {(int)changeAmount} colones.\nDesglose:\n";
        
        var coinNames = new Dictionary<string, string>
        {
            { "Coin500", "moneda de 500" },
            { "Coin100", "moneda de 100" },
            { "Coin50", "moneda de 50" },
            { "Coin25", "moneda de 25" }
        };

        foreach (var kvp in changeBreakdown.Where(x => x.Value > 0).OrderByDescending(x => GetCoinValue(x.Key)))
        {
            var coinName = coinNames.ContainsKey(kvp.Key) ? coinNames[kvp.Key] : kvp.Key;
            message += $"{kvp.Value} {coinName}\n";
        }

        return message.TrimEnd('\n');
    }

    private int GetCoinValue(string coinType)
    {
        return coinType switch
        {
            "Coin500" => 500,
            "Coin100" => 100,
            "Coin50" => 50,
            "Coin25" => 25,
            _ => 0
        };
    }
}
