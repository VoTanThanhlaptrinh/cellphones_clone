import { isPlatformBrowser } from '@angular/common';
import {
  Component,
  CUSTOM_ELEMENTS_SCHEMA,
  Inject,
  PLATFORM_ID,
} from '@angular/core';

@Component({
  selector: 'app-category',
  imports: [],
  templateUrl: './category.component.html',
  styleUrl: './category.component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class CategoryComponent {
  isBrowser = false;
  constructor(@Inject(PLATFORM_ID) platformId: Object) {
    this.isBrowser = isPlatformBrowser(platformId);
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
  category_list = [
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/tmp/catalog/product/f/r/frame_59.png',
      alt: 'Điện thoại Apple',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/tmp/catalog/product/f/r/frame_60.png',
      alt: 'Điện thoại Samsung',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/tmp/catalog/product/f/r/frame_61.png',
      alt: 'Điện thoại Xiaomi',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/tmp/catalog/product/f/r/frame_62.png',
      alt: 'Điện thoại OPPO',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/tmp/catalog/product/f/r/frame_69_1_.png',
      alt: 'Điện thoại Tecno',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/wysiwyg/HONOR.png',
      alt: 'HONOR',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/wysiwyg/nubia_1.png',
      alt: 'Điện thoại Nubia',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/catalog/product/b/r/brand-icon-sony_2.png',
      alt: 'Điện thoại Sony',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/tmp/catalog/product/f/r/frame_37_1.png',
      alt: 'Điện thoại Nokia',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/tmp/catalog/product/i/n/infinixlogo.png',
      alt: 'Điện thoại Infinix',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/wysiwyg/nothing-phone.png',
      alt: 'Điện thoại Nothing Phone',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/wysiwyg/masstel-mobile-logo022.png',
      alt: 'Điện thoại Masstel',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/tmp/catalog/product/f/r/frame_63.png',
      alt: 'Điện thoại realme',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/wysiwyg/logo-itel-11.png',
      alt: 'Điện thoại Itel',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/tmp/catalog/product/t/_/t_i_xu_ng_67_.png',
      alt: 'Điện thoại vivo',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/tmp/catalog/product/f/r/frame_65.png',
      alt: 'Điện thoại OnePlus',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/tmp/catalog/product/t/i/tivi-logo-cate.png',
      alt: 'Điện thoại TCL',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/wysiwyg/iinoi-mobile-logo022.png',
      alt: 'Điện thoại INOI',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/wysiwyg/Icon/logo-benco-icon-cate-menu.png',
      alt: 'Điện thoại Benco',
    },
    {
      src: 'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:50/q:30/plain/https://cellphones.com.vn/media/tmp/catalog/product/f/r/frame_67.png',
      alt: 'Điện thoại ASUS',
    },
  ];
  phoneCategories = [
  {
    "src": "https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/dien-thoai-gaming-icon-cate.png",
    "alt": "Điện thoại chơi game",
    "text": "Điện thoại chơi game"
  },
  {
    "src": "https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/dien-thoai-pin-trau-icon-cate.png",
    "alt": "Điện thoại pin trâu",
    "text": "Điện thoại pin trâu"
  },
  {
    "src": "https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/dien-thoai-5g-icon-cate.png",
    "alt": "Điện thoại 5G",
    "text": "Điện thoại 5G"
  },
  {
    "src": "https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/dien-thoai-chup-anh-dep-icon-cate.png",
    "alt": "Điện thoại chụp ảnh đẹp",
    "text": "Điện thoại chụp ảnh đẹp"
  },
  {
    "src": "https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/dien-thoai-gap-icon-cate.png",
    "alt": "Điện thoại gập",
    "text": "Điện thoại gập"
  },
  {
    "src": "https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/dien-thoai-ai-icon-cate.png",
    "alt": "Điện thoại AI",
    "text": "Điện thoại AI"
  },
  {
    "src": "https://cdn2.cellphones.com.vn/insecure/rs:fill:150:0/q:70/plain/https://cellphones.com.vn/media/wysiwyg/dien-thoai-pho-thong-icon-cate.png",
    "alt": "Điện thoại phổ thông",
    "text": "Điện thoại phổ thông"
  }
];
}
