import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, inject, Inject, OnInit, PLATFORM_ID, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, distinctUntilChanged, filter, finalize, map, of, switchMap, take, tap } from 'rxjs';

import { ProductService } from '../services/product.service';
import { ProductViewDetail } from '../core/models/product.model';
import { CartService } from '../services/cart.service';
import { NotifyService } from '../services/notify.service';

// === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
import { FormsModule } from '@angular/forms';
import { ReviewService } from '../services/review.service';
import { AuthService } from '../services/auth.service';
import { ReviewResponse } from '../core/models/review.model';
// === END NEW FEATURE: PRODUCT REVIEWS ===
import { VndPipe } from '../shared/pipes/vnd.pipe';
import { MatDialog } from '@angular/material/dialog';
import { ModalAskLoginComponent } from '../shared/modal-ask-login/modal-ask-login.component';

@Component({
  selector: 'app-product-detail',
  imports: [
    CommonModule,
    // === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
    FormsModule,
    // === END NEW FEATURE: PRODUCT REVIEWS ===
    VndPipe,
    RouterLink
  ],
  standalone: true,
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.css'],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class ProductDetailComponent implements OnInit {
  isBrowser: boolean;
  product = signal<ProductViewDetail | null>(null);
  selected_color = signal<number | null>(null);

  // === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
  reviews = signal<ReviewResponse[]>([]);
  averageRating = signal<number>(0);
  newReviewRating = signal<number>(5);
  newReviewComment = signal<string>('');
  private readonly reviewService = inject(ReviewService);
  public readonly authService = inject(AuthService);
  // === END NEW FEATURE: PRODUCT REVIEWS ===

  private readonly fallbackImages = [
    'assets/images/12_5_119.webp',
    'assets/images/14_2_122.webp',
    'assets/images/16_2_118.png'
  ];
  images: string[] = [...this.fallbackImages];
  slides = [
    'assets/images/b2s-pdp-dday2.gif',
    'assets/images/ProductBanner_Voucher-300K_Apple_3.webp',
    'assets/images/iPhone-product-banner-v1.webp'
  ];
  isLoading = false;
  isAdding = signal(false);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  errorMessage = '';
  constructor(
    @Inject(PLATFORM_ID) platformId: Object,
    private readonly route: ActivatedRoute,
    private readonly productService: ProductService,
    private readonly cartService: CartService,
    private readonly notifyService: NotifyService
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit(): void {
    this.route.paramMap
      .pipe(
        map(params => params.get('id')),
        map(id => {
          if (!id) return null;
          const numId = Number(id);
          return Number.isFinite(numId) && numId > 0 ? numId : null;
        }),
        filter((productId): productId is number => productId !== null), // Strict filter to ensure productId is number
        distinctUntilChanged(),
        tap(() => {
          this.isLoading = true;
          this.errorMessage = '';
        }),
        switchMap(productId =>
          this.productService.getProductDetail(productId).pipe(
            map(data => ({ success: true, data, error: null })),
            catchError(error => of({ success: false, data: null, error })), // Handle error in inner pipe to keep outer subscription alive
          )
        ),
      )
      .subscribe(result => {
        this.isLoading = false;
        if (result.success && result.data) {
          const detail = result.data;
          this.product.update(p => p = detail);
          this.selected_color.update(c => detail.colorDTOs?.[0]?.colorId ?? null);
          this.images = this.buildImageGallery(detail);
          // === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
          this.loadReviews(detail.id);
          // === END NEW FEATURE: PRODUCT REVIEWS ===
        } else {
          this.images = [...this.fallbackImages];
          this.errorMessage = 'Không tìm thấy thông tin sản phẩm hoặc có lỗi xảy ra.';
        }
      });
  }

  private buildImageGallery(detail: ProductViewDetail): string[] {
    const gallery = detail.productImages
      ?.map(image => image.originUrl)
      .filter((src): src is string => Boolean(src));

    if (gallery && gallery.length > 0) {
      return gallery;
    }

    if (detail.imageUrl) {
      return [detail.imageUrl];
    }
    return [...this.fallbackImages];
  }

  /** % giảm giá tính từ giá gốc / giá bán; 0 = không giảm. */
  get discountPercent(): number {
    const p = this.product();
    if (!p || !p.salePrice || !p.basePrice || p.basePrice <= p.salePrice) return 0;
    return Math.round(((p.basePrice - p.salePrice) / p.basePrice) * 100);
  }

  /** Tổng tồn kho; null = chưa có dữ liệu kho (không chặn mua). */
  get stockQuantity(): number | null {
    const stores = this.product()?.StoreHouseDTOs;
    if (!stores || stores.length === 0) return null;
    return stores.reduce((sum, s) => sum + (s.quantity || 0), 0);
  }

  get isOutOfStock(): boolean {
    return this.stockQuantity === 0;
  }

  addToCart() {
    this.submitToCart(false);
  }

  buyNow() {
    this.submitToCart(true);
  }

  private submitToCart(goToCart: boolean) {
    if (this.isAdding()) return;
    if (!this.authService.accessToken) {
      this.dialog.open(ModalAskLoginComponent, {
        enterAnimationDuration: '300ms',
        exitAnimationDuration: '300ms',
      });
      return;
    }
    const productId = this.product()?.id;
    const colorId = this.selected_color();
    if (!productId || productId <= 0) return;
    if (!colorId || colorId <= 0) {
      this.notifyService.warning('Vui lòng chọn màu sắc sản phẩm');
      return;
    }
    this.isAdding.set(true);
    this.cartService.addToCart({ productId, colorId }).pipe(take(1)).subscribe({
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
      }
    });
  }

  onRadioChange(event: Event) {
    const value = Number((event.target as HTMLInputElement | null)?.value);
    if (Number.isFinite(value) && value > 0) {
      this.selected_color.set(value);
    }
  }

  scrollToReviews(): void {
    if (!this.isBrowser) return;
    document.getElementById('reviews')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  // === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
  loadReviews(productId: number): void {
    this.reviewService.getReviews(productId).subscribe({
      next: (data) => {
        this.reviews.set(data);
      },
      error: (err) => {
        console.error('Failed to load reviews:', err);
      }
    });

    this.reviewService.getAverageRating(productId).subscribe({
      next: (avg) => {
        this.averageRating.set(avg);
      },
      error: (err) => {
        console.error('Failed to load average rating:', err);
      }
    });
  }

  setRating(rating: number): void {
    this.newReviewRating.set(rating);
  }

  submitReview(): void {
    const prodId = this.product()?.id;
    if (!prodId) {
      this.notifyService.error('Không tìm thấy thông tin sản phẩm');
      return;
    }

    if (this.newReviewRating() < 1 || this.newReviewRating() > 5) {
      this.notifyService.warning('Vui lòng chọn số sao từ 1 đến 5');
      return;
    }

    const comment = this.newReviewComment().trim();
    if (!comment) {
      this.notifyService.warning('Vui lòng nhập nhận xét của bạn');
      return;
    }

    const request = {
      productId: prodId,
      rating: this.newReviewRating(),
      comment: comment
    };

    this.reviewService.createReview(request).subscribe({
      next: (res) => {
        this.notifyService.success('Đánh giá sản phẩm thành công!');
        this.newReviewComment.set('');
        this.newReviewRating.set(5);
        this.loadReviews(prodId);
      },
      error: (err) => {
        console.error('Failed to submit review:', err);
      }
    });
  }

  getReviewsCountByStar(star: number): number {
    return this.reviews().filter(r => r.rating === star).length;
  }
  // === END NEW FEATURE: PRODUCT REVIEWS ===
}
