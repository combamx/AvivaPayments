using AvivaPayments.Application.Interfaces;
using AvivaPayments.Domain.Entities;
using AvivaPayments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace AvivaPayments.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly PaymentsDbContext _context;

    public OrderRepository ( PaymentsDbContext context )
    {
        _context = context;
    }

    public async Task<Order> AddAsync ( Order order , CancellationToken cancellationToken = default )
    {
        _context.Orders.Add ( order );
        await _context.SaveChangesAsync ( cancellationToken );
        return order;
    }

    public async Task<List<Order>> GetAllAsync ( CancellationToken cancellationToken = default )
    {
        return await _context.Orders
            .Include ( o => o.Items )
            .OrderByDescending ( o => o.CreatedAt )
            .ToListAsync ( cancellationToken );
    }

    public async Task<Order?> GetByIdAsync ( Guid id , CancellationToken cancellationToken = default )
    {
        return await _context.Orders
            .Include ( o => o.Items )
            .FirstOrDefaultAsync ( o => o.Id == id , cancellationToken );
    }

    public async Task UpdateAsync ( Order order , CancellationToken cancellationToken = default )
    {
        _context.Orders.Update ( order );
        await _context.SaveChangesAsync ( cancellationToken );
    }
}
