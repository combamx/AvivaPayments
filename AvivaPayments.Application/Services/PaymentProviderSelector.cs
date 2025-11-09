using AvivaPayments.Application.Interfaces;
using AvivaPayments.Domain.Entities;

namespace AvivaPayments.Application.Services;

public class PaymentProviderSelector : IPaymentProviderSelector
{
    private readonly IEnumerable<IPaymentProvider> _providers;

    public PaymentProviderSelector ( IEnumerable<IPaymentProvider> providers )
    {
        _providers = providers;
    }

    // Seleccionar el mejor proveedor basado en la tarifa más baja para el monto y modo de pago dados
    public (IPaymentProvider provider, decimal fee) SelectBestProvider ( decimal amount , PaymentMode paymentMode )
    {

        IPaymentProvider? bestProvider = null;
        decimal bestFee = decimal.MaxValue;

        // CazaPagosPaymentProvider
        // PagaFacilPaymentProvider
        foreach (var provider in _providers)
        {
            var fee = provider.CalculateFee ( amount , paymentMode );
            if (fee < bestFee)
            {
                bestFee = fee;
                bestProvider = provider;
            }
        }

        if (bestProvider == null)
            throw new InvalidOperationException ( "No hay proveedores de pago registrados." );

        return (bestProvider, bestFee);
    }
}
