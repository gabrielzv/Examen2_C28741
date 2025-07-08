namespace VendingMachine.Application.DTOs;

public class MoneySlotDto
{
    public string CoinType { get; set; } = string.Empty;
    public int Value { get; set; }
    public int Quantity { get; set; }
    public decimal TotalValue { get; set; }
}

public class PaymentDto
{
    public Dictionary<string, int> InsertedMoney { get; set; } = new();
    public decimal TotalInserted { get; set; }
}

public class PaymentResultDto
{
    public bool IsSuccessful { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal TotalPaid { get; set; }
    public decimal TotalCost { get; set; }
    public decimal ChangeAmount { get; set; }
    public Dictionary<string, int> ChangeBreakdown { get; set; } = new();
    public bool IsOutOfService { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ProcessPaymentRequestDto
{
    public PaymentDto Payment { get; set; } = new();
    public decimal TotalCost { get; set; }
    public CartDto? Cart { get; set; }
}
