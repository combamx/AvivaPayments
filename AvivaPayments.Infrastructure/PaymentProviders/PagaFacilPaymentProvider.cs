using AvivaPayments.Application.Interfaces;
using AvivaPayments.Domain.Entities;

namespace AvivaPayments.Infrastructure.PaymentProviders;

public class PagaFacilPaymentProvider : IPaymentProvider
{
    public string Name => "PagaFacil";

    public decimal CalculateFee ( decimal amount , PaymentMode paymentMode )
    {
        // PDF: efectivo = 15 MXN fijos, tarjeta = 1%
        return paymentMode switch
        {
            PaymentMode.Cash => 15m,
            PaymentMode.CreditCard => Math.Round ( amount * 0.01m , 2 ),
            _ => 10m // un default chico
        };
    }

    public Task<PaymentProviderOrderResult> CreateRemoteOrderAsync (
        decimal amount ,
        PaymentMode paymentMode ,
        CancellationToken cancellationToken = default )
    {
        // aquí iría el HttpClient al swagger de PagaFacil
        var result = new PaymentProviderOrderResult
        {
            ProviderOrderId = $"PF-{Guid.NewGuid ( ):N}"
        };
        return Task.FromResult ( result );
    }

    // en real aquí harías el HttpClient al swagger
    public Task CancelRemoteOrderAsync ( string providerOrderId , CancellationToken cancellationToken = default )
    {
        // simulado
        return Task.CompletedTask;
    }

    public Task PayRemoteOrderAsync ( string providerOrderId , CancellationToken cancellationToken = default )
    {
        // simulado
        return Task.CompletedTask;
    }
}
