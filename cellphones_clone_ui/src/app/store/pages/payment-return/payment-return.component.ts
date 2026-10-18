import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { HeaderCartOrderComponent } from '@store/layout/header-cart-order/header-cart-order.component';
import { PaymentService, PENDING_PAYMENT_ORDER_KEY } from '@store/services/payment.service';
import { OnlinePayment } from '@core/models/payment.model';

/**
 * Trang trung gian /payment/return (returnUrl/cancelUrl cũ của payment gateway; cấu hình mới nên trỏ thẳng tới
 * /payment/success và /payment/failed). Không tin tham số URL: mã đơn lấy từ phiên trình duyệt, kết quả hỏi lại backend,
 * rồi chuyển tới trang tương ứng. Giao dịch còn hiệu lực (khách bấm quay lại giữa chừng) thì về /payment/:orderId.
 */
@Component({
  selector: 'app-payment-return',
  standalone: true,
  imports: [HeaderCartOrderComponent],
  templateUrl: './payment-return.component.html',
})
export class PaymentReturnComponent implements OnInit, OnDestroy {
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
    } else {
      // FAILED, EXPIRED, CANCELLED, REVIEW_REQUIRED: trang thất bại tự phân biệt trường hợp cần kiểm tra.
      this.router.navigate(['/payment/failed'], { replaceUrl: true });
    }
  }

  ngOnDestroy() {
    this.poll?.unsubscribe();
  }
}
