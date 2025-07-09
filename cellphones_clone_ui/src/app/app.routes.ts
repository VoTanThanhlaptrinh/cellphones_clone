import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { OrderComponent } from './order/order.component';
import { ProductDetailComponent } from './product-detail/product-detail.component';
import { CategoryComponent } from './category/category.component';
import { CartComponent } from './cart/cart.component';
import { CheckoutComponent } from './checkout/checkout.component';
import { RenderMode } from '@angular/ssr';

export const routes: Routes = [
    {
        path:'',
        component:HomeComponent
    },
        {
        path:'login',
        component:LoginComponent
    },
        {
        path:'register',
        component:RegisterComponent
    },
        {
        path:'order',
        component:OrderComponent
    },
        {
        path:'product/:id',
        component:ProductDetailComponent,
    
    },
        {
        path:'category/:id',
        component:CategoryComponent
    },
        {
        path:'cart',
        component:CartComponent
    },
        {
        path:'checkout',
        component:CheckoutComponent
    },
];
