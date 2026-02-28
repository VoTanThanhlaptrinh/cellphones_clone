import { isPlatformBrowser, NgTemplateOutlet } from '@angular/common';
import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  CUSTOM_ELEMENTS_SCHEMA,
  Inject,
  OnInit,
  PLATFORM_ID,
} from '@angular/core';
import { CategoryService } from '../services/category.service';
import { ActivatedRoute, Route, Router, RouterLink } from '@angular/router';
import { CategoryDetailView, CategoryView } from '../core/models/category_view.model';
import { ProductCardComponent } from "../product-card/product-card.component";
import { ProductView } from '../core/models/product.model';
import { delay } from 'rxjs';
import { NotifyService } from '../services/notify.service';
import { ProductService } from '../services/product.service';
@Component({
  selector: 'app-category',
  imports: [ProductCardComponent],
  templateUrl: './category.component.html',
  styleUrl: './category.component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class CategoryComponent implements OnInit {
  isBrowser = false;
  slugName: string | null = null;
  products: ProductView[] = [];
  page: number = 1;               // Trang hiện tại
  isLoading = false;        // Biến cờ để hiện Skeleton
  hasMore = true;           // Kiểm tra xem còn dữ liệu để tải không
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
    this.slugName = this.activatedRoute.snapshot.paramMap.get('slug')
    if (this.slugName == null) {
      this.router.navigate(['/home'])
      return;
    }
    this.categoryService.GetCategoryDetail(this.slugName).subscribe({
      next: response => {
        delay(2000);
        if (response && response.products) {
          this.data = response
          this.products = [...response.products, ...this.products]
          this.total = response.total - response.products.length
          this.cdr.detectChanges()
        }
      },
      error: reportError => {
        this.notifyService.error('Không thể tải thông tin danh mục');
      }
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
    if (this.slugName == null) {
      this.router.navigate(['/home'])
      return;
    }
    this.productService.LoadMoreProduct('category', this.slugName, this.products[this.products.length - 1].id).subscribe({
      next: res => {
        if (res && res.length > 0) {
          this.products = [...this.products, ...res];
          this.total = this.total - res.length;
          this.cdr.detectChanges()
        }
      },
      error: e => {
        this.notifyService.error('Không thể tải thêm sản phẩm');
        this.hasMore = false
        this.isLoading = false;
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
