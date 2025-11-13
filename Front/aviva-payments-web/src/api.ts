import type { CreateOrderRequest, OrderResponse } from "./types";


const BASE = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") || "";

async function http<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    headers: { "Content-Type": "application/json", ...(init?.headers || {}) },
    ...init,
  });
  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(`${res.status} ${res.statusText} - ${text}`);
  }
  return res.json() as Promise<T>;
}

export const OrdersApi = {
  create: (payload: CreateOrderRequest) =>
    http<OrderResponse>("/api/orders", {
      method: "POST",
      body: JSON.stringify(payload)
    }),

  list: () => http<OrderResponse[]>("/api/orders"),

  get: (id: string) => http<OrderResponse>(`/api/orders/${id}`),

  cancel: (id: string) =>
    fetch(`${BASE}/api/orders/${id}/cancel`, { method: "POST" }).then(r => {
      if (!r.ok) return r.text().then(t => Promise.reject(new Error(t || r.statusText)));
    }),

  pay: (id: string) =>
    fetch(`${BASE}/api/orders/${id}/pay`, { method: "POST" }).then(r => {
      if (!r.ok) return r.text().then(t => Promise.reject(new Error(t || r.statusText)));
    }),
};
