using AvivaPayments.Domain.Entities;

namespace AvivaPayments.Application.Interfaces;

public interface IPaymentProvider
{
    string Name { get; }

    // Para que el selector pueda preguntar "¿cuánto me cobras por esto?"
    decimal CalculateFee ( decimal amount , PaymentMode paymentMode );

    // Crear la orden en el proveedor
    Task<PaymentProviderOrderResult> CreateRemoteOrderAsync (
        decimal amount ,
        PaymentMode paymentMode ,
        CancellationToken cancellationToken = default );

    Task CancelRemoteOrderAsync ( string providerOrderId , CancellationToken cancellationToken = default );
    Task PayRemoteOrderAsync ( string providerOrderId , CancellationToken cancellationToken = default );
}
