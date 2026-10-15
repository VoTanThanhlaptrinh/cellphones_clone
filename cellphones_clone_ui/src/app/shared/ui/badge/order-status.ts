import { BadgeTone } from './badge.component';

const LABELS: Record<string, string> = {
  Pending: 'Đang xử lý',
  Paid: 'Đã thanh toán',
  Shipping: 'Đang giao hàng',
  Completed: 'Hoàn thành',
  Cancelled: 'Đã hủy',
};

const TONES: Record<string, BadgeTone> = {
  Pending: 'neutral',
  Paid: 'success',
  Shipping: 'info',
  Completed: 'success',
  Cancelled: 'danger',
};

/** Nhãn tiếng Việt của trạng thái đơn hàng (dùng chung cho storefront và admin). */
export function orderStatusLabel(status: string): string {
  return LABELS[status] ?? status;
}

export function orderStatusTone(status: string): BadgeTone {
  return TONES[status] ?? 'neutral';
}

/** Các trạng thái đơn theo thứ tự xử lý, để dựng bộ lọc. */
export const ORDER_STATUSES = Object.keys(LABELS);
