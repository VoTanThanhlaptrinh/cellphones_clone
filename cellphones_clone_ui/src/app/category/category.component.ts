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
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CategoryDetailView, CategoryView } from '../core/models/category_view.model';
import { ProductCardComponent } from "../product-card/product-card.component";
import { ProductView } from '../core/models/product.model';
import { delay } from 'rxjs';
@Component({
  selector: 'app-category',
  imports: [ProductCardComponent],
  templateUrl: './category.component.html',
  styleUrl: './category.component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class CategoryComponent implements OnInit {
  isBrowser = false;
  categoryId: number = 0;
  products: ProductView[] = [];
  page: number = 1;               // Trang hiện tại
  isLoading = false;        // Biến cờ để hiện Skeleton
  hasMore = true;           // Kiểm tra xem còn dữ liệu để tải không
  total = 0;
  data: CategoryDetailView | null = null;
  constructor(@Inject(PLATFORM_ID) platformId: Object, private cdr: ChangeDetectorRef,
    private categoryService: CategoryService,
    private activatedRoute: ActivatedRoute) {
    this.isBrowser = isPlatformBrowser(platformId);
  }
  ngOnInit(): void {
    this.categoryId = Number(this.activatedRoute.snapshot.paramMap.get('id'))
    this.categoryService.GetCategoryDetail(this.categoryId).subscribe({
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
        console.log(reportError)
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
    this.page += 1
    this.categoryService.LoadMoreProduct(this.categoryId, this.page).subscribe({
      next: res => {
        console.log(res)
        if (res && res.length > 0) {
          this.products = [...this.products, ...res];
          this.total = this.total - res.length;
          this.cdr.detectChanges()
        }
      },
      error: e => {
        console.error(e)
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
