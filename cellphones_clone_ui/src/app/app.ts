import {
  ChangeDetectionStrategy,
  Component,
  CUSTOM_ELEMENTS_SCHEMA,
  effect,
  OnInit,
} from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { FooterComponent } from './footer/footer.component';
import { HeaderComponent } from './header/header.component';
import { filter } from 'rxjs';
import { ViewportScroller } from '@angular/common';
import { BackToTopComponent } from './back-to-top/back-to-top.component';
import { NotifyService } from './services/notify.service';
import { NgxSpinnerModule, NgxSpinnerService } from 'ngx-spinner';
import { LoadingService } from './core/services/loading.service';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, FooterComponent, HeaderComponent, BackToTopComponent, NgxSpinnerModule],
  templateUrl: './app.html',
  styleUrl: './app.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App implements OnInit {
  protected title = 'cellphones-clone-ui';
  isHideHeader = false;
  isHideFooter = false;
  urlForHideHeadFooter: string[] = ['/login', '/register', '/cart', '/order', '/member-dashboard', '/checkout', '/order-review'];

  constructor(
    private router: Router,
    private viewportScroller: ViewportScroller,
    private notifyService: NotifyService,
    private spinner: NgxSpinnerService,
    private loadingService: LoadingService,
    private authService: AuthService
  ) { 
    effect(() =>{
      if(this.loadingService.isLoading()){
        this.spinner.show();
      }else{
        this.spinner.hide();
      }
    })
  }
  ngOnInit(): void {
    this.isHideHeader = this.isHideFooter = this.urlForHideHeadFooter.includes(
      this.router.url
    );
    this.router.events
      .pipe(filter((e) => e instanceof NavigationEnd))
      .subscribe({
        next: (value) => {
          this.isHideHeader = this.isHideFooter =
            this.urlForHideHeadFooter.includes(value.url);
        },
        error: (err) => {
          this.notifyService.error('Không thể điều hướng trang web');
        },
      });
  }
  onScrollToTop() {
    this.viewportScroller.scrollToPosition([0, 0]);
  }
  onAmountCart() : number | undefined{
    return this.authService.currentUser()?.amountCart;
  }
}
