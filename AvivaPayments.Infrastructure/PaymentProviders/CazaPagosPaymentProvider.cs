using AvivaPayments.Application.Interfaces;
using AvivaPayments.Domain.Entities;

namespace AvivaPayments.Infrastructure.PaymentProviders;

public class CazaPagosPaymentProvider : IPaymentProvider
{
    public string Name => "CazaPagos";

    public decimal CalculateFee ( decimal amount , PaymentMode paymentMode )
    {
        // PDF: tarjeta de crédito tiene tramos
        if (paymentMode == PaymentMode.CreditCard)
        {
            if (amount <= 1500) return Math.Round ( amount * 0.02m , 2 );
            if (amount <= 5000) return Math.Round ( amount * 0.015m , 2 );
            return Math.Round ( amount * 0.005m , 2 );
        }

        // Transferencia: 0–500 → 5 MXN, 500–1000 → 2.5%, >1000 → 2.0%
        if (paymentMode == PaymentMode.Transfer)
        {
            if (amount <= 500) return 5m;
            if (amount <= 1000) return Math.Round ( amount * 0.025m , 2 );
            return Math.Round ( amount * 0.02m , 2 );
        }

        // efectivo u otros
        return 12m;
    }

    public Task<PaymentProviderOrderResult> CreateRemoteOrderAsync (
        decimal amount ,
        PaymentMode paymentMode ,
        CancellationToken cancellationToken = default )
    {
        // aquí iría el HttpClient real con x-api-key al swagger de CazaPagos
        var result = new PaymentProviderOrderResult
        {
            ProviderOrderId = $"CZ-{Guid.NewGuid ( ):N}"
        };
        return Task.FromResult ( result );
    }

    public Task CancelRemoteOrderAsync ( string providerOrderId , CancellationToken cancellationToken = default )
    {
        return Task.CompletedTask;
    }

    public Task PayRemoteOrderAsync ( string providerOrderId , CancellationToken cancellationToken = default )
    {
        return Task.CompletedTask;
    }
}
