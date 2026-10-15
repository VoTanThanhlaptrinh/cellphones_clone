import { Routes } from '@angular/router';
import { AdminShellComponent } from './layout/admin-shell/admin-shell.component';

/**
 * Các route của khu quản trị, được nạp lười qua `loadChildren` ở app.routes.ts và chỉ ADMIN vào được (adminGuard).
 * Trang mới: thêm route ở đây và một mục trong layout/admin-nav.ts.
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
        ]
    },
];
