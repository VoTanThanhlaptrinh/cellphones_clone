import { Component, CUSTOM_ELEMENTS_SCHEMA, inject, OnInit } from '@angular/core';

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
@Component({
  selector: 'app-order',
  imports: [HeaderCartOrderComponent, MatTabsModule, CommonModule, FormsModule, ReactiveFormsModule, PaymentComponent, PaymentInforComponent],
  templateUrl: './order.component.html',
  styleUrl: './order.component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})

export class OrderComponent implements OnInit {
  totalQuantity?: number;
  isAppear: boolean = true;
  storeViews?: StoreView[];
  constructor(
    private orderService: OrderService,
    private cartService: CartService,
    private notifySerivce: NotifyService,
  ) { }
  ngOnInit(): void {
    this.totalQuantity = this.cartService.cartQuantity();
    this.getStoreViews();
  }
  allowAppear(event: any) {
    this.isAppear = event
  }
  getStoreViews() {
    this.orderService.getStoreView().subscribe({
      next: res => {
        this.storeViews = res
      },
      error: err => this.notifySerivce.error(err)
    })
  }

}
