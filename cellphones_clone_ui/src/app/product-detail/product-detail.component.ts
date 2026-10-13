import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, inject, Inject, OnInit, PLATFORM_ID, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
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

@Component({
  selector: 'app-product-detail',
  imports: [
    CommonModule,
    // === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
    FormsModule,
    // === END NEW FEATURE: PRODUCT REVIEWS ===
    VndPipe
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

  get warehouseAddress(): string {
    if (!this.product()?.StoreHouseDTOs || this.product()?.StoreHouseDTOs.length === 0) {
      return '';
    }

    return [this.product()?.StoreHouseDTOs[0].street, this.product()?.StoreHouseDTOs[0].district, this.product()?.StoreHouseDTOs[0].city]
      .filter((value): value is string => Boolean(value))
      .join(', ');
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

  addToCart() {
    var productId = this.product()?.id
    var color_id = this.selected_color()
    if (productId && color_id && productId > 0 && color_id > 0)
      this.cartService.addToCart({ productId: this.product()?.id, colorId: color_id }).pipe(take(1)).subscribe({
        next: res => {
          this.notifyService.success('Thêm vào giỏ hàng thành công');
        },
        error: (err) => {
          this.notifyService.error('Thêm vào giỏ hàng thất bại');
        }
      })
    else
      this.notifyService.warning('Vui lòng chọn màu sắc sản phẩm');
  }
  onRadioChange(event: any) {
    const value = Number(event?.target?.value);
    this.selected_color.update(c => {
      return Number.isFinite(c) ? c : null;
    });
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
