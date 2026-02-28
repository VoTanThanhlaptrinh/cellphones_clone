import { Component, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { NgClass } from '@angular/common';
import { HeaderCartOrderComponent } from '../shared/header-cart-order/header-cart-order.component';
import { CartService } from '../services/cart.service';

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
    private cartService: CartService
  ) { }

  ngOnInit() {
    const state = history.state;
    if (state && state.success && state.orderId) {
      this.isSuccess.set(true);
      this.orderId.set(state.orderId);
      // Clear the local cart state
      this.cartService.updateCartItems([]);
    } else {
      // If accessed without proper state, redirect home
      this.router.navigate(['/home']);
    }
  }
}
