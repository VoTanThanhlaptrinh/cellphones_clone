import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EMPTY, switchMap, take } from 'rxjs';
import { ProductService } from '../services/product.service';
import { ProductViewDetail } from '../core/models/product.model';
@Component({
  selector: 'app-product-detail',
  imports: [CommonModule],
  standalone: true,
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class ProductDetailComponent implements OnInit {
  isBrowser: boolean;
  product?: ProductViewDetail;
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
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit(): void {
    this.route.paramMap
      .pipe(
        take(1),
        switchMap(params => {
          const idParam = params.get('id');
          const productId = Number(idParam);

          if (!productId) {
            this.errorMessage = 'Không tìm thấy sản phẩm.';
            this.isLoading = false;
            return EMPTY;
          }

          this.isLoading = true;
          this.errorMessage = '';
          return this.productService.getProductDetail(productId);
        })
      )
      .subscribe({
        next: detail => {
          this.isLoading = false;
          if (!detail) {
            this.product = undefined;
            this.images = [...this.fallbackImages];
            this.errorMessage = 'Không tìm thấy thông tin sản phẩm.';
            return;
          }

          this.product = detail;
          this.images = this.buildImageGallery(detail);
        },
        error: () => {
          this.isLoading = false;
          this.errorMessage = 'Đã xảy ra lỗi khi tải dữ liệu sản phẩm.';
        }
      });
  }

  get warehouseAddress(): string {
    if (!this.product) {
      return '';
    }

    return [this.product.street, this.product.district, this.product.city]
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
}
