// Giao diện cho thông tin nhận tại quán
export interface StoreInfo {
  city: string;
  district: string;
  storeId: string;
  streetName?: string;
}

// Giao diện cho thông tin giao hàng
export interface ShipInfo {
  receiverName: string;
  receiverPhone: string;
  shipCity: string;
  shipDistrict: string;
  shipWard: string;
  address: string;
}

// Giao diện tổng cho toàn bộ Form
export interface PaymentFormData {
  deliveryMethod: 'at-store' | 'delivery' | string;
  email: string;
  note: string;
  storeInfo: StoreInfo;
  shipInfo: ShipInfo;
}