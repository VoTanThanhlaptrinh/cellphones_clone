import { computed, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { StoreView } from '../core/models/store.model';
import { ApiResponse } from '../core/models/api-response.model';
import { OrderView, PickupOrderRequest, DeliveryOrderRequest } from '../core/models/order.model';
import { NotifyService } from './notify.service';
import { PaymentFormData } from '../core/models/payment.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private baseUrl = environment.apiUrl;
  private appear = signal<boolean>(true);
  orderView = signal<OrderView | null>(null)
  private paymentData: PaymentFormData | null = null;
  constructor(private http: HttpClient, private notifyServce: NotifyService) {
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
  createPickupOrder(payload: PickupOrderRequest): Observable<ApiResponse<OrderView>> {
    return this.http.post<ApiResponse<OrderView>>(`${this.baseUrl}/orders/pickup`, payload);
  }

  createDeliveryOrder(payload: DeliveryOrderRequest): Observable<ApiResponse<OrderView>> {
    return this.http.post<ApiResponse<OrderView>>(`${this.baseUrl}/orders/delivery`, payload);
  }
  get IsAppear(): boolean {
    return this.appear();
  }
  setAppear(b: boolean) {
    this.appear.set(b);
  }
}
