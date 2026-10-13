import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { OrderService } from '../services/order.service';
import { NgClass } from '@angular/common';

import { NotifyService } from '../services/notify.service';
import { FormsModule } from '@angular/forms';
import { HeaderCartOrderComponent } from '../shared/header-cart-order/header-cart-order.component';
import { AuthService } from '../services/auth.service';
import { CartView } from '../core/models/cart_request.model';
import { VndPipe } from '../shared/pipes/vnd.pipe';

@Component({
    selector: 'app-order-review',
    standalone: true,
    imports: [NgClass, HeaderCartOrderComponent, FormsModule, RouterLink, VndPipe],
    templateUrl: './order-review.component.html',
    styleUrl: './order-review.component.css',
})
export class OrderReviewComponent implements OnInit {
    cartDetailIds: number[] = [];
    cartItems: CartView[] = [];
    isLoading = signal<boolean>(false);
    totalQuantity: number = 0;
    totalPrice: number = 0;
    fulfillmentMethod: 'PAY_ON_PICKUP' | 'ONLINE' = 'PAY_ON_PICKUP';
    selectedStore: string | null = null;
    username = signal<string | undefined>("user");

    stores = [
        { id: '1', name: 'Showroom Thái Hà, Hà Nội' },
        { id: '2', name: 'Showroom Nguyễn Trãi, TP.HCM' },
        { id: '3', name: 'Showroom Lê Hồng Phong, Đà Nẵng' }
    ];


    constructor(
        private router: Router,
        private orderService: OrderService,
        private notifyService: NotifyService,
        private authService: AuthService
    ) { }

    ngOnInit() {
        const state = history.state;
        this.username.set(this.authService.currentUser()?.fullName);
        if (state && state.cartDetailIds && state.cartItems) {
            this.cartDetailIds = state.cartDetailIds;
            this.cartItems = state.cartItems;
            this.calculateTotals();
        } else {
            this.router.navigate(['/cart']);
        }
    }

    calculateTotals() {
        if (this.cartItems && this.cartItems.length > 0) {
            this.totalQuantity = this.cartItems.reduce((acc, curr) => acc + (curr.quantity || 0), 0);
            this.totalPrice = this.cartItems.reduce((acc, curr) => acc + ((curr.salePrice || 0) * (curr.quantity || 0)), 0);
        }
    }


    proceed() {
        // if (this.fulfillmentMethod === 'PAY_ON_PICKUP') {
        //     if (!this.selectedStore) {
        //         this.notifyService.error('Vui lòng chọn cửa hàng để nhận hàng!');
        //         return;
        //     }
        //     this.isLoading.set(true);
        //     // Create order and go to success
        //     this.orderService.createOrder(this.cartDetailIds).subscribe({
        //         next: (response) => {
        //             this.notifyService.success('Đặt hàng thành công!');
        //             this.router.navigate(['/checkout'], { state: { orderId: response.data.id, success: true } });
        //         },
        //         error: (err) => {
        //             this.notifyService.error('Có lỗi xảy ra khi tạo đơn hàng');
        //             this.isLoading.set(false);
        //         },
        //         complete: () => {
        //             this.isLoading.set(false);
        //         }
        //     });
        // } else {
        //     // Navigate to Payment Processing
        //     this.router.navigate(['/payment'], { state: { cartDetailIds: this.cartDetailIds, totalPrice: this.totalPrice } });
        // }
    }
}
