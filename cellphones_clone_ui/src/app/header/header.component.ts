import { Component, DestroyRef, ElementRef, HostListener, inject, Input, OnInit, signal, ViewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { ProductService } from '../services/product.service';
import { ProductView } from '../core/models/product.model';
import { AuthService } from '@core/auth/auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, of, Subject, switchMap } from 'rxjs';
import { ModalAskLoginComponent } from '../shared/modal-ask-login/modal-ask-login.component';
import { MatDialog } from '@angular/material/dialog';
import { ModalCategoriesComponent } from '../shared/modal-categories/modal-categories.component';
import { VndPipe } from '../shared/pipes/vnd.pipe';
import { IconComponent } from '../shared/icons/icon.component';


@Component({
  selector: 'app-header',
  imports: [RouterLink, CommonModule, FormsModule, VndPipe, IconComponent],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})

export class HeaderComponent implements OnInit {
  keyword: string = '';
  @Input() amountCart: number | undefined = 0;
  private searchSubject = new Subject<string>();
  private destroyRef = inject(DestroyRef);
  isCalled = false;
  useSearch = false;
  menuOpen = false;
  /** Mục kết quả tìm kiếm đang được chọn bằng phím ↑/↓ (-1 = chưa chọn). */
  activeIndex = -1;
  searchResults = signal<ProductView[]>([]);
  isLoggedIn$ = this.authService.isLoggedIn$;
  private dialog = inject(MatDialog);
  constructor(
    private router: Router,
    private productService: ProductService,
    public authService: AuthService,
  ) {
  }

  @ViewChild('searchContainer') searchContainer!: ElementRef;
  @ViewChild('accountContainer') accountContainer!: ElementRef;

  get showDropdown(): boolean {
    return this.useSearch && !!this.keyword.trim();
  }

  logout() {
    this.menuOpen = false;
    this.authService.logout();
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: Event) {
    const target = event.target as Node;
    if (this.searchContainer && !this.searchContainer.nativeElement.contains(target)) {
      this.useSearch = false;
      this.activeIndex = -1;
    }
    if (this.accountContainer && !this.accountContainer.nativeElement.contains(target)) {
      this.menuOpen = false;
    }
  }

  @HostListener('document:keydown.escape')
  onEscape() {
    this.useSearch = false;
    this.activeIndex = -1;
    this.menuOpen = false;
  }

  ngOnInit() {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap((keyword) => {
        if (!keyword.trim()) {
          return of([] as ProductView[]);
        }
        return this.productService.searchProducts(keyword);
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(results => {
      this.searchResults.set(results);
      this.activeIndex = -1;
      this.isCalled = !!this.keyword.trim();
      this.useSearch = true;
    });
  }

  onSearch() {
    this.useSearch = true;
    if (!this.keyword.trim()) {
      this.searchResults.set([]);
      this.isCalled = false;
    }
    this.searchSubject.next(this.keyword);
  }

  /** ↑/↓ chọn kết quả, Enter mở sản phẩm đang chọn, Esc đóng. */
  onSearchKeydown(event: KeyboardEvent) {
    const results = this.searchResults();
    if (event.key === 'ArrowDown' && results.length > 0) {
      event.preventDefault();
      this.useSearch = true;
      this.activeIndex = (this.activeIndex + 1) % results.length;
    } else if (event.key === 'ArrowUp' && results.length > 0) {
      event.preventDefault();
      this.activeIndex = this.activeIndex <= 0 ? results.length - 1 : this.activeIndex - 1;
    } else if (event.key === 'Enter') {
      event.preventDefault();
      const active = results[this.activeIndex];
      if (active) {
        this.router.navigate(['/product', active.id]);
        this.closeSearch();
      }
    }
  }

  closeSearch() {
    this.useSearch = false;
    this.activeIndex = -1;
    this.keyword = '';
    this.searchResults.set([]);
    this.isCalled = false;
  }

  /** Khách chưa đăng nhập bấm giỏ hàng thì hiện modal yêu cầu đăng nhập thay vì chuyển trang. */
  onCartClick(event: Event) {
    if (!this.authService.accessToken) {
      event.preventDefault();
      this.openRequireLoginModal();
    }
  }

  openRequireLoginModal(): void {
    if (this.authService.accessToken) {
      this.router.navigate(['/cart'])
      return;
    }
    this.dialog.open(ModalAskLoginComponent, {
      enterAnimationDuration: '300ms', // Thêm animation mở lên cho mượt
      exitAnimationDuration: '300ms',  // Animation đóng lại
      disableClose: false
    });
  }
  openCategoryModal(): void {
    this.dialog.open(ModalCategoriesComponent, {
      width: '640px',
      maxWidth: '95vw',      // Đảm bảo không bị tràn màn hình ở laptop nhỏ
      panelClass: 'p-0',
      enterAnimationDuration: '300ms', // Thêm animation mở lên cho mượt
      exitAnimationDuration: '300ms',  // Animation đóng lại
      disableClose: false
    });
  }
}
