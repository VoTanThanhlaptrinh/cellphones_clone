import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { OrderComponent } from './order/order.component';
import { ProductDetailComponent } from './product-detail/product-detail.component';
import { CategoryComponent } from './category/category.component';
import { CartComponent } from './cart/cart.component';
import { CheckoutComponent } from './checkout/checkout.component';

import { AutocompleteInputComponent } from './shared/custom/autocomplete-input/autocomplete-input.component';
import { MemberDashboardComponent } from './member-dashboard/member-dashboard.component';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'home',
        pathMatch: 'full'
    },
    {
        path: 'home',
        component: HomeComponent
    },
    {
        path: 'login',
        component: LoginComponent
    },
    {
        path: 'register',
        component: RegisterComponent
    },
    {
        path: 'order',
        component: OrderComponent
    },
    {
        path: 'product/:id',
        component: ProductDetailComponent,
    },
    {
        path: 'category/:slug',
        component: CategoryComponent
    },
    {
        path: 'cart',
        component: CartComponent
    },
    {
        path: 'checkout',
        component: CheckoutComponent
    },

    {
        path: 'autocomplete',
        component: AutocompleteInputComponent
    },
    {
        path: 'member-dashboard',
        component: MemberDashboardComponent
    },
    { path: '**', redirectTo: 'home' }
    ,
];
