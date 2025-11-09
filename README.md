# AvivaPayments

Mini API para gestionar **órdenes de pago** y enviarlas al **proveedor más barato** (PagaFacil o CazaPagos) según el monto y la forma de pago.

---

## ¿Qué hace?

1. Recibe una orden (forma de pago + productos).
2. Calcula el total.
3. Calcula la comisión de cada proveedor.
4. Elige el proveedor con menor comisión.
5. Crea la orden en ese proveedor externo (via `x-api-key`).
6. Guarda la orden localmente con el `providerName` y el `providerOrderId`.
7. Permite listar, ver, cancelar y pagar la orden, propagándolo al proveedor.

---

## Endpoints principales

- `POST /api/orders` → crea una orden
- `GET /api/orders` → lista órdenes
- `GET /api/orders/{id}` → detalle
- `POST /api/orders/{id}/cancel` → cancela en tu sistema **y** en el proveedor
- `POST /api/orders/{id}/pay` → marca pagada en tu sistema **y** en el proveedor

---

## Proveedores soportados

- **PagaFacil**: `https://app-paga-chg-aviva.azurewebsites.net/swagger`
- **CazaPagos**: `https://app-caza-chg-aviva.azurewebsites.net/swagger`


---

## Bajar y Probar
- git clone https://github.com/combamx/AvivaPayments.git
- cd AvivaPayments
- dotnet restore
- dotnet build
  
Ambos requieren header:

```http
x-api-key: apikey-1cnmoisyhkif4s



