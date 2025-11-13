// src/types.ts
export enum PaymentMode { 
  Cash = 0, 
  Transfer = 1, 
  CreditCard = 2 
}

export enum OrderStatus  {
  Created = 0,
  Pending = 1, 
  Paid = 2, 
  Cancelled = 3
}

export type CreateOrderItem = {
  productName: string;
  quantity: number;
  unitPrice: number;
};

export type CreateOrderRequest = {
  paymentMode: PaymentMode;
  items: CreateOrderItem[];
};

export type OrderItemResponse = CreateOrderItem;

export type OrderResponse = {
  id: string;
  createdAt: string;
  totalAmount: number;
  paymentMode: PaymentMode;
  providerName: string | null;
  providerOrderId: string | null;
  providerFee: number;
  status: OrderStatus;
  items: OrderItemResponse[];
};
