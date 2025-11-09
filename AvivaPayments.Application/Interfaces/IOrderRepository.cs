using AvivaPayments.Domain.Entities;

namespace AvivaPayments.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order> AddAsync ( Order order , CancellationToken cancellationToken = default );
    Task<Order?> GetByIdAsync ( Guid id , CancellationToken cancellationToken = default );
    Task<List<Order>> GetAllAsync ( CancellationToken cancellationToken = default );
    Task UpdateAsync ( Order order , CancellationToken cancellationToken = default );
}
