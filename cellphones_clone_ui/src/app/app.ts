import {
  ChangeDetectionStrategy,
  Component,
  DOCUMENT,
  EventEmitter,
  inject,
  Input,
  OnInit,
} from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { FooterComponent } from './footer/footer.component';
import { HeaderComponent } from './header/header.component';
import { filter, fromEvent, map, Observable } from 'rxjs';
import { ViewportScroller } from '@angular/common';
import { BackToTopComponent } from './back-to-top/back-to-top.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, FooterComponent, HeaderComponent, BackToTopComponent],
  templateUrl: './app.html',
  styleUrl: './app.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App implements OnInit {
  protected title = 'cellphones-clone-ui';
  isHideHeader = false;
  isHideFooter = false;
  urlForHideHeadFooter: string[] = ['/login', '/register', '/cart', '/order', '/member-dashboard'];

  constructor(
    private router: Router,
    private viewportScroller: ViewportScroller
  ) { }
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
          console.error(err);
        },
      });
  }
  onScrollToTop() {
    console.log('[Parent] received');
    this.viewportScroller.scrollToPosition([0, 0]);
  }
}
