import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { HeaderCartOrderComponent } from '@store/layout/header-cart-order/header-cart-order.component';
import { PaymentService, PENDING_PAYMENT_ORDER_KEY } from '@store/services/payment.service';
import { OnlinePayment } from '@core/models/payment.model';
import { IconComponent } from '@shared/ui/icon/icon.component';

/**
 * Trang thanh toán không thành công (/payment/failed) – cấu hình làm cancelUrl của payment gateway. Không tin URL:
 * mã đơn lấy từ phiên trình duyệt, kết quả hỏi lại backend. Khách thật ra đã trả tiền thì sang /payment/success,
 * giao dịch còn hiệu lực thì về trang giao dịch để thanh toán tiếp.
 */
@Component({
  selector: 'app-payment-failed',
  standalone: true,
  imports: [HeaderCartOrderComponent, IconComponent, RouterLink],
  templateUrl: './payment-failed.component.html',
})
export class PaymentFailedComponent implements OnInit, OnDestroy {
  state = signal<'checking' | 'failed' | 'review'>('checking');
  orderId = signal<number | null>(null);
  private poll?: Subscription;

  constructor(
    private router: Router,
    private paymentService: PaymentService) { }

  ngOnInit() {
    const orderId = Number(sessionStorage.getItem(PENDING_PAYMENT_ORDER_KEY));
    if (!(orderId > 0)) {
      this.router.navigate(['/home']);
      return;
    }
    this.poll = this.paymentService.getPayment(orderId).subscribe({
      next: payment => this.onStatus(payment),
      error: () => this.router.navigate(['/home']),
    });
  }

  private onStatus(payment: OnlinePayment) {
    if (payment.status === 'PAID') {
      this.router.navigate(['/payment/success'], { replaceUrl: true });
    } else if (payment.status === 'PENDING') {
      this.router.navigate(['/payment', payment.orderId], { replaceUrl: true });
    } else if (payment.status === 'REVIEW_REQUIRED') {
      this.orderId.set(payment.orderId);
      this.state.set('review');
    } else {
      sessionStorage.removeItem(PENDING_PAYMENT_ORDER_KEY);
      this.orderId.set(payment.orderId);
      this.state.set('failed');
    }
  }

  ngOnDestroy() {
    this.poll?.unsubscribe();
  }
}
