namespace VendingMachine.Application.DTOs;

public class CartItemDto
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();
}

public class OrderSummaryDto
{
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }
    public string FormattedTotal { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class OrderItemDto
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string FormattedUnitPrice { get; set; } = string.Empty;
    public string FormattedLineTotal { get; set; } = string.Empty;
}
