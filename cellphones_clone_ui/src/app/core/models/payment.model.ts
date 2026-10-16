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

/** Cổng thanh toán online mà payment gateway hỗ trợ. */
export type OnlineProvider = 'PAYOS' | 'VNPAY';

/** NONE = chưa tạo link; còn lại là trạng thái do payment gateway báo. */
export type OnlinePaymentStatus = 'NONE' | 'PENDING' | 'PAID' | 'FAILED' | 'EXPIRED' | 'CANCELLED' | 'REVIEW_REQUIRED';

/** Thông tin để tự hiển thị thanh toán (payOS): chuỗi VietQR, tài khoản nhận, nội dung chuyển khoản. */
export interface OnlinePaymentInfo {
  qrCode: string | null;
  bankBin: string | null;
  accountNumber: string | null;
  accountName: string | null;
  transferContent: string | null;
}

/**
 * Một giao dịch thanh toán online của đơn. `id` là mã giao dịch, nằm trên URL /payment/:id để tải lại không mất giao dịch.
 * `checkoutUrl`, `expiresAt`, `paymentInfo` chỉ có khi khách còn có thể thanh toán (PENDING).
 */
export interface OnlinePayment {
  id: number;
  orderId: number;
  amount: number;
  status: OnlinePaymentStatus;
  provider: OnlineProvider | null;
  checkoutUrl: string | null;
  expiresAt: string | null;
  paymentInfo: OnlinePaymentInfo | null;
}

// Giao diện tổng cho toàn bộ Form
export interface PaymentFormData {
  deliveryMethod: 'at-store' | 'delivery' | string;
  email: string;
  note: string;
  storeInfo: StoreInfo;
  shipInfo: ShipInfo;
}