import { Routes } from '@angular/router';
import { adminGuard } from '@core/auth/admin.guard';
import { storeRoutes } from './store/store.routes';

/** `**` phải đứng cuối cùng; route admin (nếu có) khai báo trước nó. */
export const routes: Routes = [
    {
        path: '',
        redirectTo: 'home',
        pathMatch: 'full'
    },
    ...storeRoutes,
    {
        // Khu quản trị: chỉ ADMIN (adminGuard), tải lười để không làm nặng bundle của storefront.
        path: 'admin',
        canMatch: [adminGuard],
        loadChildren: () => import('./admin/admin.routes').then(m => m.adminRoutes)
    },
    { path: '**', redirectTo: 'home' }
];
