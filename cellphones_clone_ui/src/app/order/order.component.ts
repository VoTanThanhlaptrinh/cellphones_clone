import { Component, computed, CUSTOM_ELEMENTS_SCHEMA, effect, inject, OnInit, signal } from '@angular/core';

import { CartService } from '../services/cart.service';
import { MatTabsModule } from '@angular/material/tabs';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PaymentComponent } from "../payment/payment.component";
import { PaymentInforComponent } from "../payment-infor/payment-infor.component";
import { StoreView } from '../core/models/store.model';
import { OrderService } from '../services/order.service';
import { NotifyService } from '../services/notify.service';
import { HeaderCartOrderComponent } from '../shared/header-cart-order/header-cart-order.component';
import { AuthService } from '../services/auth.service';
import { CartView } from '../core/models/cart_request.model';
import { User } from '../core/models/user.model';
import { Router } from '@angular/router';
import { OrderView } from '../core/models/order.model';
@Component({
  selector: 'app-order',
  imports: [HeaderCartOrderComponent, MatTabsModule, CommonModule, FormsModule, ReactiveFormsModule, PaymentComponent, PaymentInforComponent],
  templateUrl: './order.component.html',
  styleUrl: './order.component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})

export class OrderComponent implements OnInit {
  orderView: OrderView | null = null;
  cartDetails: CartView[] = [];
  totalQuantity?: number;
  isAppear = computed(() => this.orderService.IsAppear);
  username = signal<User | null>(null);
  totalPrice: number | undefined = 0;
  constructor(
    private orderService: OrderService,
    private authService: AuthService,
    private router: Router) {
    effect(() => {
      this.orderView = this.orderService.orderView()
    })
  }
  ngOnInit(): void {
    const state = history.state;
    this.username.set(this.authService.currentUser());
    if (state && state.cartDetails) {
      this.cartDetails = state.cartDetails;
    } else {
      this.router.navigate(['/home']);
    }
    this.totalQuantity = this.authService.currentUser()?.amountCart;
  }
  allowAppear(event: any) {
    this.isAppear = event
  }
}