import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { HeaderCartOrderComponent } from '../shared/header-cart-order/header-cart-order.component';
import { AuthService } from '../services/auth.service';
import { OrderService } from '../services/order.service';

/**
 * Khung chung của luồng đặt hàng. Các bước là route con:
 * /order (thông tin) → /order/store hoặc /order/ship (địa chỉ) → /order/payment (thanh toán).
 */
@Component({
  selector: 'app-order',
  imports: [HeaderCartOrderComponent, RouterOutlet],
  templateUrl: './order.component.html',
  styleUrl: './order.component.css',
})
export class OrderComponent implements OnInit, OnDestroy {
  totalQuantity?: number;

  constructor(
    private orderService: OrderService,
    private authService: AuthService,
    private router: Router) { }

  ngOnInit(): void {
    // Vào luồng đặt hàng mà không có giỏ hàng (tải lại trang, gõ thẳng URL) thì quay về trang chủ.
    if (!history.state?.cartDetails) {
      this.router.navigate(['/home']);
      return;
    }
    this.orderService.resetPaymentForm();
    this.totalQuantity = this.authService.currentUser()?.amountCart;
  }

  ngOnDestroy(): void {
    this.orderService.resetPaymentForm();
  }
}
