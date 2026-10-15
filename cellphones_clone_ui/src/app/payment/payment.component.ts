import { Component, effect, OnDestroy, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { OrderService } from '../services/order.service';
import { NotifyService } from '../services/notify.service';
import { FormsModule } from '@angular/forms';
import { catchError, EMPTY, filter, Subscription, switchMap, take, timer } from 'rxjs';
import { CartView } from '../core/models/cart_request.model';
import { PaymentService } from '../services/payment.service';
import { OrderStepHeaderComponent } from '../shared/order-step-header/order-step-header.component';
import { OrderBottomBarComponent } from '../shared/order-bottom-bar/order-bottom-bar.component';

/** Bước 2 của luồng đặt hàng: chọn phương thức thanh toán. */
@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [FormsModule, OrderStepHeaderComponent, OrderBottomBarComponent],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.css',
})
export class PaymentComponent implements OnInit, OnDestroy {
  cartDetails: CartView[] = [];
  totalPrice: number = 0;
  selectedMethod = signal<'COD' | 'ONLINE'>('COD');
  isLoading = signal<boolean>(false);
  /** Đọc trực tiếp từ signal của service để cập nhật khi QR được tạo xong. */
  get qrCodeUrl(): string | null {
    return this.paymentService.getQr();
  }


  constructor(
    private router: Router,
    private orderService: OrderService,
    private paymentService: PaymentService,
    private notifyService: NotifyService) {
    effect(() => {
      if (this.selectedMethod() === 'ONLINE') {
        const orderId = this.orderService.getOrderId();
        if (orderId) {
          this.paymentService.generateQRPaymentLink(orderId);
          this.watchPayment(orderId);
        }
      } else {
        this.stopWatching();
      }
    })
  }

  private paymentWatch?: Subscription;

  /** Hỏi trạng thái đơn mỗi 3 giây; khi webhook PayOS đã đánh dấu Paid thì sang trang đặt hàng thành công. */
  private watchPayment(orderId: number) {
    if (this.paymentWatch) return;
    this.paymentWatch = timer(3000, 3000).pipe(
      switchMap(() => this.orderService.getOrderStatus(orderId).pipe(catchError(() => EMPTY))),
      filter(status => status === 'Paid'),
      take(1),
    ).subscribe(() => this.router.navigate(['/checkout'], { state: { orderId, success: true } }));
  }

  private stopWatching() {
    this.paymentWatch?.unsubscribe();
    this.paymentWatch = undefined;
  }

  ngOnDestroy() {
    this.stopWatching();
  }

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

  /** Thanh toán khi nhận hàng: đơn đã được tạo ở bước thông tin, giờ xác nhận để xoá các sản phẩm đã đặt khỏi giỏ hàng. */
  completeOrder() {
    const orderId = this.orderService.getOrderId();
    if (!orderId) {
      this.notifyService.error('Không tìm thấy đơn hàng. Vui lòng quay lại bước thông tin.');
      return;
    }
    this.isLoading.set(true);
    this.orderService.confirmOrder(orderId).subscribe({
      next: () => this.router.navigate(['/checkout'], { state: { orderId, success: true } }),
      error: () => this.isLoading.set(false),
    });
  }
}
