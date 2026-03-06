import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { OrderService } from '../services/order.service';
import { CurrencyPipe, NgClass } from '@angular/common';
import { NotifyService } from '../services/notify.service';
import { FormsModule } from '@angular/forms';
import { CartView } from '../core/models/cart_request.model';
import { DeliveryOrderRequest, PickupOrderRequest } from '../core/models/order.model';
import { PaymentService } from '../services/payment.service';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [NgClass, FormsModule],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.css',
  providers: [CurrencyPipe]
})
export class PaymentComponent implements OnInit {
  cartDetails: CartView[] = [];
  totalPrice: number = 0;

  selectedMethod = signal<'COD' | 'ONLINE'>('COD');
  isLoading = signal<boolean>(false);
  qrCodeUrl = signal<string | null>(null);

  private currency = inject(CurrencyPipe);

  constructor(
    private router: Router,
    private orderService: OrderService,
    private paymentService: PaymentService,
    private notifyService: NotifyService) { }

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

  createOrder() {
    this.isLoading.set(true);
    const paymentData = this.orderService.getPaymentData();

    if (!paymentData) {
      this.notifyService.error('Không tìm thấy thông tin giao hàng!');
      this.isLoading.set(false);
      return;
    }

    const cartIds = this.cartDetails.map(c => c.cartDetailId);

    if (paymentData.deliveryMethod === 'at-store') {
      const payload: PickupOrderRequest = {
        cartDetailIds: cartIds,
        storeHouseId: Number(paymentData.storeInfo?.storeId)
      };

      this.orderService.createPickupOrder(payload).subscribe({
        next: (response) => this.handleOrderSuccess(response.data.id),
        error: (err) => this.handleOrderError(err)
      });
    } else {
      const payload: DeliveryOrderRequest = {
        cartDetailIds: cartIds,
        provinceName: paymentData.shipInfo?.shipCity || '',
        districtName: paymentData.shipInfo?.shipDistrict || '',
        street: paymentData.shipInfo?.address || ''
      };

      this.orderService.createDeliveryOrder(payload).subscribe({
        next: (response) => this.handleOrderSuccess(response.data.id),
        error: (err) => this.handleOrderError(err)
      });
    }
  }

  private handleOrderSuccess(orderId: number) {
    if (this.selectedMethod() === 'ONLINE') {
      this.notifyService.success('Đang tạo mã thanh toán QR...');
      this.paymentService.generateQRPaymentLink(orderId).subscribe({
        next: (response) => {
          this.qrCodeUrl.set(response.data);
          this.isLoading.set(false);
          this.orderService.clearPaymentData();
          // Keep user on the page to scan the QR
        },
        error: (err) => {
          this.notifyService.error('Lỗi khi tạo mã QR thanh toán.');
          this.isLoading.set(false);
        }
      });
    } else {
      this.notifyService.success('Tạo đơn hàng thành công!');
      this.isLoading.set(false);
      this.orderService.clearPaymentData();
      this.router.navigate(['/checkout'], { state: { orderId: orderId, success: true } });
    }
  }

  private handleOrderError(err: any) {
    this.notifyService.error(err.error?.message || 'Có lỗi xảy ra khi tạo đơn hàng');
    this.isLoading.set(false);
  }

  convertPriceVnd(price: number): string {
    const formatted = this.currency.transform(price, 'VND', 'symbol', '1.0-0') ?? '';
    return formatted.replace('₫', 'đ');
  }

  calculateTotals() {
    if (this.cartDetails && this.cartDetails.length > 0) {
      return this.cartDetails.reduce((acc, cart) => acc + ((cart.salePrice || 0) * (cart.quantity || 0)), 0);
    } else {
      return 0;
    }
  }
}
