using AvivaPayments.Application.Interfaces;
using AvivaPayments.Application.Services;
using AvivaPayments.Infrastructure.Interfaces;
using AvivaPayments.Infrastructure.Persistence;
using AvivaPayments.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder ( args );

builder.Services.AddControllers ( );
builder.Services.AddEndpointsApiExplorer ( );
builder.Services.AddSwaggerGen ( );

builder.Services.AddDbContext<PaymentsDbContext> ( options =>
{
    var cs = builder.Configuration.GetConnectionString ( "DefaultConnection" )
             ?? "Data Source=payments.db";
    options.UseSqlite ( cs );
} );

builder.Services.AddScoped<IOrderService , OrderService> ( );
builder.Services.AddScoped<IOrderRepository , OrderRepository> ( );

var app = builder.Build ( );

// crea la DB y las tablas si no existen
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

app.UseHttpsRedirection ( );
app.UseAuthorization ( );
app.MapControllers ( );
app.Run ( );
