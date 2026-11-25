import { Component, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { NgClass } from '@angular/common';
import { HeaderCartOrderComponent } from '@store/layout/header-cart-order/header-cart-order.component';
import { AuthService } from '@core/auth/auth.service';
import { CartService } from '@store/services/cart.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [HeaderCartOrderComponent, RouterLink],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css'
})
export class CheckoutComponent implements OnInit {
  orderId = signal<number | null>(null);
  isSuccess = signal<boolean>(false);

  constructor(
    private router: Router,
    private cartService: CartService,
    private authService: AuthService
  ) { }

  ngOnInit() {
    const state = history.state;
    if (state && state.success && state.orderId) {
      this.isSuccess.set(true);
      this.orderId.set(state.orderId);
      // Clear the local cart state
      this.cartService.updateCartItems([]);
      // Số trên icon giỏ hàng ở header lấy từ claim amountCart trong JWT: phải xin token mới thì mới cập nhật.
      this.authService.refreshToken(true).subscribe({ error: () => { } });
    } else {
      // If accessed without proper state, redirect home
      this.router.navigate(['/home']);
    }
  }
}
