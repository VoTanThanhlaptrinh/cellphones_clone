import { Injectable } from '@angular/core';
import { HttpClient, HttpContext } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '@core/models/api.model';
import { SKIP_GLOBAL_SPINNER } from '@core/http/loading.context';
import { OnlinePayment, OnlineProvider } from '@core/models/payment.model';

/** Khoá sessionStorage để trang /payment/return biết giao dịch nào vừa rời đi thanh toán (không tin tham số URL). */
export const PENDING_PAYMENT_ID_KEY = 'pendingPaymentId';

/** Thanh toán online qua payment gateway. Kết quả chỉ tin từ backend (backend hỏi gateway), không tin tham số URL khi khách quay về. */
@Injectable({
    providedIn: 'root'
})
export class PaymentService {
    private baseUrl = environment.apiUrl;
    constructor(private http: HttpClient) { }

    /** Tạo giao dịch; nếu đơn đã có giao dịch còn hiệu lực thì backend trả lại giao dịch đó (có thể thuộc cổng khác cổng vừa chọn). */
    startCheckout(orderId: number, provider: OnlineProvider): Observable<OnlinePayment> {
        return this.http.post<ApiResponse<OnlinePayment>>(`${this.baseUrl}/payments/checkout`, { orderId, provider })
            .pipe(map(response => response.data));
    }

    /** Giao dịch theo mã. `silent` cho các lần hỏi định kỳ, không bật spinner toàn trang. */
    getPayment(id: number, silent = false): Observable<OnlinePayment> {
        const context = new HttpContext().set(SKIP_GLOBAL_SPINNER, silent);
        return this.http.get<ApiResponse<OnlinePayment>>(`${this.baseUrl}/payments/${id}`, { context })
            .pipe(map(response => response.data));
    }
}
