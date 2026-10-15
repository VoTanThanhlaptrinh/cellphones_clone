import { NgOptimizedImage } from '@angular/common';
import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProductView } from '@core/models/product.model';
import { VndPipe } from '@shared/pipes/vnd.pipe';

@Component({
  selector: 'app-product-card',
  imports: [RouterLink, VndPipe, NgOptimizedImage],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.css'
})
export class ProductCardComponent {
  @Input() productView?: ProductView;

  /** % giảm giá tính từ dữ liệu thật; 0 nghĩa là không có giảm giá. */
  get discountPercent(): number {
    const p = this.productView;
    if (!p || !p.salePrice || !p.basePrice || p.basePrice <= p.salePrice) return 0;
    return Math.round(((p.basePrice - p.salePrice) / p.basePrice) * 100);
  }
}
