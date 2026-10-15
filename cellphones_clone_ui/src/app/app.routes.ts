import { Routes } from '@angular/router';
import { HomeComponent } from './store/pages/home/home.component';

/**
 * Trang chủ tải ngay (trang đầu tiên người dùng thấy); các trang còn lại tải theo nhu cầu.
 * `title` được AppTitleStrategy ghép thêm hậu tố thương hiệu; trang sản phẩm/danh mục
 * cập nhật lại tiêu đề theo dữ liệu thật sau khi tải xong.
 */
export const routes: Routes = [
    {
        path: '',
        redirectTo: 'home',
        pathMatch: 'full'
    },
    {
        path: 'home',
        component: HomeComponent,
        title: 'Điện thoại, laptop chính hãng'
    },
    {
        path: 'login',
        loadComponent: () => import('./store/pages/login/login.component').then(m => m.LoginComponent),
        title: 'Đăng nhập'
    },
    {
        path: 'register',
        loadComponent: () => import('./store/pages/register/register.component').then(m => m.RegisterComponent),
        title: 'Đăng ký thành viên'
    },
    {
        path: 'order',
        loadComponent: () => import('./store/pages/order/order.component').then(m => m.OrderComponent),
        children: [
            {
                path: '',
                loadComponent: () => import('./store/pages/order/payment-infor/payment-infor.component').then(m => m.PaymentInforComponent),
                title: 'Thông tin đặt hàng'
            },
            {
                path: 'store',
                loadComponent: () => import('./store/pages/order/store-form/store-form.component').then(m => m.StoreFormComponent),
                title: 'Nhận tại cửa hàng'
            },
            {
                path: 'ship',
                loadComponent: () => import('./store/pages/order/ship-form/ship-form.component').then(m => m.ShipFormComponent),
                title: 'Giao hàng tận nơi'
            },
            {
                path: 'payment',
                loadComponent: () => import('./store/pages/order/payment/payment.component').then(m => m.PaymentComponent),
                title: 'Thanh toán'
            },
        ]
    },
    {
        path: 'product/:id',
        loadComponent: () => import('./store/pages/product-detail/product-detail.component').then(m => m.ProductDetailComponent),
        title: 'Chi tiết sản phẩm'
    },
    {
        path: 'category/:slug',
        loadComponent: () => import('./store/pages/category/category.component').then(m => m.CategoryComponent),
        title: 'Danh mục sản phẩm'
    },
    {
        path: 'cart',
        loadComponent: () => import('./store/pages/cart/cart.component').then(m => m.CartComponent),
        title: 'Giỏ hàng'
    },
    {
        path: 'checkout',
        loadComponent: () => import('./store/pages/checkout/checkout.component').then(m => m.CheckoutComponent),
        title: 'Đặt hàng thành công'
    },
    {
        path: 'member-dashboard',
        loadComponent: () => import('./store/pages/member-dashboard/member-dashboard.component').then(m => m.MemberDashboardComponent),
        title: 'Tài khoản của tôi'
    },
    { path: '**', redirectTo: 'home' }
];
