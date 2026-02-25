import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { OrderService } from '../services/order.service';
import { Order, OrderDetail } from '../core/models/order.model';
import { CurrencyPipe, NgClass } from '@angular/common';

import { NotifyService } from '../services/notify.service';
import { FormsModule } from '@angular/forms';
import { HeaderCartOrderComponent } from '../shared/header-cart-order/header-cart-order.component';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [NgClass, HeaderCartOrderComponent, FormsModule, RouterLink],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css',
  providers: [CurrencyPipe]
})
export class CheckoutComponent implements OnInit {
  order: Order | null = null;
  isLoading: boolean = true;
  totalQuantity: number = 0;
  totalPrice: number = 0;

  paymentMethod: 'ONLINE' | 'COD' = 'COD';

  private currency = inject(CurrencyPipe);

  constructor(
    private router: Router,
    private orderService: OrderService,
    private notifyService: NotifyService
  ) { }

  ngOnInit() {
    const state = history.state;
    if (state && state.cartDetailIds) {
      this.orderService.createOrder(state.cartDetailIds).subscribe({
        next: (response) => {
          this.order = response.data;
          this.calculateTotals();
        },
        error: (err) => {
          this.notifyService.error('Có lỗi xảy ra khi tạo đơn hàng');
          this.isLoading = false;
        },
        complete: () => {
          this.isLoading = false;
        }
      });
    } else {
      this.router.navigate(['/cart']);
    }
  }

  calculateTotals() {
    if (this.order && this.order.orderDetails) {
      this.totalQuantity = this.order.orderDetails.reduce((acc, curr) => acc + curr.quantity, 0);
      this.totalPrice = this.order.orderDetails.reduce((acc, curr) => acc + (curr.price * curr.quantity), 0);
    }
  }

  convertPriceVnd(price: number | null | undefined): string {
    if (price == null) return 'Đang cập nhật';
    const formatted = this.currency.transform(price, 'VND', 'symbol', '1.0-0') ?? '';
    return formatted.replace('₫', 'đ');
  }

  confirmPayment() {
    if (this.paymentMethod === 'ONLINE') {
      this.notifyService.success('Đang chuyển hướng đến cổng thanh toán...');
      setTimeout(() => {
        this.router.navigate(['/home']);
      }, 1500);
    } else {
      this.notifyService.success('Đặt hàng thành công! Đơn hàng sẽ được giao đến bạn.');
      setTimeout(() => {
        this.router.navigate(['/home']);
      }, 1500);
    }
  }
}
