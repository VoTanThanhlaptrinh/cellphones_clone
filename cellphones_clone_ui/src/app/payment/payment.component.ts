import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { OrderService } from '../services/order.service';
import { CurrencyPipe, NgClass } from '@angular/common';
import { NotifyService } from '../services/notify.service';
import { FormsModule } from '@angular/forms';
import { CartView } from '../core/models/cart_request.model';

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

  private currency = inject(CurrencyPipe);

  constructor(
    private router: Router,
    private orderService: OrderService,
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

    // Simulate API call and success for either COD or ONLINE payment
    setTimeout(() => {
      this.isLoading.set(false);
      this.notifyService.success(this.selectedMethod() === 'ONLINE' ? 'Chuyển hướng đến cổng thanh toán...' : 'Tạo đơn hàng thành công!');

      // Simulate redirection to checkout success view
      this.router.navigate(['/checkout'], { state: { success: true } });
    }, 1500);

    // If using the real API, uncomment the following and adapt cart details payload:
    /*
    this.orderService.createOrder(this.cartDetails.map(c => c.id)).subscribe({
      next: (response) => {
        this.notifyService.success('Thanh toán thành công!');
        this.router.navigate(['/checkout'], { state: { orderId: response.data.id, success: true } });
      },
      error: (err) => {
        this.notifyService.error('Có lỗi xảy ra khi tạo đơn hàng');
        this.isLoading.set(false);
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
    */
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
