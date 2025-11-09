El documento describe un challenge para construir un mini sistema de órdenes de pago, con énfasis en que tu API sepa hablar con múltiples proveedores de pago y elegir el más barato según monto y forma de pago. Te lo desarmo por partes. 
1. Objetivo del reto
•	Hay una pantalla de productos: el usuario selecciona varios y “crea una orden”.
•	Al crearla, tu Web API de Pagos debe:
1.	Recibir el payload (forma de pago + productos).
2.	Calcular el monto.
3.	Elegir el proveedor óptimo (el de menor comisión para ese caso).
4.	Llamar al API de ese proveedor externo para que “cree” la orden allá.
5.	Guardar en tu sistema la orden, incluyendo con qué proveedor se creó y las comisiones.
•	Luego se debe poder: listar órdenes, ver una orden, cancelarla, marcarla como pagada. Y esas acciones deben propagarse al proveedor. 
2. Operaciones que piden
En tu API:
•	Crear orden
•	Obtener listado de órdenes
•	Obtener detalle de una orden
•	Cancelar orden
•	Pagar (marcar pagada) una orden
Todo esto debe reflejarse también en el proveedor que se usó. 
En los proveedores (PagaFacil, CazaPagos):
•	Crear orden
•	Listar órdenes
•	Obtener una
•	Cancelar
•	Pagar
Te dan las URLs de swagger y te dicen que llevan x-api-key en el header. O sea: hay integración real (o simulada) y hay que poner la clave. 
3. La parte clave: cómo elegir el proveedor
El PDF trae una tablita de reglas de comisión por proveedor y por modalidad de pago. La idea es: dado el monto total y el PaymentMode (Cash, Tarjeta, Transferencia…), calculas cuánto cobraría cada proveedor y eliges el de menor costo. Eso tiene que estar encapsulado en tu dominio para que si mañana hay otro proveedor, solo agregues otra implementación. 
Ejemplos que menciona:
•	PagaFacil:
o	Pago en efectivo: 15 MXN fijos por transacción.
o	Pago con tarjeta de crédito: 1% del monto.
•	CazaPagos:
o	Tarjeta de crédito: 0–1500 → 2%, 1500–5000 → 1.5%, >5000 → 0.5%
o	Transferencia: 0–500 → 5 MXN, 500–1000 → 2.5%, >1000 → 2.0%
La lógica es clara: implementas todas las fórmulas, ejecutas todas contra el mismo monto+modo y te quedas con la menor. Eso es un strategy de libro. 
4. Qué tecnología esperan
•	Si es FullStack: React para la app (lista de productos, crear orden, y grid de órdenes donde puedas ver, cancelar, pagar) + ASP.NET Core Web API en C#.
•	Si es solo backend: solo el Web API. 
5. Persistencia
Te dicen explícito: no es obligatorio usar BD. Puedes usar:
•	In-memory (listas, diccionarios)
•	EF InMemory
La idea es que se enfoquen en la arquitectura, no en el setup de SQL. 
6. Qué van a evaluar
•	Calidad de código y patrones
•	Separación por capas
•	Cobertura de requisitos
•	Pruebas
•	UI decente
Y que subas todo a un repo Git con commits claros. 
________________________________________
7. Arquitectura sugerida (lo que se desprende del PDF)
Capas:
1.	API layer (controllers)
o	POST /orders
o	GET /orders
o	GET /orders/{id}
o	POST /orders/{id}/cancel
o	POST /orders/{id}/pay
2.	Application/Domain services
o	OrderService que:
	Calcula monto
	Pide al PaymentProviderSelector el proveedor óptimo
	Llama a un PaymentProviderClient (interfaz) para crear la orden remota
	Guarda la orden en el repositorio in-memory con el ProviderName y el RemoteOrderId
3.	Infraestructura – proveedores
o	Interfaz: IPaymentProvider
	Task<CreateOrderResult> CreateOrderAsync(...)
	Task CancelOrderAsync(...)
	Task PayOrderAsync(...)
o	Implementaciones:
	PagaFacilProvider
	CazaPagosProvider
o	Cada una sabe:
	URL base (del swagger que dan)
	API key
	Reglas de comisión (podrías separarlas en otra clase)
4.	Selector de proveedor
o	Recibe: amount, paymentMode
o	Recorre los providers registrados y les pide “cuál sería tu comisión para esto” (método GetFee(...))
o	Devuelve el de menor fee
5.	Repositorio in-memory
o	Diccionario <Guid, Order> o Id incremental
o	Guarda:
	Id local
	Id remoto
	Provider usado
	Monto
	Productos
	Estado (Creada, Pagada, Cancelada)
	Fees detallados (según el ejemplo del PDF) 
________________________________________
8. Cosas importantes que se ven entre líneas
•	Propagación: si cancelas en tu API, también debes cancelar en el proveedor que se usó originalmente. Por eso es clave guardar ProviderName y RemoteOrderId. 
•	Extensibilidad: te advierten que los proveedores pueden cambiar y que debería ser “mínimo o nulo” el cambio en el core → usa interfaces y DI. 
•	Payloads de ejemplo: los del PDF son orientativos; puedes cambiarlos. O sea, puedes modelar mejor tu DTO. 
