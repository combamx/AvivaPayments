using System.Net.Http;
using System.Text;
using System.Text.Json;
using AvivaPayments.Application.Interfaces;
using AvivaPayments.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace AvivaPayments.Infrastructure.PaymentProviders;

public class PagaFacilPaymentProvider : IPaymentProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public PagaFacilPaymentProvider ( HttpClient httpClient , IConfiguration configuration )
    {
        _httpClient = httpClient;
        _baseUrl = configuration [ "PaymentProviders:PagaFacil:BaseUrl" ]
                   ?? throw new InvalidOperationException ( "PagaFacil BaseUrl not configured" );
        _apiKey = configuration [ "PaymentProviders:PagaFacil:ApiKey" ]
                  ?? throw new InvalidOperationException ( "PagaFacil ApiKey not configured" );
    }

    public string Name => "PagaFacil";

    public decimal CalculateFee ( decimal amount , PaymentMode paymentMode )
    {
        return paymentMode switch
        {
            PaymentMode.Cash => 15m,
            PaymentMode.CreditCard => Math.Round ( amount * 0.01m , 2 ),
            _ => 10m
        };
    }

    public async Task<PaymentProviderOrderResult> CreateRemoteOrderAsync (
    decimal amount ,
    PaymentMode paymentMode ,
    IEnumerable<OrderItem> items ,
    CancellationToken cancellationToken = default )
    {
        var url = $"{_baseUrl}/Order";

        var products = items.Select ( i => new
        {
            name = i.ProductName ,
            unitPrice = i.UnitPrice * i.Quantity
        } ).ToList ( );


        var payload = new
        {
            method = MapPaymentMode( paymentMode ) ,
            products = products
        };

        var json = JsonSerializer.Serialize ( payload );
        var request = new HttpRequestMessage ( HttpMethod.Post , url );
        request.Content = new StringContent ( json , Encoding.UTF8 , "application/json" );
        request.Headers.Add ( "x-api-key" , _apiKey );

        var response = await _httpClient.SendAsync ( request , cancellationToken );

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync ( cancellationToken );
            throw new HttpRequestException ( $"PagaFacil returned {(int) response.StatusCode} - {errorText}" );
        }

        var respJson = await response.Content.ReadAsStringAsync ( cancellationToken );
        using var doc = JsonDocument.Parse ( respJson );
        var orderId = doc.RootElement.GetProperty ( "orderId" ).GetString ( )
                      ?? throw new InvalidOperationException ( "PagaFacil no devolvió orderId" );

        return new PaymentProviderOrderResult
        {
            ProviderOrderId = orderId
        };
    }


    public async Task CancelRemoteOrderAsync ( string providerOrderId , CancellationToken cancellationToken = default )
    {
        var url = $"{_baseUrl}/cancel?id={Uri.EscapeDataString ( providerOrderId )}";

        var request = new HttpRequestMessage ( HttpMethod.Put , url );
        request.Headers.Add ( "x-api-key" , _apiKey );

        var response = await _httpClient.SendAsync ( request , cancellationToken );
        response.EnsureSuccessStatusCode ( );
    }

    public async Task PayRemoteOrderAsync ( string providerOrderId , CancellationToken cancellationToken = default )
    {
        var url = $"{_baseUrl}/pay?id={Uri.EscapeDataString ( providerOrderId )}";

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
