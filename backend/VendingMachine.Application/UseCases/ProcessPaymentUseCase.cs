using VendingMachine.Application.DTOs;
using VendingMachine.Domain.Entities;
using VendingMachine.Domain.ValueObjects;

namespace VendingMachine.Application.UseCases;

public interface IProcessPaymentUseCase
{
    Task<PaymentResultDto> ExecuteAsync(PaymentDto payment, decimal totalCost);
}

public class ProcessPaymentUseCase : IProcessPaymentUseCase
{
    private readonly CashRegister _cashRegister;

    public ProcessPaymentUseCase(CashRegister cashRegister)
    {
        _cashRegister = cashRegister ?? throw new ArgumentNullException(nameof(cashRegister));
    }

    public Task<PaymentResultDto> ExecuteAsync(PaymentDto payment, decimal totalCost)
    {
        try
        {
            var totalPaid = new Money(payment.TotalInserted);
            var cost = new Money(totalCost);

            if (totalPaid.Amount < cost.Amount)
            {
                return Task.FromResult(new PaymentResultDto
                {
                    IsSuccessful = false,
                    Message = "Pago insuficiente",
                    TotalPaid = totalPaid.Amount,
                    TotalCost = cost.Amount,
                    Errors = { $"Faltan ₡{cost.Amount - totalPaid.Amount}" }
                });
            }

            var changeAmount = _cashRegister.CalculateChange(totalPaid, cost);

            if (changeAmount.Amount > 0 && _cashRegister.IsOutOfService(changeAmount))
            {
                return Task.FromResult(new PaymentResultDto
                {
                    IsSuccessful = false,
                    Message = "Fuera de servicio",
                    IsOutOfService = true,
                    TotalPaid = totalPaid.Amount,
                    TotalCost = cost.Amount,
                    ChangeAmount = changeAmount.Amount,
                    Errors = { "La maquina no tiene suficientes monedas para dar el cambio exacto" }
                });
            }

            // Meter el dinero que inserto el usuario
            foreach (var money in payment.InsertedMoney)
            {
                if (Enum.TryParse<CoinType>(money.Key, out var coinType))
                {
                    _cashRegister.AddMoney(coinType, money.Value);
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

            return Task.FromResult(new PaymentResultDto
            {
                IsSuccessful = true,
                Message = changeAmount.Amount > 0 ? "Pago procesado, retire su cambio." : "Pago procesado exitosamente",
                TotalPaid = totalPaid.Amount,
                TotalCost = cost.Amount,
                ChangeAmount = changeAmount.Amount,
                ChangeBreakdown = changeBreakdown
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new PaymentResultDto
            {
                IsSuccessful = false,
                Message = "Error al procesar el pago",
                TotalPaid = payment.TotalInserted,
                TotalCost = totalCost,
                Errors = { ex.Message }
            });
        }
    }
}
