using AvivaPayments.Application.Dtos;

namespace AvivaPayments.Application.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync ( CreateOrderRequest request , CancellationToken cancellationToken = default );
    Task<List<OrderResponse>> GetOrdersAsync ( CancellationToken cancellationToken = default );
    Task<OrderResponse?> GetOrderByIdAsync ( Guid id , CancellationToken cancellationToken = default );

    Task<bool> CancelOrderAsync ( Guid id , CancellationToken cancellationToken = default );
    Task<bool> PayOrderAsync ( Guid id , CancellationToken cancellationToken = default );
}
