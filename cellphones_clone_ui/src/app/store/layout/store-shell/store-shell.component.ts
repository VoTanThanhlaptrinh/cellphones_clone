import { ViewportScroller } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from '@core/auth/auth.service';
import { BackToTopComponent } from '../back-to-top/back-to-top.component';
import { FooterComponent } from '../footer/footer.component';
import { HeaderComponent } from '../header/header.component';

/** Khung của các trang mua sắm có header và footer. Trang tối giản (login, cart, order…) nằm ngoài khung này. */
@Component({
  selector: 'app-store-shell',
  imports: [RouterOutlet, HeaderComponent, FooterComponent, BackToTopComponent],
  templateUrl: './store-shell.component.html',
})
export class StoreShellComponent {
  private readonly viewportScroller = inject(ViewportScroller);
  private readonly authService = inject(AuthService);

  readonly amountCart = computed(() => this.authService.currentUser()?.amountCart);

  scrollToTop() {
    this.viewportScroller.scrollToPosition([0, 0]);
  }
}
