import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-header-cart-order',
  imports: [],
  templateUrl: './header-cart-order.component.html',
  styleUrl: './header-cart-order.component.css'
})
export class HeaderCartOrderComponent {
  constructor(private router: Router) {

  }
  @Input() totalQuantity?: number;
  @Input() Username?: string = 'Thành';

  goHome() {
    this.router.navigate(['/home']);
  }
}
