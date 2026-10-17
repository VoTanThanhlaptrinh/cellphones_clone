import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { StoreShellComponent } from './layout/store-shell/store-shell.component';

/**
 * Các route của storefront. Trang chủ tải ngay (trang đầu tiên người dùng thấy); các trang còn lại tải theo nhu cầu.
 * `title` được AppTitleStrategy ghép thêm hậu tố thương hiệu; trang sản phẩm/danh mục
 * cập nhật lại tiêu đề theo dữ liệu thật sau khi tải xong.
 *
 * Nhóm đầu nằm trong StoreShellComponent (header + footer); nhóm sau là các trang tối giản không có khung.
 */
export const storeRoutes: Routes = [
    {
        path: '',
        component: StoreShellComponent,
        children: [
            {
                path: 'home',
                component: HomeComponent,
                title: 'Điện thoại, laptop chính hãng'
            },
            {
                path: 'product/:id',
                loadComponent: () => import('./pages/product-detail/product-detail.component').then(m => m.ProductDetailComponent),
                title: 'Chi tiết sản phẩm'
            },
            {
                path: 'category/:slug',
                loadComponent: () => import('./pages/category/category.component').then(m => m.CategoryComponent),
                title: 'Danh mục sản phẩm'
            },
            {
                path: 'member-dashboard',
                loadComponent: () => import('./pages/member-dashboard/member-dashboard.component').then(m => m.MemberDashboardComponent),
                title: 'Tài khoản của tôi'
            },
        ]
    },
    {
        path: 'login',
        loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent),
        title: 'Đăng nhập'
    },
    {
        path: 'register',
        loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent),
        title: 'Đăng ký thành viên'
    },
    {
        path: 'cart',
        loadComponent: () => import('./pages/cart/cart.component').then(m => m.CartComponent),
        title: 'Giỏ hàng'
    },
    {
        path: 'order',
        loadComponent: () => import('./pages/order/order.component').then(m => m.OrderComponent),
        children: [
            {
                path: '',
                loadComponent: () => import('./pages/order/payment-infor/payment-infor.component').then(m => m.PaymentInforComponent),
                title: 'Thông tin đặt hàng'
            },
            {
                path: 'store',
                loadComponent: () => import('./pages/order/store-form/store-form.component').then(m => m.StoreFormComponent),
                title: 'Nhận tại cửa hàng'
            },
            {
                path: 'ship',
                loadComponent: () => import('./pages/order/ship-form/ship-form.component').then(m => m.ShipFormComponent),
                title: 'Giao hàng tận nơi'
            },
            {
                path: 'payment',
                loadComponent: () => import('./pages/order/payment/payment.component').then(m => m.PaymentComponent),
                title: 'Thanh toán'
            },
        ]
    },
    {
        // returnUrl của payment gateway: https://<frontend>/payment/success (đứng trước payment/:orderId)
        path: 'payment/success',
        loadComponent: () => import('./pages/payment-success/payment-success.component').then(m => m.PaymentSuccessComponent),
        title: 'Thanh toán thành công'
    },
    {
        // cancelUrl của payment gateway: https://<frontend>/payment/failed (đứng trước payment/:orderId)
        path: 'payment/failed',
        loadComponent: () => import('./pages/payment-failed/payment-failed.component').then(m => m.PaymentFailedComponent),
        title: 'Thanh toán không thành công'
    },
    {
        // URL cũ của gateway (returnUrl/cancelUrl chung): tự xác minh rồi chuyển sang success/failed
        path: 'payment/return',
        loadComponent: () => import('./pages/payment-return/payment-return.component').then(m => m.PaymentReturnComponent),
        title: 'Xác nhận thanh toán'
    },
    {
        path: 'payment/:orderId',
        loadComponent: () => import('./pages/online-payment/online-payment.component').then(m => m.OnlinePaymentComponent),
        title: 'Thanh toán đơn hàng'
    },
    {
        path: 'checkout',
        loadComponent: () => import('./pages/checkout/checkout.component').then(m => m.CheckoutComponent),
        title: 'Đặt hàng thành công'
    },
];
