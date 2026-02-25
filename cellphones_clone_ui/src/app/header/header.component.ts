import { AfterViewInit, Component, ElementRef, HostListener, inject, Inject, NgModule, OnInit, ViewChild } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ProductService } from '../services/product.service';
import { ProductView } from '../core/models/product.model';
import { AuthService } from '../services/auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, Subscription, switchMap } from 'rxjs';
import { ModalAskLoginComponent } from '../shared/modal-ask-login/modal-ask-login.component';
import { MatDialog, MatDialogContent, MatDialogModule } from '@angular/material/dialog';
import { ModalCategoriesComponent } from '../shared/modal-categories/modal-categories.component';


@Component({
  selector: 'app-header',
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})

export class HeaderComponent implements OnInit {
  keyword: string = '';
  private searchSubject = new Subject<string>();
  private searchSubscription?: Subscription;
  isCalled = false;
  showTab = false;
  i = 0;
  useSearch = false;
  searchResults: ProductView[] = [];


  isLoggedIn$ = this.authService.isLoggedIn$;
  private dialog = inject(MatDialog);
  constructor(
    private router: Router,
    private productService: ProductService,
    public authService: AuthService,
  ) {
  }

  logout() {
    this.authService.logout();
  }

  goHome() {
    this.router.navigate(['/home'])
  }
  @ViewChild('searchContainer') searchContainer!: ElementRef;

  @HostListener('document:click', ['$event'])
  onClickOutside(event: Event) {
    if (this.searchContainer) {
      const clickedInside = this.searchContainer.nativeElement.contains(event.target);
      if (!clickedInside) {
        this.useSearch = false;
      }
    }
  }
  ngOnInit() {
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap((keyword) => {
        if (!keyword.trim()) {
          return [];
        }
        return this.productService.searchProducts(keyword);
      })
    ).subscribe(results => {
      this.searchResults = results;
      this.isCalled = true;
      this.useSearch = true;
    });
  }
  onSearch() {
    this.useSearch = true;
    this.searchSubject.next(this.keyword);
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
      width: '950px',        // Mở rộng kích thước tổng
      maxWidth: '95vw',      // Đảm bảo không bị tràn màn hình ở laptop nhỏ
      panelClass: 'p-0',
      enterAnimationDuration: '300ms', // Thêm animation mở lên cho mượt
      exitAnimationDuration: '300ms',  // Animation đóng lại
      disableClose: false
    });
  }
}