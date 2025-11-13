# ===== build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar csproj para cachear restore
COPY AvivaPayments.Domain/*.csproj AvivaPayments.Domain/
COPY AvivaPayments.Application/*.csproj AvivaPayments.Application/
COPY AvivaPayments.Infrastructure/*.csproj AvivaPayments.Infrastructure/
COPY AvivaPayments.Api/*.csproj AvivaPayments.Api/
RUN dotnet restore AvivaPayments.Api/AvivaPayments.Api.csproj

# Copiar el resto y publicar
COPY . .
RUN dotnet publish AvivaPayments.Api/AvivaPayments.Api.csproj -c Release -o /out /p:UseAppHost=false

# ===== runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# HTTP plano
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# DB (SQLite) en volumen/carpeta aparte
RUN mkdir -p /app/data
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/payments.db"

# (Opcional) variables para proveedores (no hornear keys)
# ENV PaymentProviders__PagaFacil__BaseUrl="https://app-paga-chg-aviva.azurewebsites.net"
# ENV PaymentProviders__PagaFacil__ApiKey=""
# ENV PaymentProviders__CazaPagos__BaseUrl="https://app-caza-chg-aviva.azurewebsites.net"
# ENV PaymentProviders__CazaPagos__ApiKey=""

COPY --from=build /out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "AvivaPayments.Api.dll"]
