import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Subscription, timer } from 'rxjs';
import { HeaderCartOrderComponent } from '@store/layout/header-cart-order/header-cart-order.component';
import { CartService } from '@store/services/cart.service';
import { PaymentService, PENDING_PAYMENT_ORDER_KEY } from '@store/services/payment.service';
import { OnlinePayment } from '@core/models/payment.model';
import { VndPipe } from '@shared/pipes/vnd.pipe';
import { IconComponent } from '@shared/ui/icon/icon.component';

/** Số giây chờ giữa lúc báo thanh toán thành công và lúc tự chuyển về trang chủ. */
const REDIRECT_DELAY_SECONDS = 10;

/**
 * Trang thanh toán thành công (/payment/success) – cấu hình làm returnUrl của payment gateway. Không tin URL:
 * mã đơn lấy từ phiên trình duyệt, kết quả hỏi lại backend. Chưa thanh toán xong thì đưa khách về đúng trang
 * (VNPay dùng chung một return URL cho cả thành công lẫn thất bại).
 */
@Component({
  selector: 'app-payment-success',
  standalone: true,
  imports: [HeaderCartOrderComponent, IconComponent, RouterLink, VndPipe],
  templateUrl: './payment-success.component.html',
})
export class PaymentSuccessComponent implements OnInit, OnDestroy {
  state = signal<'checking' | 'paid' | 'review'>('checking');
  payment = signal<OnlinePayment | null>(null);
  /** Số giây còn lại trước khi tự chuyển về trang chủ (chỉ có khi đã thanh toán). */
  redirectIn = signal<number | null>(null);
  private poll?: Subscription;
  private redirect?: Subscription;

  constructor(
    private router: Router,
    private paymentService: PaymentService,
    private cartService: CartService) { }

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
      sessionStorage.removeItem(PENDING_PAYMENT_ORDER_KEY);
      this.cartService.updateCartItems([]);
      this.payment.set(payment);
      this.state.set('paid');
      this.startRedirectCountdown();
    } else if (payment.status === 'REVIEW_REQUIRED') {
      this.payment.set(payment);
      this.state.set('review');
    } else if (payment.status === 'PENDING') {
      this.router.navigate(['/payment', payment.orderId], { replaceUrl: true });
    } else {
      this.router.navigate(['/payment/failed'], { replaceUrl: true });
    }
  }

  private startRedirectCountdown() {
    this.redirectIn.set(REDIRECT_DELAY_SECONDS);
    this.redirect = timer(1000, 1000).subscribe(() => {
      const left = (this.redirectIn() ?? 1) - 1;
      this.redirectIn.set(left);
      if (left <= 0) {
        this.redirect?.unsubscribe();
        this.router.navigate(['/home']);
      }
    });
  }

  ngOnDestroy() {
    this.poll?.unsubscribe();
    this.redirect?.unsubscribe();
  }
}
