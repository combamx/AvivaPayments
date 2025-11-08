using AvivaPayments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AvivaPayments.Infrastructure.Persistence;

public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext ( DbContextOptions<PaymentsDbContext> options ) : base ( options )
    {
    }

    public DbSet<Order> Orders => Set<Order> ( );
    public DbSet<OrderItem> OrderItems => Set<OrderItem> ( );

    protected override void OnModelCreating ( ModelBuilder modelBuilder )
    {
        base.OnModelCreating ( modelBuilder );

        modelBuilder.Entity<Order> ( entity =>
        {
            entity.HasKey ( x => x.Id );
            entity.Property ( x => x.TotalAmount ).HasColumnType ( "decimal(18,2)" );
            entity.Property ( x => x.ProviderName ).HasMaxLength ( 100 );

            entity.HasMany ( x => x.Items )
                  .WithOne ( )
                  .HasForeignKey ( x => x.OrderId );
        } );

        modelBuilder.Entity<OrderItem> ( entity =>
        {
            entity.HasKey ( x => x.Id );
            entity.Property ( x => x.ProductName ).HasMaxLength ( 200 );
            entity.Property ( x => x.UnitPrice ).HasColumnType ( "decimal(18,2)" );
        } );
    }
}
