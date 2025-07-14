import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';
import { SlickCarouselModule } from 'ngx-slick-carousel';
import { dot } from 'node:test/reporters';
@Component({
  selector: 'app-home',
  imports: [MatTabsModule, SlickCarouselModule, CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  slides = [
    { img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/m/a/macbook_29_.png' },
    { img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/r/o/robot-hut-bui-xiaomi-x20-pro_2_.png' },
    { img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/m/smart-keyboard-fot-ipad-pro.jpg' },
    { img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/m/a/macbook_29_.png' },
    { img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/r/o/robot-hut-bui-xiaomi-x20-pro_2_.png' },
    { img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/m/smart-keyboard-fot-ipad-pro.jpg' },
    { img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/m/a/macbook_29_.png' },
    { img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/r/o/robot-hut-bui-xiaomi-x20-pro_2_.png' },
    { img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/m/smart-keyboard-fot-ipad-pro.jpg' }
  ];

  slideConfig = {
    mobileFirst: true,
    slidesToShow: 5,
    slidesToScroll: 1,
    autoplay: true,
    autoplaySpeed: 5000,
    pauseOnHover: true,
    infinite: true,
    arrows: true,
    responsive: [
      { breakpoint: 300, settings: { arrows: true, slidesToScroll: 1, slidesToShow: 2, infinite: true } },
      { breakpoint: 768, settings: { arrows: true, slidesToScroll: 1, slidesToShow: 2, infinite: true } },
      { breakpoint: 1125, settings: { arrows: true, slidesToScroll: 1, slidesToShow: 5, infinite: true } }
    ],
  };

}
