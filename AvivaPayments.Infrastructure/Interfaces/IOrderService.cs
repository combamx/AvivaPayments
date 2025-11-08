using AvivaPayments.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvivaPayments.Infrastructure.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrderAsync ( CreateOrderRequest request , CancellationToken cancellationToken = default );
        Task<List<OrderResponse>> GetOrdersAsync ( CancellationToken cancellationToken = default );
        Task<OrderResponse?> GetOrderByIdAsync ( Guid id , CancellationToken cancellationToken = default );
        // estos los llenamos luego con cancel/pay
    }
}
