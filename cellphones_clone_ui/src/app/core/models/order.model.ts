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
