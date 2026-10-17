import { Component, inject, OnInit } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { InputComponent } from "@shared/form/input/input.component";
import { OrderService } from '@store/services/order.service';
import { CartView } from '@core/models/cart_request.model';
import { VndPipe } from '@shared/pipes/vnd.pipe';
import { AuthService } from '@core/auth/auth.service';
import { IconComponent } from '@shared/ui/icon/icon.component';
import { OrderStepHeaderComponent } from '@store/components/order-step-header/order-step-header.component';
import { OrderBottomBarComponent } from '@store/components/order-bottom-bar/order-bottom-bar.component';

/** Trang 1 của luồng đặt hàng: tóm tắt giỏ hàng, email và chọn hình thức nhận hàng. */
@Component({
  selector: 'app-payment-infor',
  imports: [ReactiveFormsModule, InputComponent, VndPipe, IconComponent, OrderStepHeaderComponent, OrderBottomBarComponent],
  templateUrl: './payment-infor.component.html',
  styleUrl: './payment-infor.component.css',
})
export class PaymentInforComponent implements OnInit {
  private router = inject(Router);
  private orderService = inject(OrderService);
  private authService = inject(AuthService);

  readonly paymentForm = this.orderService.paymentForm;
  cartDetails: CartView[] = [];
  readonly userName = this.authService.currentUser()?.fullName ?? '';

  get emailError(): string {
    const control = this.paymentForm.get('email');
    return control && control.touched && control.invalid ? 'Vui lòng nhập email hợp lệ' : '';
  }

  get total(): number {
    return this.orderService.totalOf(this.cartDetails);
  }

  ngOnInit(): void {
    const state = history.state;
    if (state && state.cartDetails) {
      this.cartDetails = state.cartDetails;
    } else {
      this.router.navigate(['/home']);
    }
  }

  deleteMail() {
    this.paymentForm.patchValue({ email: '' });
  }

  /** Sang trang nhập địa chỉ tương ứng với hình thức nhận hàng đã chọn. */
  goNext() {
    const email = this.paymentForm.get('email');
    if (email?.invalid) {
      email.markAsTouched();
      return;
    }
    const page = this.paymentForm.get('deliveryMethod')?.value === 'at-store' ? 'store' : 'ship';
    this.router.navigate(['/order', page], { state: { cartDetails: this.cartDetails } });
  }

  goCart() {
    this.router.navigate(['/cart']);
  }
}
