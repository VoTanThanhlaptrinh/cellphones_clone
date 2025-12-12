import { Component, Input } from '@angular/core';
import { ProductView } from '../core/models/product.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-product-card',
  imports: [],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.css'
})
export class ProductCardComponent {
  @Input() productView?: ProductView;
  constructor(private router: Router) {}
    goProductDetail(id:number) {
    this.router.navigate(['/product', id]);
  }
}
