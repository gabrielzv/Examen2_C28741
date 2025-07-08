using VendingMachine.Application.DTOs;
using VendingMachine.Domain.Entities;

namespace VendingMachine.Application.UseCases;

public interface IGetCashRegisterStatusUseCase
{
    Task<List<MoneySlotDto>> ExecuteAsync();
}

public class GetCashRegisterStatusUseCase : IGetCashRegisterStatusUseCase
{
    private readonly CashRegister _cashRegister;

    public GetCashRegisterStatusUseCase(CashRegister cashRegister)
    {
        _cashRegister = cashRegister ?? throw new ArgumentNullException(nameof(cashRegister));
    }

    public Task<List<MoneySlotDto>> ExecuteAsync()
    {
        var result = _cashRegister.MoneySlots.Select(kvp => new MoneySlotDto
        {
            CoinType = kvp.Key.ToString(),
            Value = (int)kvp.Key,
            Quantity = kvp.Value.Quantity,
            TotalValue = kvp.Value.GetTotalValue().Amount
        }).OrderByDescending(x => x.Value).ToList();

        return Task.FromResult(result);
    }
}
