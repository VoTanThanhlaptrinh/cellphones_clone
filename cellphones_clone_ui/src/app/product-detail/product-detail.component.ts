import { CommonModule, CurrencyPipe, isPlatformBrowser } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, inject, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { EMPTY, switchMap, take } from 'rxjs';
import { ProductService } from '../services/product.service';
import { ProductViewDetail } from '../core/models/product.model';
import { CartService } from '../services/cart.service';
@Component({
  selector: 'app-product-detail',
  imports: [CommonModule],
  providers: [CurrencyPipe],
  standalone: true,
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class ProductDetailComponent implements OnInit {
  isBrowser: boolean;
  product?: ProductViewDetail;
  selected_color: any;
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
  private currency = inject(CurrencyPipe);
  constructor(
    @Inject(PLATFORM_ID) platformId: Object,
    private readonly route: ActivatedRoute,
    private readonly productService: ProductService,
    private readonly cartService: CartService
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
          this.selected_color = this.product.colorDTOs[0].colorId
          this.images = this.buildImageGallery(detail);
        },
        error: () => {
          this.isLoading = false;
          this.errorMessage = 'Đã xảy ra lỗi khi tải dữ liệu sản phẩm.';
        }
      });
  }

  get warehouseAddress(): string {
    if (!this.product?.StoreHouseDTOs) {
      return '';
    }

    return [this.product.StoreHouseDTOs[0].street, this.product.StoreHouseDTOs[0].district, this.product.StoreHouseDTOs[0].city]
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
    var productId = this.product?.id
    var color_id = this.selected_color
    if (productId && color_id && productId > 0 && color_id > 0)
      this.cartService.addToCart({ productId: this.product?.id, colorId: color_id }).pipe(take(1)).subscribe({
        next: res =>{
          console.log('success')
        },
        error: (err) => console.error(err)
      })
    else
      console.log('Dữ liệu có vấn đề')
  }
  convertPriceVnd(price: number | null | undefined): string {
    if (price == null) return 'Đang cập nhật';
    const formatted = this.currency.transform(price, 'VND', 'symbol', '1.0-0') ?? '';
    return formatted.replace('₫', 'đ');
  }
  onRadioChange(event: any) {
    const input = event.target.value
    console.log('selected value =', input);
  }
}
