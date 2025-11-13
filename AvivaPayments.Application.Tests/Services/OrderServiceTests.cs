using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvivaPayments.Application.Dtos;
using AvivaPayments.Application.Interfaces;
using AvivaPayments.Application.Services;
using AvivaPayments.Domain.Entities;
using Moq;
using Xunit;

namespace AvivaPayments.Application.Tests.Services
{
    public class OrderServiceTests
    {
        private static OrderService CreateSut (
            Mock<IOrderRepository>? repoMock = null ,
            Mock<IPaymentProviderSelector>? selectorMock = null ,
            IEnumerable<IPaymentProvider>? providers = null )
        {
            repoMock ??= new Mock<IOrderRepository> ( );
            selectorMock ??= new Mock<IPaymentProviderSelector> ( );
            providers ??= Enumerable.Empty<IPaymentProvider> ( );

            return new OrderService (
                repoMock.Object ,
                selectorMock.Object ,
                providers );
        }

        [Fact]
        public async Task CreateOrderAsync_Throws_WhenItemsNullOrEmpty ( )
        {
            // Arrange
            var sut = CreateSut ( );

            var requestWithNull = new CreateOrderRequest
            {
                PaymentMode = PaymentMode.Cash ,
                Items = null!
            };

            var requestWithEmpty = new CreateOrderRequest
            {
                PaymentMode = PaymentMode.Cash ,
                Items = new List<CreateOrderItemRequest> ( )
            };

            // Act + Assert
            var ex1 = await Assert.ThrowsAsync<ArgumentException> (
                ( ) => sut.CreateOrderAsync ( requestWithNull ) );
            Assert.Contains ( "La orden debe tener al menos un item" , ex1.Message );

            var ex2 = await Assert.ThrowsAsync<ArgumentException> (
                ( ) => sut.CreateOrderAsync ( requestWithEmpty ) );
            Assert.Contains ( "La orden debe tener al menos un item" , ex2.Message );
        }

        [Fact]
        public async Task CreateOrderAsync_Throws_WhenItemQuantityIsInvalid ( )
        {
            // Arrange
            var sut = CreateSut ( );

            var request = new CreateOrderRequest
            {
                PaymentMode = PaymentMode.Cash ,
                Items = new List<CreateOrderItemRequest>
                {
                    new()
                    {
                        ProductName = "Plan básico",
                        Quantity = 0,      // inválido
                        UnitPrice = 100m
                    }
                }
            };

            // Act + Assert
            var ex = await Assert.ThrowsAsync<ArgumentException> (
                ( ) => sut.CreateOrderAsync ( request ) );

            Assert.Contains ( "Cantidad inválida en un item" , ex.Message );
        }

        [Fact]
        public async Task CreateOrderAsync_ValidRequest_CreatesOrderWithCorrectTotalsAndProviderData ( )
        {
            // Arrange
            var repoMock = new Mock<IOrderRepository> ( );
            var selectorMock = new Mock<IPaymentProviderSelector> ( );
            var providerMock = new Mock<IPaymentProvider> ( );

            var request = new CreateOrderRequest
            {
                PaymentMode = PaymentMode.CreditCard ,
                Items = new List<CreateOrderItemRequest>
                {
                    new()
                    {
                        ProductName = "Plan básico",
                        Quantity = 2,
                        UnitPrice = 100m
                    },
                    new()
                    {
                        ProductName = "Soporte 30 días",
                        Quantity = 1,
                        UnitPrice = 50m
                    }
                }
            };

            var expectedTotal = 2 * 100m + 1 * 50m; // 250

            selectorMock
                .Setup ( s => s.SelectBestProvider ( expectedTotal , request.PaymentMode ) )
                .Returns ( (providerMock.Object, 10m) );

            providerMock.SetupGet ( p => p.Name ).Returns ( "PagaFacil" );

            providerMock
                .Setup ( p => p.CreateRemoteOrderAsync (
                    expectedTotal ,
                    request.PaymentMode ,
                    It.IsAny<IEnumerable<OrderItem>> ( ) ,
                    It.IsAny<CancellationToken> ( ) ) )
                .ReturnsAsync ( new PaymentProviderOrderResult
                {
                    ProviderOrderId = "PF-123"
                } );

            repoMock
                .Setup ( r => r.AddAsync ( It.IsAny<Order> ( ) , It.IsAny<CancellationToken> ( ) ) )
                .ReturnsAsync ( ( Order o , CancellationToken _ ) => o );

            var sut = new OrderService (
                repoMock.Object ,
                selectorMock.Object ,
                new [] { providerMock.Object } );

            // Act
            var response = await sut.CreateOrderAsync ( request );

            // Assert
            Assert.Equal ( expectedTotal , response.TotalAmount );
            Assert.Equal ( request.PaymentMode , response.PaymentMode );
            Assert.Equal ( "PagaFacil" , response.ProviderName );
            Assert.Equal ( "PF-123" , response.ProviderOrderId );
            Assert.Equal ( 10m , response.ProviderFee );
            Assert.Equal ( 2 , response.Items.Count );

            selectorMock.Verify (
                s => s.SelectBestProvider ( expectedTotal , request.PaymentMode ) ,
                Times.Once );

            repoMock.Verify (
                r => r.AddAsync ( It.IsAny<Order> ( ) , It.IsAny<CancellationToken> ( ) ) ,
                Times.Once );

            providerMock.Verify (
                p => p.CreateRemoteOrderAsync (
                    expectedTotal ,
                    request.PaymentMode ,
                    It.IsAny<IEnumerable<OrderItem>> ( ) ,
                    It.IsAny<CancellationToken> ( ) ) ,
                Times.Once );
        }

        [Fact]
        public async Task CancelOrderAsync_ReturnsFalse_WhenOrderDoesNotExist ( )
        {
            // Arrange
            var repoMock = new Mock<IOrderRepository> ( );
            repoMock
                .Setup ( r => r.GetByIdAsync ( It.IsAny<Guid> ( ) , It.IsAny<CancellationToken> ( ) ) )
                .ReturnsAsync ( (Order?) null );

            var sut = CreateSut ( repoMock );

            // Act
            var result = await sut.CancelOrderAsync ( Guid.NewGuid ( ) );

            // Assert
            Assert.False ( result );
        }

        [Fact]
        public async Task CancelOrderAsync_AlreadyCancelled_ReturnsTrueWithoutCallingProvider ( )
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid ( ) ,
                Status = OrderStatus.Cancelled ,
                ProviderName = "PagaFacil" ,
                ProviderOrderId = "PF-123" ,
                PaymentMode = PaymentMode.CreditCard ,
                TotalAmount = 100m
            };

            var repoMock = new Mock<IOrderRepository> ( );
            repoMock
                .Setup ( r => r.GetByIdAsync ( order.Id , It.IsAny<CancellationToken> ( ) ) )
                .ReturnsAsync ( order );

            var providerMock = new Mock<IPaymentProvider> ( );
            providerMock.SetupGet ( p => p.Name ).Returns ( "PagaFacil" );

            var sut = CreateSut (
                repoMock: repoMock ,
                providers: new [] { providerMock.Object } );

            // Act
            var result = await sut.CancelOrderAsync ( order.Id );

            // Assert
            Assert.True ( result );
            providerMock.Verify (
                p => p.CancelRemoteOrderAsync ( It.IsAny<string> ( ) , It.IsAny<CancellationToken> ( ) ) ,
                Times.Never );
        }

        [Fact]
        public async Task CancelOrderAsync_ValidFlow_CallsProviderAndUpdatesOrder ( )
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid ( ) ,
                Status = OrderStatus.Paid ,
                ProviderName = "PagaFacil" ,
                ProviderOrderId = "PF-123" ,
                PaymentMode = PaymentMode.CreditCard ,
                TotalAmount = 100m
            };

            var repoMock = new Mock<IOrderRepository> ( );
            repoMock
                .Setup ( r => r.GetByIdAsync ( order.Id , It.IsAny<CancellationToken> ( ) ) )
                .ReturnsAsync ( order );

            var providerMock = new Mock<IPaymentProvider> ( );
            providerMock.SetupGet ( p => p.Name ).Returns ( "PagaFacil" );

            var sut = CreateSut (
                repoMock: repoMock ,
                providers: new [] { providerMock.Object } );

            // Act
            var result = await sut.CancelOrderAsync ( order.Id );

            // Assert
            Assert.True ( result );
            Assert.Equal ( OrderStatus.Cancelled , order.Status );

            providerMock.Verify (
                p => p.CancelRemoteOrderAsync ( order.ProviderOrderId! , It.IsAny<CancellationToken> ( ) ) ,
                Times.Once );

            repoMock.Verify (
                r => r.UpdateAsync ( order , It.IsAny<CancellationToken> ( ) ) ,
                Times.Once );
        }
    }
}
