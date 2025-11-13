using System;
using System.Collections.Generic;
using AvivaPayments.Application.Interfaces;
using AvivaPayments.Application.Services;
using AvivaPayments.Domain.Entities;
using Moq;
using Xunit;

namespace AvivaPayments.Application.Tests.Services
{
    public class PaymentProviderSelectorTests
    {
        [Fact]
        public void SelectBestProvider_ReturnsProviderWithLowestFee ( )
        {
            // Arrange
            var amount = 100m;
            var mode = PaymentMode.CreditCard;

            var providerCheap = new Mock<IPaymentProvider> ( );
            providerCheap.SetupGet ( p => p.Name ).Returns ( "Barato" );
            providerCheap
                .Setup ( p => p.CalculateFee ( amount , mode ) )
                .Returns ( 1m );

            var providerExpensive = new Mock<IPaymentProvider> ( );
            providerExpensive.SetupGet ( p => p.Name ).Returns ( "Caro" );
            providerExpensive
                .Setup ( p => p.CalculateFee ( amount , mode ) )
                .Returns ( 5m );

            var sut = new PaymentProviderSelector ( new []
            {
                providerCheap.Object,
                providerExpensive.Object
            } );

            // Act
            var (provider, fee) = sut.SelectBestProvider ( amount , mode );

            // Assert
            Assert.Same ( providerCheap.Object , provider );
            Assert.Equal ( 1m , fee );
        }

        [Fact]
        public void SelectBestProvider_Throws_WhenNoProvidersRegistered ( )
        {
            // Arrange
            var sut = new PaymentProviderSelector ( new List<IPaymentProvider> ( ) );

            // Act + Assert
            var ex = Assert.Throws<InvalidOperationException> (
                ( ) => sut.SelectBestProvider ( 100m , PaymentMode.Cash ) );

            Assert.Contains ( "No hay proveedores de pago registrados" , ex.Message );
        }
    }
}
