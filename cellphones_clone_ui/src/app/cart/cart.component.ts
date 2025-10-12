import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
export interface CartDetail {
  src: string;
  title: string;
  priceAfterDiscount: number;
  realPrice: number;
}
@Component({
  selector: 'app-cart',
  imports: [],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css',
})
export class CartComponent {
  carts: CartDetail[] = [
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:350:0/q:80/plain/https://cellphones.com.vn/media/catalog/product/g/r/group_784.png',
      title: 'PC CPS X MSI Gaming Intel i5 Gen 12 Kèm màn hình',
      priceAfterDiscount: 20000000,
      realPrice: 23000000
    },
  ];
}
