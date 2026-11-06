import { Routes } from '@angular/router';
import { CATALOG_RESOURCES } from './catalog/catalog-resources';
import { AdminShellComponent } from './layout/admin-shell/admin-shell.component';

/**
 * Các route của khu quản trị, được nạp lười qua `loadChildren` ở app.routes.ts và chỉ ADMIN vào được (adminGuard).
 * Trang quản lý danh mục sinh từ CATALOG_RESOURCES; trang khác thì thêm route ở đây và một mục trong layout/admin-nav.ts.
 */
export const adminRoutes: Routes = [
    {
        path: '',
        component: AdminShellComponent,
        children: [
            { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
            {
                path: 'dashboard',
                loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.AdminDashboardComponent),
                title: 'Tổng quan quản trị'
            },
            ...CATALOG_RESOURCES.map(resource => ({
                path: resource.key,
                loadComponent: () => import('./catalog/catalog-page/catalog-page.component').then(m => m.CatalogPageComponent),
                data: { resource: resource.key },
                title: `Quản lý ${resource.title.toLowerCase()}`
            })),
            {
                path: 'commitments',
                loadComponent: () => import('./pages/commitments/commitments.component').then(m => m.AdminCommitmentsComponent),
                title: 'Quản lý cam kết sản phẩm'
            },
            {
                path: 'specifications',
                loadComponent: () => import('./pages/specifications/specifications.component').then(m => m.AdminSpecificationsComponent),
                title: 'Quản lý thông số kỹ thuật'
            },
            {
                path: 'images',
                loadComponent: () => import('./pages/images/images.component').then(m => m.AdminImagesComponent),
                title: 'Thư viện hình ảnh'
            },
        ]
    },
];
