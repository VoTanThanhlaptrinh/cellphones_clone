import { CommonModule, isPlatformBrowser } from '@angular/common';
import { AfterViewInit, Component, CUSTOM_ELEMENTS_SCHEMA, Inject, PLATFORM_ID } from '@angular/core';
@Component({
  selector: 'app-product-detail',
  imports: [CommonModule],
  standalone: true,
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class ProductDetailComponent  {
  isBrowser: boolean;
  constructor(@Inject(PLATFORM_ID) platformId: Object) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  images = ['assets/images/12_5_119.webp', 'assets/images/14_2_122.webp', 'assets/images/16_2_118.png'];
  slides = ['assets/images/b2s-pdp-dday2.gif',
    'assets/images/ProductBanner_Voucher-300K_Apple_3.webp',
    'assets/images/iPhone-product-banner-v1.webp'
  ]

}
