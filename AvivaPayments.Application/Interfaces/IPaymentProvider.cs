using AvivaPayments.Application.Interfaces;
using AvivaPayments.Domain.Entities;

public interface IPaymentProvider
{
    string Name { get; }

    decimal CalculateFee ( decimal amount , PaymentMode paymentMode );

    Task<PaymentProviderOrderResult> CreateRemoteOrderAsync (
        decimal amount ,
        PaymentMode paymentMode ,
        IEnumerable<OrderItem> items ,
        CancellationToken cancellationToken = default );

    Task CancelRemoteOrderAsync ( string providerOrderId , CancellationToken cancellationToken = default );
    Task PayRemoteOrderAsync ( string providerOrderId , CancellationToken cancellationToken = default );
}
