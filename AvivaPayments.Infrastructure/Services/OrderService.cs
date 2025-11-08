using AvivaPayments.Application.Dtos;
using AvivaPayments.Application.Interfaces;
using AvivaPayments.Domain.Entities;
using AvivaPayments.Infrastructure.Interfaces;

namespace AvivaPayments.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService ( IOrderRepository orderRepository )
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderResponse> CreateOrderAsync ( CreateOrderRequest request , CancellationToken cancellationToken = default )
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new ArgumentException ( "La orden debe tener al menos un item" );

        // Mapear DTO → Entidad
        var order = new Order
        {
            PaymentMode = request.PaymentMode ,
            // ProviderName lo rellenaremos cuando integremos el selector
            ProviderName = string.Empty ,
        };

        decimal total = 0m;
        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                throw new ArgumentException ( "Cantidad inválida en un item" );

            if (item.UnitPrice < 0)
                throw new ArgumentException ( "Precio inválido en un item" );

            var orderItem = new OrderItem
            {
                ProductName = item.ProductName ,
                Quantity = item.Quantity ,
                UnitPrice = item.UnitPrice
            };

            order.Items.Add ( orderItem );
            total += orderItem.Subtotal;
        }

        order.TotalAmount = total;

        // Guardar
        order = await _orderRepository.AddAsync ( order , cancellationToken );

        // Mapear a respuesta
        return MapToResponse ( order );
    }

    public async Task<List<OrderResponse>> GetOrdersAsync ( CancellationToken cancellationToken = default )
    {
        var orders = await _orderRepository.GetAllAsync ( cancellationToken );
        return orders.Select ( MapToResponse ).ToList ( );
    }

    public async Task<OrderResponse?> GetOrderByIdAsync ( Guid id , CancellationToken cancellationToken = default )
    {
        var order = await _orderRepository.GetByIdAsync ( id , cancellationToken );
        if (order is null) return null;
        return MapToResponse ( order );
    }

    private static OrderResponse MapToResponse ( Order order )
    {
        return new OrderResponse
        {
            Id = order.Id ,
            CreatedAt = order.CreatedAt ,
            TotalAmount = order.TotalAmount ,
            PaymentMode = order.PaymentMode ,
            ProviderName = order.ProviderName ,
            ProviderOrderId = order.ProviderOrderId ,
            Status = order.Status ,
            Items = order.Items.Select ( x => new OrderItemResponse
            {
                ProductName = x.ProductName ,
                Quantity = x.Quantity ,
                UnitPrice = x.UnitPrice
            } ).ToList ( )
        };
    }
}
