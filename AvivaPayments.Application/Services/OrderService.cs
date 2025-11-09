using AvivaPayments.Application.Dtos;
using AvivaPayments.Application.Interfaces;
using AvivaPayments.Domain.Entities;

namespace AvivaPayments.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentProviderSelector _paymentProviderSelector;
    private readonly IEnumerable<IPaymentProvider> _paymentProviders;

    public OrderService (
        IOrderRepository orderRepository ,
        IPaymentProviderSelector paymentProviderSelector ,
        IEnumerable<IPaymentProvider> paymentProviders )
    {
        _orderRepository = orderRepository;
        _paymentProviderSelector = paymentProviderSelector;
        _paymentProviders = paymentProviders;
    }

    public async Task<OrderResponse> CreateOrderAsync ( CreateOrderRequest request , CancellationToken cancellationToken = default )
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new ArgumentException ( "La orden debe tener al menos un item" );

        var order = new Order
        {
            PaymentMode = request.PaymentMode ,
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

        // elegir proveedor
        var (provider, fee) = _paymentProviderSelector.SelectBestProvider ( order.TotalAmount , order.PaymentMode );

        var remoteResult = await provider.CreateRemoteOrderAsync ( order.TotalAmount , order.PaymentMode , cancellationToken );

        order.ProviderName = provider.Name;
        order.ProviderOrderId = remoteResult.ProviderOrderId;
        order.ProviderFee = fee;

        order = await _orderRepository.AddAsync ( order , cancellationToken );

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

    public async Task<bool> CancelOrderAsync ( Guid id , CancellationToken cancellationToken = default )
    {
        var order = await _orderRepository.GetByIdAsync ( id , cancellationToken );
        if (order is null) return false;

        if (order.Status == OrderStatus.Cancelled)
            return true;

        var provider = _paymentProviders.FirstOrDefault ( p => p.Name == order.ProviderName );
        if (provider is null)
            throw new InvalidOperationException ( $"No se encontró el proveedor {order.ProviderName}" );

        if (!string.IsNullOrWhiteSpace ( order.ProviderOrderId ))
        {
            await provider.CancelRemoteOrderAsync ( order.ProviderOrderId , cancellationToken );
        }

        order.Status = OrderStatus.Cancelled;
        await _orderRepository.UpdateAsync ( order , cancellationToken );
        return true;
    }

    public async Task<bool> PayOrderAsync ( Guid id , CancellationToken cancellationToken = default )
    {
        var order = await _orderRepository.GetByIdAsync ( id , cancellationToken );
        if (order is null) return false;

        if (order.Status == OrderStatus.Paid)
            return true;

        var provider = _paymentProviders.FirstOrDefault ( p => p.Name == order.ProviderName );
        if (provider is null)
            throw new InvalidOperationException ( $"No se encontró el proveedor {order.ProviderName}" );

        if (!string.IsNullOrWhiteSpace ( order.ProviderOrderId ))
        {
            await provider.PayRemoteOrderAsync ( order.ProviderOrderId , cancellationToken );
        }

        order.Status = OrderStatus.Paid;
        await _orderRepository.UpdateAsync ( order , cancellationToken );
        return true;
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
            ProviderFee = order.ProviderFee ,
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
