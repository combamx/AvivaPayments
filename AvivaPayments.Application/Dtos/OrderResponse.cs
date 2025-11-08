using AvivaPayments.Domain.Entities;

namespace AvivaPayments.Application.Dtos;

public class OrderResponse
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string? ProviderOrderId { get; set; }
    public decimal ProviderFee { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new ( );
}


public class OrderItemResponse
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}
