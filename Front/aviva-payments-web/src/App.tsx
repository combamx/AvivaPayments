import { useEffect, useMemo, useState } from "react";
import { OrdersApi } from "./api";
import { PaymentMode } from "./types";              // ✅ valor (enum) en runtime
import type { CreateOrderRequest, OrderResponse } from "./types";

// Catálogo fijo para demo (puedes traerlo de un endpoint si quieres)
const CATALOG = [
  { name: "Plan básico", price: 199 },
  { name: "Plan pro", price: 499 },
  { name: "Soporte 30 días", price: 99 },
  { name: "Implementación", price: 699 },
];

type CartItem = { productName: string; quantity: number; unitPrice: number };

export default function App() {
  const [mode, setMode] = useState<PaymentMode>(PaymentMode.CreditCard);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [orders, setOrders] = useState<OrderResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [tab, setTab] = useState<"create"|"orders">("create");
  const [error, setError] = useState<string | null>(null);

  const total = useMemo(
    () => cart.reduce((acc, i) => acc + i.quantity * i.unitPrice, 0),
    [cart]
  );

  async function refreshOrders() {
    const data = await OrdersApi.list();
    setOrders(data.sort((a, b) => (a.createdAt < b.createdAt ? 1 : -1)));
  }

  useEffect(() => {
    refreshOrders().catch(console.error);
  }, []);

  function addToCart(name: string, price: number) {
    setCart(prev => {
      const found = prev.find(p => p.productName === name);
      if (found) {
        return prev.map(p => p.productName === name ? { ...p, quantity: p.quantity + 1 } : p);
      }
      return [...prev, { productName: name, quantity: 1, unitPrice: price }];
    });
  }

  function updateQty(name: string, qty: number) {
    setCart(prev =>
      prev.map(p => p.productName === name ? { ...p, quantity: Math.max(1, qty) } : p)
    );
  }

  function removeFromCart(name: string) {
    setCart(prev => prev.filter(p => p.productName !== name));
  }

  async function createOrder() {
    setError(null);
    if (cart.length === 0) {
      setError("Agrega al menos un producto.");
      return;
    }
    const payload: CreateOrderRequest = {
      paymentMode: mode,
      items: cart.map<CreateOrderItem>(c => ({
        productName: c.productName,
        quantity: c.quantity,
        unitPrice: c.unitPrice,
      })),
    };
    setLoading(true);
    try {
      const created = await OrdersApi.create(payload);
      setCart([]);
      setTab("orders");
      await refreshOrders();
      alert(`Orden creada: ${created.id}\nProveedor: ${created.providerName}`);
    } catch (e: any) {
      setError(e.message ?? "Error al crear");
    } finally {
      setLoading(false);
    }
  }

  async function doCancel(id: string) {
    setLoading(true);
    try {
      await OrdersApi.cancel(id);
      await refreshOrders();
    } catch (e: any) {
      alert(e.message ?? "Error al cancelar");
    } finally {
      setLoading(false);
    }
  }

  async function doPay(id: string) {
    setLoading(true);
    try {
      await OrdersApi.pay(id);
      await refreshOrders();
    } catch (e: any) {
      alert(e.message ?? "Error al pagar");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="container">
      <div className="row" style={{justifyContent:"space-between", alignItems:"center"}}>
        <h1>Aviva Payments • Front</h1>
        <div className="row" style={{gap:8}}>
          <button className={tab==="create" ? "primary": ""} onClick={()=>setTab("create")}>Crear orden</button>
          <button className={tab==="orders" ? "primary": ""} onClick={()=>setTab("orders")}>Órdenes</button>
        </div>
      </div>

      {tab === "create" && (
        <div className="row" style={{alignItems:"flex-start"}}>
          {/* Catálogo */}
          <div className="card" style={{flex:1}}>
            <h2>Catálogo</h2>
            <div className="grid">
              {CATALOG.map(p => (
                <div key={p.name} className="card">
                  <div style={{fontWeight:600}}>{p.name}</div>
                  <small className="muted">${p.price.toFixed(2)}</small>
                  <div style={{marginTop:8}}>
                    <button className="primary" onClick={()=>addToCart(p.name,p.price)}>Agregar</button>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Carrito */}
          <div className="card" style={{flex:1}}>
            <h2>Carrito</h2>
            {cart.length === 0 ? (
              <small className="muted">Vacío. Agrega del catálogo →</small>
            ) : (
              <table className="table">
                <thead>
                  <tr><th>Producto</th><th>Cant.</th><th>Precio</th><th>Subtotal</th><th></th></tr>
                </thead>
                <tbody>
                  {cart.map(item => (
                    <tr key={item.productName}>
                      <td>{item.productName}</td>
                      <td>
                        <input
                          type="number"
                          value={item.quantity}
                          min={1}
                          onChange={(e)=>updateQty(item.productName, Number(e.target.value))}
                          style={{width:70}}
                        />
                      </td>
                      <td>${item.unitPrice.toFixed(2)}</td>
                      <td>${(item.unitPrice * item.quantity).toFixed(2)}</td>
                      <td><button className="warn" onClick={()=>removeFromCart(item.productName)}>Quitar</button></td>
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  <tr><td colSpan={5} style={{textAlign:"right"}}><b>Total: ${total.toFixed(2)}</b></td></tr>
                </tfoot>
              </table>
            )}

            <div style={{marginTop:12}} className="row">
              <select value={mode} onChange={(e)=>setMode(Number(e.target.value))}>
                <option value={PaymentMode.Cash}>Cash</option>
                <option value={PaymentMode.Transfer}>Transfer</option>
              </select>
              <button className="success" disabled={loading || cart.length===0} onClick={createOrder}>
                {loading ? "Creando..." : "Crear orden"}
              </button>
            </div>
            {error && <div style={{color:"#fca5a5", marginTop:8}}>{error}</div>}
          </div>
        </div>
      )}

      {tab === "orders" && (
        <div className="card">
          <div className="row" style={{justifyContent:"space-between", alignItems:"center"}}>
            <h2>Órdenes</h2>
            <button onClick={()=>refreshOrders()}>Actualizar</button>
          </div>
          <table className="table" style={{marginTop:8}}>
            <thead>
              <tr>
                <th>Id</th>
                <th>Fecha</th>
                <th>Total</th>
                <th>Pago</th>
                <th>Proveedor</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {orders.map(o => (
                <tr key={o.id}>
                  <td style={{maxWidth:220, overflow:"hidden", textOverflow:"ellipsis"}} title={o.id}>{o.id}</td>
                  <td>{new Date(o.createdAt).toLocaleString()}</td>
                  <td>${o.totalAmount.toFixed(2)}</td>
                  <td><span className="badge">{PaymentMode[o.paymentMode]}</span></td>
                  <td>{o.providerName ?? "-"}</td>
                  <td>{o.status}</td>
                  <td className="row" style={{gap:8}}>
                    <button className="warn" disabled={loading} onClick={()=>doCancel(o.id)}>Cancelar</button>
                    <button className="success" disabled={loading} onClick={()=>doPay(o.id)}>Pagar</button>
                  </td>
                </tr>
              ))}
              {orders.length === 0 && (
                <tr><td colSpan={7}><small className="muted">No hay órdenes todavía.</small></td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
