import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { OrderService } from '../services/order.service';
import { NotifyService } from '../services/notify.service';
import { FormsModule } from '@angular/forms';
import { CartView } from '../core/models/cart_request.model';
import { DeliveryOrderRequest, PickupOrderRequest } from '../core/models/order.model';
import { PaymentService } from '../services/payment.service';
import { VndPipe } from '../shared/pipes/vnd.pipe';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [FormsModule, VndPipe],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.css',
})
export class PaymentComponent implements OnInit {
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
        }
      }
    })
  }

  ngOnInit() {
    const state = history.state;
    if (state && state.cartDetails) {
      this.cartDetails = state.cartDetails;
      this.totalPrice = this.calculateTotals();
    } else {
      this.router.navigate(['/cart']);
    }
  }
  goPaymentInfor() {
    this.orderService.setAppear(true)
  }

  /** Đơn hàng đã được tạo ở bước thông tin; COD chỉ cần chuyển sang trang đặt hàng thành công. */
  completeOrder() {
    const orderId = this.orderService.getOrderId();
    if (!orderId) {
      this.notifyService.error('Không tìm thấy đơn hàng. Vui lòng quay lại bước thông tin.');
      return;
    }
    this.router.navigate(['/checkout'], { state: { orderId, success: true } });
  }



  calculateTotals() {
    if (this.cartDetails && this.cartDetails.length > 0) {
      return this.cartDetails.reduce((acc, cart) => acc + ((cart.salePrice || 0) * (cart.quantity || 0)), 0);
    } else {
      return 0;
    }
  }
}
