using AvivaPayments.Domain.Entities;
using AvivaPayments.Application.Interfaces;

namespace AvivaPayments.Application.Services;

public interface IPaymentProviderSelector
{
    (IPaymentProvider provider, decimal fee) SelectBestProvider ( decimal amount , PaymentMode paymentMode );
}
