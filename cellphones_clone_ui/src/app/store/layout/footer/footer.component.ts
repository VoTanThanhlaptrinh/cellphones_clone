import { Component } from '@angular/core';

interface PaymentMethod {
  name: string;
  href: string;
  img: string;
}

@Component({
  selector: 'app-footer',
  imports: [],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.css'
})
export class FooterComponent {
  readonly paymentMethods: PaymentMethod[] = [
    { name: 'Apple Pay', href: 'https://cellphones.com.vn/sforum/apple-pay-viet-nam', img: 'https://cdn2.cellphones.com.vn/x35,webp/media/wysiwyg/apple-pay-og.png' },
    { name: 'VNPay', href: 'https://cellphones.com.vn/sforum/vnpay-la-gi-cach-dang-ky-vnpay-thanh-toan-vnpay-chi-tiet', img: 'https://cdn2.cellphones.com.vn/x35,webp/media/logo/payment/vnpay-logo.png' },
    { name: 'MoMo', href: 'https://cellphones.com.vn/huong-dan-thanh-toan-qua-vi-momo-cellphones', img: 'https://cdn2.cellphones.com.vn/x/media/wysiwyg/momo_1.png' },
    { name: 'OnePay', href: 'https://cellphones.com.vn/huong-dan-mua-hang-va-thanh-toan-qua-cong-onepay', img: 'https://cdn2.cellphones.com.vn/x35,webp/media/logo/payment/onepay-logo.png' },
    { name: 'Mpos', href: 'https://cellphones.com.vn/huong-dan-mua-hang-online', img: 'https://cdn2.cellphones.com.vn/x35,webp/media/logo/payment/mpos-logo.png' },
    { name: 'Kredivo', href: 'https://cellphones.com.vn/uu-dai-doi-tac/kredivo', img: 'https://cdn2.cellphones.com.vn/x35,webp/media/logo/payment/kredivo-logo.png' },
    { name: 'ZaloPay', href: 'https://cellphones.com.vn/sforum/huong-dan-toan-bang-zalopay-khi-mua-hang-tren-website-cellphones', img: 'https://cdn2.cellphones.com.vn/x35,webp/media/logo/payment/zalopay-logo.png' },
    { name: 'Alepay', href: 'https://cellphones.com.vn/huong-dan-mua-hang-online', img: 'https://cdn2.cellphones.com.vn/x35,webp/media/logo/payment/alepay-logo.png' },
    { name: 'Fundiin', href: 'https://cellphones.com.vn/huong-dan-thanh-toan-qua-cong-fundiin-tren-website-cellphones', img: 'https://cdn2.cellphones.com.vn/x/media/wysiwyg/fundiin.png' },
  ];

  readonly policies = [
    'Mua hàng và thanh toán Online',
    'Mua hàng trả góp Online',
    'Mua hàng trả góp bằng thẻ tín dụng',
    'Chính sách giao hàng',
    'Chính sách đổi trả',
    'Tra thông tin bảo hành',
    'Trung tâm bảo hành chính hãng',
    'Chính sách khui hộp sản phẩm Apple',
  ];

  readonly services = [
    'Khách hàng doanh nghiệp (B2B)',
    'Ưu đãi thanh toán',
    'Quy chế hoạt động',
    'Chính sách bảo mật thông tin cá nhân',
    'Chính sách bảo hành',
    'Liên hệ hợp tác kinh doanh',
    'Tuyển dụng',
    'Dịch vụ bảo hành mở rộng',
  ];
}
