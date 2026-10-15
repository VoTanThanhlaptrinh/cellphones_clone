import { inject, Injectable, signal } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { environment } from '../../environments/environment';
import { HttpClient, HttpContext } from '@angular/common/http';
import { SKIP_GLOBAL_SPINNER } from '../core/interceptor/loading.context';
import { ApiResponse } from '../core/models/api-response.model';
import { OrderView, PickupOrderRequest, DeliveryOrderRequest, OrderHistory, PagedResult } from '../core/models/order.model';
import { CartView } from '../core/models/cart_request.model';
import { defer, map, Observable, of, switchMap, tap } from 'rxjs';
import { AuthService } from './auth.service';

/** Số điện thoại di động Việt Nam: 10 chữ số, đầu 03/05/07/08/09 (hoặc +84/84 thay cho số 0). */
const VN_MOBILE = /^(0|\+?84)(3|5|7|8|9)\d{8}$/;

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private baseUrl = environment.apiUrl;
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  orderView = signal<OrderView | null>(null)
  private orderId = signal<number | null>(null);
  private form: FormGroup | null = null;
  /** Dấu vân tay (giỏ hàng + thông tin nhận hàng) của đơn đã tạo trong lần đặt hàng này. */
  private orderSignature: string | null = null;

  /**
   * Form đặt hàng dùng chung cho các trang /order, /order/store và /order/ship
   * nên dữ liệu đã nhập được giữ nguyên khi người dùng quay lại bước trước.
   */
  get paymentForm(): FormGroup {
    if (!this.form) {
      this.form = this.buildForm();
    }
    return this.form;
  }

  /** Xoá dữ liệu đã nhập (dùng khi bắt đầu một lần đặt hàng mới). */
  resetPaymentForm() {
    this.form = null;
    this.orderId.set(null);
    this.orderSignature = null;
  }

  private buildForm(): FormGroup {
    const form = new FormGroup({
      deliveryMethod: new FormControl<'at-store' | 'ship'>('at-store'),
      email: new FormControl('', [Validators.required, Validators.email]),
      storeInfo: new FormGroup({
        city: new FormControl('', Validators.required),
        district: new FormControl('', Validators.required),
        storeId: new FormControl<number | string>('', Validators.required),
        streetName: new FormControl(''),
        note: new FormControl(''),
      }),
      shipInfo: new FormGroup({
        receiverName: new FormControl(this.authService.currentUser()?.fullName ?? '', Validators.required),
        receiverPhone: new FormControl('', [Validators.required, Validators.pattern(VN_MOBILE)]),
        shipCity: new FormControl('', Validators.required),
        shipDistrict: new FormControl('', Validators.required),
        shipWard: new FormControl(''),
        address: new FormControl('', Validators.required),
        note: new FormControl(''),
      }),
    });
    return form;
  }

  createPickupOrder(payload: PickupOrderRequest): Observable<ApiResponse<OrderView>> {
    return this.http.post<ApiResponse<OrderView>>(`${this.baseUrl}/orders/pickup`, payload);
  }

  createDeliveryOrder(payload: DeliveryOrderRequest): Observable<ApiResponse<OrderView>> {
    return this.http.post<ApiResponse<OrderView>>(`${this.baseUrl}/orders/delivery`, payload);
  }

  /**
   * Tạo đơn theo hình thức nhận hàng đang chọn trong form và trả về mã đơn.
   *
   * Giỏ hàng chỉ bị xoá khi thanh toán xong, nên khách có thể quay lại sửa thông tin nhận hàng:
   * - Thông tin y hệt lần trước: dùng lại đơn đã tạo.
   * - Đã thay đổi: huỷ đơn cũ (backend hoàn lại tồn kho) rồi tạo đơn mới, để không còn đơn treo.
   */
  submitOrder(cartDetails: CartView[]): Observable<number> {
    const value = this.paymentForm.getRawValue();
    const cartDetailIds = cartDetails.map(c => c.cartDetailId);
    const isPickup = value.deliveryMethod === 'at-store';
    const pickup: PickupOrderRequest = { cartDetailIds, storeHouseId: Number(value.storeInfo.storeId) };
    const delivery: DeliveryOrderRequest = {
      cartDetailIds,
      provinceName: value.shipInfo.shipCity || '',
      districtName: value.shipInfo.shipDistrict || '',
      street: value.shipInfo.address || '',
    };
    const signature = JSON.stringify(isPickup ? pickup : delivery);

    const existingId = this.orderId();
    if (existingId !== null && signature === this.orderSignature) {
      return of(existingId);
    }

    const create$ = defer(() => isPickup ? this.createPickupOrder(pickup) : this.createDeliveryOrder(delivery)).pipe(
      map(response => response.data.id),
      tap(id => {
        this.orderId.set(id);
        this.orderSignature = signature;
      }),
    );
    if (existingId === null) {
      return create$;
    }
    return this.cancelOrder(existingId).pipe(
      tap(() => {
        this.orderId.set(null);
        this.orderSignature = null;
      }),
      switchMap(() => create$),
    );
  }

  /** Lịch sử mua hàng của người dùng đang đăng nhập, mới nhất trước. */
  getMyOrders(pageIndex = 1, pageSize = 10): Observable<PagedResult<OrderHistory>> {
    return this.http.get<ApiResponse<PagedResult<OrderHistory>>>(`${this.baseUrl}/orders/history/me`, {
      params: { pageIndex, pageSize },
    }).pipe(map(response => response.data));
  }

  /** Huỷ đơn chưa hoàn tất; backend hoàn lại tồn kho đã giữ cho đơn. */
  cancelOrder(orderId: number): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(`${this.baseUrl}/orders/${orderId}`);
  }

  /** Khách bấm hoàn tất (thanh toán khi nhận hàng): backend xoá các sản phẩm đã đặt khỏi giỏ hàng. */
  confirmOrder(orderId: number): Observable<ApiResponse<string>> {
    return this.http.get<ApiResponse<string>>(`${this.baseUrl}/orders/checkout/${orderId}`);
  }

  /** Trạng thái đơn (Pending, Paid, …). Hỏi định kỳ nên không bật spinner toàn trang. */
  getOrderStatus(orderId: number): Observable<string> {
    return this.http.get<ApiResponse<string>>(`${this.baseUrl}/orders/${orderId}/status`, {
      context: new HttpContext().set(SKIP_GLOBAL_SPINNER, true),
    }).pipe(map(response => response.data));
  }

  /** Tổng tiền các dòng giỏ hàng theo giá bán. */
  totalOf(cartDetails: CartView[]): number {
    return cartDetails.reduce((sum, c) => sum + (c.salePrice || 0) * (c.quantity || 0), 0);
  }

  getOrderId(): number | null {
    return this.orderId();
  }
}
