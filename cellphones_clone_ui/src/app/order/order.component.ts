import { Component, CUSTOM_ELEMENTS_SCHEMA, inject, OnInit } from '@angular/core';
import { HeaderCartOrderComponent } from "../header-cart-order/header-cart-order.component";
import { CartService } from '../services/cart.service';
import { MatTabsModule } from '@angular/material/tabs';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { UserView } from '../core/models/user_response.model';
import { FormsModule } from '@angular/forms';
import { IgxInputDirective, IgxInputGroupComponent, IgxLabelDirective, IgxIconComponent, IgxPrefixDirective, IgxSuffixDirective, IgxHintDirective, IgxGridComponent, IgxColumnComponent } from 'igniteui-angular';
@Component({
  selector: 'app-order',
  imports: [HeaderCartOrderComponent, MatTabsModule, CommonModule, FormsModule],
  templateUrl: './order.component.html',
  styleUrl: './order.component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})

export class OrderComponent implements OnInit {
  totalQuantity?: number;
  email?: string;
  private currency = inject(CurrencyPipe);
  user?: UserView = {
    name: "Võ Tấn Thành", mobile: "0796692184", email: "votanthanh32004@gmail.com"
  };
  constructor(private cartService: CartService) {
  }
  ngOnInit(): void {
    this.totalQuantity = this.cartService.cartQuantity();
  }
  convertPriceVnd(price: number | null | undefined): string {
    if (price == null) return 'Đang cập nhật';
    const formatted = this.currency.transform(price, 'VND', 'symbol', '1.0-0') ?? '';
    return formatted.replace('₫', 'đ');
  }
}
