import { ProductView } from "./product.model";

export interface OrderDetailView {
  product?: ProductView | null;
  colorId?: number | null;
  colorName?: string | null;
  colorImageUrl?: string | null;
  price: number;
  quantity: number;
}

export interface OrderView {
  id: number;
  createDate: string | Date;
  orderDetails: OrderDetailView[];
}

export interface PickupOrderRequest {
  cartDetailIds: number[];
  storeHouseId: number;
}

export interface DeliveryOrderRequest {
  cartDetailIds: number[];
  provinceName: string;
  districtName: string;
  street: string;
}

export type OrderStatus = 'Pending' | 'Paid' | 'Shipping' | 'Completed' | 'Cancelled';

export interface OrderHistoryItem {
  productId: number;
  productName: string | null;
  imageUrl: string | null;
  colorId: number | null;
  colorName: string | null;
  quantity: number;
  price: number;
}

/** Một đơn trong lịch sử mua hàng (GET /orders/history/me). */
export interface OrderHistory {
  id: number;
  createDate: string;
  status: OrderStatus | string;
  /** pickup = nhận tại cửa hàng, delivery = giao hàng tận nơi. */
  type: 'pickup' | 'delivery' | string;
  shippingFee: number;
  /** Tiền hàng cộng phí vận chuyển. */
  totalAmount: number;
  items: OrderHistoryItem[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  totalPages: number;
}
