using AvivaPayments.Domain.Entities;

namespace AvivaPayments.Application.Dtos;

public class CreateOrderRequest
{
    public PaymentMode PaymentMode { get; set; }

    // Lista de productos que trae la UI
    public List<CreateOrderItemRequest> Items { get; set; } = new ( );
}

public class CreateOrderItemRequest
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
