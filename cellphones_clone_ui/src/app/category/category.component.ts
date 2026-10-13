import { isPlatformBrowser } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  CUSTOM_ELEMENTS_SCHEMA,
  DestroyRef,
  inject,
  Inject,
  OnInit,
  PLATFORM_ID,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CategoryService } from '../services/category.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CategoryDetailView } from '../core/models/category_view.model';
import { ProductCardComponent } from "../product-card/product-card.component";
import { ProductSkeletonComponent } from '../product-skeleton/product-skeleton.component';
import { ProductView } from '../core/models/product.model';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { NotifyService } from '../services/notify.service';
import { ProductService } from '../services/product.service';
@Component({
  selector: 'app-category',
  imports: [ProductCardComponent, ProductSkeletonComponent, RouterLink],
  templateUrl: './category.component.html',
  styleUrl: './category.component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class CategoryComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  isBrowser = false;
  slugName: string | null = null;
  products: ProductView[] = [];
  page: number = 1;               // Trang hiện tại
  isLoading = false;        // Đang tải thêm sản phẩm
  loadFailed = false;       // Tải danh mục thất bại
  hasMore = false;          // Kiểm tra xem còn dữ liệu để tải không
  total = 0;
  data: CategoryDetailView | null = null;
  constructor(@Inject(PLATFORM_ID) platformId: Object, private cdr: ChangeDetectorRef,
    private categoryService: CategoryService,
    private activatedRoute: ActivatedRoute,
    private notifyService: NotifyService,
    private productService: ProductService,
    private router: Router) {
    this.isBrowser = isPlatformBrowser(platformId);
  }
  ngOnInit(): void {
    // Theo dõi paramMap để đổi danh mục (cùng component) vẫn tải lại dữ liệu.
    this.activatedRoute.paramMap.pipe(
      map(params => params.get('slug')),
      tap(slug => {
        this.slugName = slug;
        this.data = null;
        this.products = [];
        this.total = 0;
        this.hasMore = false;
        this.isLoading = false;
        this.loadFailed = false;
        if (slug == null) {
          this.router.navigate(['/home']);
        }
      }),
      switchMap(slug => slug == null ? of(null) : this.categoryService.GetCategoryDetail(slug).pipe(
        catchError(() => {
          this.loadFailed = true;
          this.notifyService.error('Không thể tải thông tin danh mục');
          return of(null);
        })
      )),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(response => {
      if (response && response.products) {
        this.data = response;
        this.products = [...response.products];
        this.total = Math.max(response.total - response.products.length, 0);
        this.hasMore = this.total > 0;
      }
      this.cdr.detectChanges();
    });
  }
  slides1 = [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:595:100/q:100/plain/https://dashboard.cellphones.com.vn/storage/595x100_iPhone_17_Pro_opensale_v3.png',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:595:100/q:100/plain/https://dashboard.cellphones.com.vn/storage/xiaomi-15t-5g-cate-0925.png',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:595:100/q:100/plain/https://dashboard.cellphones.com.vn/storage/honor-magic-v5-cate.png',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:595:100/q:100/plain/https://dashboard.cellphones.com.vn/storage/honor-400-CATE-1025.png',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:595:100/q:100/plain/https://dashboard.cellphones.com.vn/storage/iphone-16-pro-max-cate-0925.png',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:595:100/q:100/plain/https://dashboard.cellphones.com.vn/storage/595x100_Cate_iPhone_Air_Opensale_v2.png',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:595:100/q:100/plain/https://dashboard.cellphones.com.vn/storage/dienj-thoai-vivo-b2s.png',
  ];
  slides2 = [
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:595:100/q:100/plain/https://dashboard.cellphones.com.vn/storage/dienj-thoai-vivo-b2s.png',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:595:100/q:100/plain/https://dashboard.cellphones.com.vn/storage/595x100_Cate_iPhone_Air_Opensale_v2.png',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:595:100/q:100/plain/https://dashboard.cellphones.com.vn/storage/iphone-16-pro-max-cate-0925.png',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:595:100/q:100/plain/https://dashboard.cellphones.com.vn/storage/honor-400-CATE-1025.png',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:595:100/q:100/plain/https://dashboard.cellphones.com.vn/storage/honor-magic-v5-cate.png',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:595:100/q:100/plain/https://dashboard.cellphones.com.vn/storage/xiaomi-15t-5g-cate-0925.png',
    'https://cdn2.cellphones.com.vn/insecure/rs:fill:595:100/q:100/plain/https://dashboard.cellphones.com.vn/storage/595x100_iPhone_17_Pro_opensale_v3.png',
  ];
  loadMore() {
    if (this.isLoading || this.products.length === 0) return;
    if (this.slugName == null) {
      this.router.navigate(['/home'])
      return;
    }
    this.isLoading = true;
    this.productService.LoadMoreProduct('category', this.slugName, this.products[this.products.length - 1].id).subscribe({
      next: res => {
        if (res && res.length > 0) {
          this.products = [...this.products, ...res];
          this.total = Math.max(this.total - res.length, 0);
        }
      },
      error: e => {
        this.notifyService.error('Không thể tải thêm sản phẩm');
        this.hasMore = false
        this.isLoading = false;
        this.cdr.detectChanges()
      },
      complete: () => {
        this.hasMore = this.total >= 1;
        this.isLoading = false
        this.cdr.detectChanges()
      }
    });
  }
  getRemainingCount() {
    return this.total
  }
}
