namespace TicketSales;

public sealed class TicketOrder
{
    public string EventName { get; init; } = string.Empty;
    public DateTime ShowDate { get; init; }
    public string TicketType { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Amount { get; init; }
}

public sealed class SaleSummary
{
    public long SaleId { get; init; }
    public DateTime SoldAt { get; init; }
    public int TicketCount { get; init; }
    public decimal TotalAmount { get; init; }
}
