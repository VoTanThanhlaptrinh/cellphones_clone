import { Component, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { OrderService } from '@store/services/order.service';
import { NotifyService } from '@core/services/notify.service';
import { FormsModule } from '@angular/forms';
import { CartView } from '@core/models/cart_request.model';
import { OnlineProvider } from '@core/models/payment.model';
import { PaymentService } from '@store/services/payment.service';
import { OrderStepHeaderComponent } from '@store/components/order-step-header/order-step-header.component';
import { OrderBottomBarComponent } from '@store/components/order-bottom-bar/order-bottom-bar.component';

/** Bước 2 của luồng đặt hàng: chọn phương thức thanh toán. Thanh toán online chuyển sang trang giao dịch /payment/:id. */
@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [FormsModule, OrderStepHeaderComponent, OrderBottomBarComponent],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.css',
})
export class PaymentComponent implements OnInit {
  cartDetails: CartView[] = [];
  totalPrice: number = 0;
  selectedMethod = signal<'COD' | 'ONLINE'>('COD');
  selectedProvider = signal<OnlineProvider>('PAYOS');
  isLoading = signal<boolean>(false);

  constructor(
    private router: Router,
    private orderService: OrderService,
    private paymentService: PaymentService,
    private notifyService: NotifyService) { }

  ngOnInit() {
    const state = history.state;
    if (state && state.cartDetails) {
      this.cartDetails = state.cartDetails;
      this.totalPrice = this.orderService.totalOf(this.cartDetails);
    } else {
      this.router.navigate(['/cart']);
    }
  }

  /** Quay lại bước thông tin (trang chọn hình thức nhận hàng). */
  goPaymentInfor() {
    this.router.navigate(['/order'], { state: { cartDetails: this.cartDetails } });
  }

  /**
   * Thanh toán online: tạo giao dịch rồi sang trang /payment/:id (mã giao dịch nằm trên URL nên tải lại không mất).
   * Đơn đã có giao dịch còn hiệu lực thì backend trả lại chính giao dịch đó.
   */
  private payOnline(orderId: number) {
    this.isLoading.set(true);
    this.paymentService.startCheckout(orderId, this.selectedProvider()).subscribe({
      next: payment => this.router.navigate(['/payment', payment.id]),
      error: () => this.isLoading.set(false),
    });
  }

  /** Thanh toán khi nhận hàng: đơn đã được tạo ở bước thông tin, giờ xác nhận để xoá các sản phẩm đã đặt khỏi giỏ hàng. */
  completeOrder() {
    const orderId = this.orderService.getOrderId();
    if (!orderId) {
      this.notifyService.error('Không tìm thấy đơn hàng. Vui lòng quay lại bước thông tin.');
      return;
    }
    if (this.selectedMethod() === 'ONLINE') {
      this.payOnline(orderId);
      return;
    }
    this.isLoading.set(true);
    this.orderService.confirmOrder(orderId).subscribe({
      next: () => this.router.navigate(['/checkout'], { state: { orderId, success: true } }),
      error: () => this.isLoading.set(false),
    });
  }

  /** Nhãn nút chính ở thanh đáy. */
  get actionLabel(): string {
    if (this.isLoading()) return 'Đang xử lý…';
    return this.selectedMethod() === 'ONLINE' ? 'Tiếp tục thanh toán' : 'Hoàn tất đặt hàng';
  }
}
