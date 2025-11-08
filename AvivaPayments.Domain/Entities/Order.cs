namespace AvivaPayments.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid ( );
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public PaymentMode PaymentMode { get; set; }

    // Proveedor elegido (PagaFacil, CazaPagos, etc.)
    public string ProviderName { get; set; } = string.Empty;

    // Id que devolvió el proveedor
    public string? ProviderOrderId { get; set; }

    // Comisión que nos cobra el proveedor para esta orden
    public decimal ProviderFee { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Created;

    public List<OrderItem> Items { get; set; } = new ( );
}
