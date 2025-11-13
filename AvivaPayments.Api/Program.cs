using AvivaPayments.Application.Interfaces;
using AvivaPayments.Application.Services;
using AvivaPayments.Infrastructure.PaymentProviders;
using AvivaPayments.Infrastructure.Persistence;
using AvivaPayments.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder ( args );

// Config
builder.Configuration
    .AddJsonFile ( "appsettings.json" , optional: false , reloadOnChange: true )
    .AddJsonFile ( $"appsettings.{builder.Environment.EnvironmentName}.json" , optional: true , reloadOnChange: true )
    .AddEnvironmentVariables ( );

builder.Services.AddControllers ( );
builder.Services.AddEndpointsApiExplorer ( );
builder.Services.AddSwaggerGen ( );

// DB
builder.Services.AddDbContext<PaymentsDbContext> ( options =>
{
    var cs = builder.Configuration.GetConnectionString ( "DefaultConnection" )
             ?? "Data Source=payments.db";
    options.UseSqlite ( cs );
} );

// Application
builder.Services.AddScoped<IOrderService , OrderService> ( );
builder.Services.AddScoped<IPaymentProviderSelector , PaymentProviderSelector> ( );

// Repos
builder.Services.AddScoped<IOrderRepository , OrderRepository> ( );

// Providers
builder.Services.AddScoped<IPaymentProvider , PagaFacilPaymentProvider> ( );
builder.Services.AddScoped<IPaymentProvider , CazaPagosPaymentProvider> ( );
builder.Services.AddHttpClient<PagaFacilPaymentProvider> ( );
builder.Services.AddHttpClient<CazaPagosPaymentProvider> ( );

// CORS
var cors = "_aviva";
builder.Services.AddCors ( o =>
    o.AddPolicy ( cors , p => p.WithOrigins ( "http://localhost:5173" ).AllowAnyHeader ( ).AllowAnyMethod ( ) )
);

var app = builder.Build ( );

// DB init
using (var scope = app.Services.CreateScope ( ))
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext> ( );
    db.Database.EnsureCreated ( );
}

if (app.Environment.IsDevelopment ( ))
{
    app.UseSwagger ( );
    app.UseSwaggerUI ( );
}

app.UseCors ( cors );
app.UseHttpsRedirection ( );
app.UseAuthorization ( );
app.MapControllers ( );
app.Run ( );
