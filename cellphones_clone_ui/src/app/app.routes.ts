import { Routes } from '@angular/router';
import { storeRoutes } from './store/store.routes';

/** `**` phải đứng cuối cùng; route admin (nếu có) khai báo trước nó. */
export const routes: Routes = [
    {
        path: '',
        redirectTo: 'home',
        pathMatch: 'full'
    },
    ...storeRoutes,
    { path: '**', redirectTo: 'home' }
];
