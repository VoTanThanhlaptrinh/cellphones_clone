import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-header-cart-order',
  imports: [],
  templateUrl: './header-cart-order.component.html',
  styleUrl: './header-cart-order.component.css'
})
export class HeaderCartOrderComponent {
  constructor(){

  }
  @Input() totalQuantity?: number;
  @Input() Username?: string = 'Thành';
}
