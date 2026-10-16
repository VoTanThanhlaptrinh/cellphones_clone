import { NgOptimizedImage } from '@angular/common';
import { Component, Injector, Input, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { take } from 'rxjs';
import { ProductView } from '@core/models/product.model';
import { AuthService } from '@core/auth/auth.service';
import { CartService } from '@store/services/cart.service';
import { NotifyService } from '@core/services/notify.service';
import { IconComponent } from '@shared/ui/icon/icon.component';
import { VndPipe } from '@shared/pipes/vnd.pipe';

@Component({
  selector: 'app-product-card',
  imports: [RouterLink, VndPipe, NgOptimizedImage, IconComponent],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.css'
})
export class ProductCardComponent {
  @Input() productView?: ProductView;

  private readonly authService = inject(AuthService);
  private readonly cartService = inject(CartService);
  private readonly notifyService = inject(NotifyService);
  private readonly router = inject(Router);
  private readonly injector = inject(Injector);

  readonly isAdding = signal(false);

  /** Tải Material Dialog khi cần để không làm nặng bundle ban đầu (thẻ sản phẩm nằm ở trang chủ). */
  private async openLoginModal(): Promise<void> {
    const [{ MatDialog }, { ModalAskLoginComponent }] = await Promise.all([
      import('@angular/material/dialog'),
      import('@store/components/modal-ask-login/modal-ask-login.component'),
    ]);
    this.injector.get(MatDialog).open(ModalAskLoginComponent, {
      enterAnimationDuration: '300ms',
      exitAnimationDuration: '300ms',
    });
  }

  /** Chỉ mua nhanh được khi còn hàng và API đã cho biết màu mặc định. */
  get canQuickBuy(): boolean {
    return !!this.productView?.defaultColorId && !this.isOutOfStock;
  }

  /** Thêm vào giỏ (hoặc mua ngay = thêm rồi chuyển sang giỏ) mà không rời trang danh sách. */
  quickAdd(event: Event, goToCart: boolean): void {
    // Nút nằm trong thẻ <a>: chặn điều hướng sang trang chi tiết
    event.preventDefault();
    event.stopPropagation();
    if (this.isAdding() || !this.canQuickBuy) return;
    if (!this.authService.accessToken) {
      void this.openLoginModal();
      return;
    }
    this.isAdding.set(true);
    this.cartService
      .addToCart({ productId: this.productView!.id, colorId: this.productView!.defaultColorId })
      .pipe(take(1))
      .subscribe({
        next: () => {
          this.isAdding.set(false);
          if (goToCart) {
            this.router.navigate(['/cart']);
          } else {
            this.notifyService.success('Thêm vào giỏ hàng thành công');
          }
        },
        error: () => {
          this.isAdding.set(false);
          this.notifyService.error('Thêm vào giỏ hàng thất bại');
        },
      });
  }

  /** % giảm giá tính từ dữ liệu thật; 0 nghĩa là không có giảm giá. */
  get discountPercent(): number {
    const p = this.productView;
    if (!p || !p.salePrice || !p.basePrice || p.basePrice <= p.salePrice) return 0;
    return Math.round(((p.basePrice - p.salePrice) / p.basePrice) * 100);
  }

  /** Số tiền tiết kiệm được so với giá gốc; 0 nghĩa là không có giảm giá. */
  get savedAmount(): number {
    return this.discountPercent > 0 ? this.productView!.basePrice - this.productView!.salePrice : 0;
  }

  /** Còn dưới ngưỡng này thì báo "Sắp hết hàng". */
  private readonly lowStockThreshold = 5;

  get isLowStock(): boolean {
    const stock = this.productView?.stock;
    return stock != null && stock > 0 && stock <= this.lowStockThreshold;
  }

  get isOutOfStock(): boolean {
    const stock = this.productView?.stock;
    return stock != null && stock <= 0;
  }

  get stockStatus(): { label: string; class: string } | null {
    const stock = this.productView?.stock;
    if (stock === null || stock === undefined) return null;
    if (stock <= 0) return { label: 'Hết hàng', class: 'text-gray-500' };
    if (stock <= this.lowStockThreshold) return { label: `Sắp hết hàng (còn ${stock})`, class: 'text-amber-700' };
    return { label: 'Còn hàng', class: 'text-green-700' };
  }

  get hasReviews(): boolean {
    return (this.productView?.reviewCount ?? 0) > 0 && this.productView?.rating != null;
  }

  readonly stars = [1, 2, 3, 4, 5];
}
