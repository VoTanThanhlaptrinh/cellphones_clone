import { computed, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { StoreView } from '../core/models/store.model';
import { ApiResponse } from '../core/models/api-response.model';
import { OrderView } from '../core/models/order.model';
import { NotifyService } from './notify.service';
import { PaymentFormData } from '../core/models/payment.model';

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private baseUrl = environment.apiUrl;
  private appear = signal<boolean>(true);
  orderView = signal<OrderView | null>(null)
  storeViews = signal<StoreView[]>([]);
  private paymentData: PaymentFormData | null = null;
  constructor(private http: HttpClient, private notifyServce: NotifyService) {
  }
  initStoreView() {
    if (this.storeViews().length === 0) {
      this.http.get<ApiResponse<StoreView[]>>(`${this.baseUrl}/stores/views`).subscribe({
        next: (res) => {
          this.storeViews.set(res.data)
        },
        error: (err) => console.error('Lỗi lấy cửa hàng:', err)
      });
    }
  }
    // Lưu data vào Service
  setPaymentData(data: PaymentFormData) {
    this.paymentData = data;
  }

  // Lấy data ra để fill vào Form
  getPaymentData(): PaymentFormData | null {
    return this.paymentData;
  }

  // Xóa data (Dùng khi thanh toán thành công hoặc hủy đơn)
  clearPaymentData() {
    this.paymentData = null;
  }
  createOrder(cartDetailIds: number[]) {
    return this.http.post<ApiResponse<OrderView>>(`${this.baseUrl}/orders`, cartDetailIds)
  }
  get IsAppear(): boolean {
    return this.appear();
  }
  setAppear(b: boolean) {
    this.appear.set(b);
  }
}
