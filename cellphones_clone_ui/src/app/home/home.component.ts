import { CommonModule, isPlatformBrowser } from '@angular/common';
import {
  Component,
  CUSTOM_ELEMENTS_SCHEMA,
  Inject,
  OnInit,
  PLATFORM_ID,
  NgModule,
  signal
} from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';
import { RouterModule } from '@angular/router';
import { ProductService } from '../services/product.service';
import { HomeView } from '../core/models/home_view.model';
import { ProductCardComponent } from "../product-card/product-card.component";
@Component({
  selector: 'app-home',
  imports: [
    MatTabsModule,
    CommonModule,
    RouterModule,
    ProductCardComponent
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class HomeComponent implements OnInit {
  isBrowser = signal(false);
  public home_view = this.productService.homeData;
  protected show = false
  protected i = 0;
  constructor(@Inject(PLATFORM_ID) platformId: Object, private productService: ProductService) {
    this.isBrowser.set(isPlatformBrowser(platformId));
  }

  slides = [
    {
      img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/m/a/macbook_29_.png',
    },
    {
      img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/r/o/robot-hut-bui-xiaomi-x20-pro_2_.png',
    },
    {
      img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/m/smart-keyboard-fot-ipad-pro.jpg',
    },
    {
      img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/m/a/macbook_29_.png',
    },
    {
      img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/r/o/robot-hut-bui-xiaomi-x20-pro_2_.png',
    },
    {
      img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/m/smart-keyboard-fot-ipad-pro.jpg',
    },
    {
      img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/m/a/macbook_29_.png',
    },
    {
      img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/r/o/robot-hut-bui-xiaomi-x20-pro_2_.png',
    },
    {
      img: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/m/smart-keyboard-fot-ipad-pro.jpg',
    },
  ];
  breakpoints1 = {
    0: {
      slidesPerView: 2,
      grid: { rows: 2, fill: 'row' },
      spaceBetween: 8,
    },
    425: {
      slidesPerView: 2,
      grid: { rows: 2, fill: 'row' },
      spaceBetween: 8,
    },
    768: {
      slidesPerView: 3,
      grid: { rows: 2, fill: 'row' },
      spaceBetween: 8,
    },
    1024: {
      slidesPerView: 4,
      grid: { rows: 2, fill: 'row' },
      spaceBetween: 8,
    },
  };
  breakpoints = {
    0: {
      slidesPerView: 2,
    },
    425: {
      slidesPerView: 2,
      spaceBetween: 10
    },
    768: {
      slidesPerView: 4,
      spaceBetween: 10
    },
    1024: {
      slidesPerView: 6,
      spaceBetween: 15
    },
  };
  breakpoints2 = {
    0: {
      slidesPerView: 'auto',
      spaceBetween: 10,
    }
  };
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
      {
        breakpoint: 300,
        settings: {
          arrows: true,
          slidesToScroll: 1,
          slidesToShow: 2,
          infinite: true,
        },
      },
      {
        breakpoint: 768,
        settings: {
          arrows: true,
          slidesToScroll: 1,
          slidesToShow: 3,
          infinite: true,
        },
      },
      {
        breakpoint: 1125,
        settings: {
          arrows: true,
          slidesToScroll: 1,
          slidesToShow: 5,
          infinite: true,
        },
      },
    ],
  };
  list_phu = [
    {
      name_title: 'PHỤ KIỆN',
      list_phukien: [
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/pk-apple-tai-nghe.png',
          name: 'Phụ kiện Apple',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/pk-apple-cap-sac.png',
          name: 'Cáp, sạc',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/pin-du-phong-20000-mah.png',
          name: 'Pin sạc dự phòng',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/op-bao-da-sam-sung-s24.png',
          name: 'Ốp lưng - Bao da',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/dan-man-hinh-iphone-15.png',
          name: 'Dán màn hình',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/the-nho-usb-otg-the-nho-usb.png',
          name: 'Thẻ nhớ, USB',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/gaming-gear-play-staytion.png',
          name: 'Gaming Gear, Playstation',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/sim-sim-4g.png',
          name: 'Sim 4G',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/thiet-bi-phat-song-wifi-router-wifi.png',
          name: 'Thiết bị mạng',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/camera-hanh-trinh-trong-nha.png',
          name: 'Camera',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/camera-gimbal.png',
          name: 'Gimbal',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/camera-flycam.png',
          name: 'Flycam',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/camera-may-anh.png',
          name: 'Máy ảnh',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/phu-kien-ban-phim-chuot.png',
          name: 'Chuột, bàn phím',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/balo-tui-chong-soc-laptop-17inch.png',
          name: 'Balo, túi xách',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/cap-sac-hub.png',
          name: 'Hub chuyển đổi',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/pk-phu-kien-dien-thoai.png',
          name: 'Phụ kiện điện thoại',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/pk-may-tinh-laptop-camera.png',
          name: 'Phụ kiện Laptop',
        },
      ],
    },
    {
      name_title: 'LINH KIỆN MÁY TÍNH',
      list_phukien: [
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/Untitled_2.png',
          name: 'PC ráp sẵn CellphoneS',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/image_1680.png',
          name: 'CPU',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/image_2__2.png',
          name: 'Mainboard',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/image_3__2.png',
          name: 'RAM',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/image_4__2.png',
          name: 'Ổ cứng',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/image_5__1.png',
          name: 'Card màn hình',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/image_6_.png',
          name: 'Nguồn máy tính',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/image_7_.png',
          name: 'Tản nhiệt',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/image_8_.png',
          name: 'Case máy tính',
        },
      ],
    },
    {
      name_title: 'HÀNG CŨ',
      list_phukien: [
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/catalog/product/i/p/ip-14-hp-cu.png',
          name: 'Điện thoại cũ',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/catalog/product/i/p/ipad-cate-cu.png',
          name: 'Máy tính bảng cũ',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/icons/category/cate-392.svg',
          name: 'Mac cũ',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/icons/category/cate-878.svg',
          name: 'Laptop cũ',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/catalog/product/c/a/cate-tai-nghe_1.png',
          name: 'Tai nghe cũ',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/catalog/product/c/a/cate-loa_1.png',
          name: 'Loa cũ',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/icons/category/cate-451.svg',
          name: 'Đồng hồ thông minh cũ',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/icons/category/cate-846.svg',
          name: 'Đồ gia dụng cũ',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/catalog/product/c/a/cate-phu-kien.png',
          name: 'Phụ kiện cũ',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/catalog/product/c/a/cate-man-hinh.png',
          name: 'Màn hình cũ',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/tivi-man-hinh-lon.png',
          name: 'Tivi cũ',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/icons/category/cate-114.svg',
          name: 'Cáp sạc cũ',
        },
        {
          src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/icons/category/cate-122.svg',
          name: 'Pin dự phòng cũ',
        },
      ],
    },
  ];

  ngOnInit(): void {
    this.productService.fetchHomeInit();
  }
}
