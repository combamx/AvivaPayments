using System.Net.Http;
using System.Text;
using System.Text.Json;
using AvivaPayments.Application.Interfaces;
using AvivaPayments.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace AvivaPayments.Infrastructure.PaymentProviders;

public class CazaPagosPaymentProvider : IPaymentProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public CazaPagosPaymentProvider ( HttpClient httpClient , IConfiguration configuration )
    {
        _httpClient = httpClient;
        _baseUrl = configuration [ "PaymentProviders:CazaPagos:BaseUrl" ]
                   ?? throw new InvalidOperationException ( "CazaPagos BaseUrl not configured" );
        _apiKey = configuration [ "PaymentProviders:CazaPagos:ApiKey" ]
                  ?? throw new InvalidOperationException ( "CazaPagos ApiKey not configured" );
    }

    public string Name => "CazaPagos";

    // seguimos usando la lógica de tarifas que ya teníamos en memoria
    // (si quisieras usar /Order/Quote habría que volver async la interfaz)
    public decimal CalculateFee ( decimal amount , PaymentMode paymentMode )
    {
        if (paymentMode == PaymentMode.CreditCard)
        {
            if (amount <= 1500) return Math.Round ( amount * 0.02m , 2 );
            if (amount <= 5000) return Math.Round ( amount * 0.015m , 2 );
            return Math.Round ( amount * 0.005m , 2 );
        }

        if (paymentMode == PaymentMode.Transfer)
        {
            if (amount <= 500) return 5m;
            if (amount <= 1000) return Math.Round ( amount * 0.025m , 2 );
            return Math.Round ( amount * 0.02m , 2 );
        }

        return 12m;
    }

    public async Task<PaymentProviderOrderResult> CreateRemoteOrderAsync (
        decimal amount ,
        PaymentMode paymentMode ,
        IEnumerable<OrderItem> items ,
        CancellationToken cancellationToken = default )
    {
        // swagger: POST /Order
        var url = $"{_baseUrl}/Order";

        var products = items.Select ( i => new
        {
            name = i.ProductName ,
            unitPrice = i.UnitPrice * i.Quantity
        } ).ToList ( );

        // igual que con PagaFacil: mandamos un producto con el total
        var payload = new
        {
            method = MapPaymentMode ( paymentMode ) ,
            products = products
        };

        var json = JsonSerializer.Serialize ( payload );
        var request = new HttpRequestMessage ( HttpMethod.Post , url );
        request.Content = new StringContent ( json , Encoding.UTF8 , "application/json" );
        request.Headers.Add ( "x-api-key" , _apiKey );

        var response = await _httpClient.SendAsync ( request , cancellationToken );
        response.EnsureSuccessStatusCode ( );

        var respJson = await response.Content.ReadAsStringAsync ( cancellationToken );
        using var doc = JsonDocument.Parse ( respJson );

        // respuesta también trae "orderId"
        var orderId = doc.RootElement.GetProperty ( "orderId" ).GetString ( )
                      ?? throw new InvalidOperationException ( "CazaPagos no devolvió orderId" );

        return new PaymentProviderOrderResult
        {
            ProviderOrderId = orderId
        };
    }

    public async Task CancelRemoteOrderAsync ( string providerOrderId , CancellationToken cancellationToken = default )
    {
        // swagger: PUT /cancellation?id=...
        var url = $"{_baseUrl}/cancellation?id={Uri.EscapeDataString ( providerOrderId )}";

        var request = new HttpRequestMessage ( HttpMethod.Put , url );
        request.Headers.Add ( "x-api-key" , _apiKey );

        var response = await _httpClient.SendAsync ( request , cancellationToken );
        response.EnsureSuccessStatusCode ( );
    }

    public async Task PayRemoteOrderAsync ( string providerOrderId , CancellationToken cancellationToken = default )
    {
        // swagger: PUT /payment?id=...
        var url = $"{_baseUrl}/payment?id={Uri.EscapeDataString ( providerOrderId )}";

        var request = new HttpRequestMessage ( HttpMethod.Put , url );
        request.Headers.Add ( "x-api-key" , _apiKey );

        var response = await _httpClient.SendAsync ( request , cancellationToken );
        response.EnsureSuccessStatusCode ( );
    }

    private static string MapPaymentMode ( PaymentMode mode )
    {
        return mode switch
        {
            PaymentMode.Cash => "1",
            PaymentMode.CreditCard => "2",
            PaymentMode.Transfer => "3",
            _ => "0"
        };
    }
}
