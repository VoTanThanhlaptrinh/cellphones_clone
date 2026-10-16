import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { HeaderCartOrderComponent } from '@store/layout/header-cart-order/header-cart-order.component';
import { PaymentService, PENDING_PAYMENT_ID_KEY } from '@store/services/payment.service';
import { OnlinePayment } from '@core/models/payment.model';

/**
 * Trang khách quay về sau khi thanh toán ở cổng (returnUrl/cancelUrl cấu hình ở payment gateway). Không tin tham số URL:
 * mã giao dịch lấy từ phiên trình duyệt, kết quả hỏi lại backend (backend hỏi gateway).
 * Giao dịch còn hiệu lực (khách bấm quay lại giữa chừng) thì đưa về trang giao dịch /payment/:id để thanh toán tiếp.
 */
@Component({
  selector: 'app-payment-return',
  standalone: true,
  imports: [HeaderCartOrderComponent, RouterLink],
  templateUrl: './payment-return.component.html',
})
export class PaymentReturnComponent implements OnInit, OnDestroy {
  state = signal<'checking' | 'failed' | 'review'>('checking');
  private poll?: Subscription;

  constructor(
    private router: Router,
    private paymentService: PaymentService) { }

  ngOnInit() {
    const id = Number(sessionStorage.getItem(PENDING_PAYMENT_ID_KEY));
    if (!(id > 0)) {
      this.router.navigate(['/home']);
      return;
    }
    this.poll = this.paymentService.getPayment(id).subscribe({
      next: payment => this.onStatus(id, payment),
      error: () => this.router.navigate(['/home']),
    });
  }

  private onStatus(id: number, payment: OnlinePayment) {
    if (payment.status === 'PAID') {
      sessionStorage.removeItem(PENDING_PAYMENT_ID_KEY);
      this.router.navigate(['/checkout'], { state: { orderId: payment.orderId, success: true }, replaceUrl: true });
    } else if (payment.status === 'REVIEW_REQUIRED') {
      this.state.set('review');
    } else if (payment.status === 'PENDING') {
      // Chưa có kết quả (khách bấm quay lại hoặc tiền chưa về): trang giao dịch tiếp tục hiển thị, đếm ngược và hỏi kết quả.
      this.router.navigate(['/payment', id], { replaceUrl: true });
    } else {
      sessionStorage.removeItem(PENDING_PAYMENT_ID_KEY);
      this.state.set('failed');
    }
  }

  ngOnDestroy() {
    this.poll?.unsubscribe();
  }
}
