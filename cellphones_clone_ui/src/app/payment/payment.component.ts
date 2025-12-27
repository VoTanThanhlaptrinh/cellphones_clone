import { Component, EventEmitter, inject, Input, Output, OnInit, computed } from '@angular/core';
import { UserView } from '../core/models/user_response.model';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormGroup, FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { CheckoutService } from '../services/checkout.service';

import { InputComponent } from "../shared/custom/input/input.component";

@Component({
  selector: 'app-payment',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.css',
})
export class PaymentComponent implements OnInit {
  @Input() checkoutForm!: FormGroup;
  @Output() isPaymentInfor = new EventEmitter<boolean>();

  paymentForm: FormGroup;

  private fb = inject(FormBuilder);
  private currency = inject(CurrencyPipe);
  private checkoutService = inject(CheckoutService);

  paymentInfo = computed(() => this.checkoutService.checkoutState().paymentInfo);

  user?: UserView = {
    name: "Võ Tấn Thành", mobile: "0796692184", email: "votanthanh32004@gmail.com"
  };

  constructor() {
    this.paymentForm = this.fb.group({
      coupon: [''],
      paymentMethod: ['cod', Validators.required]
    });
  }

  ngOnInit(): void {
    // Sync with checkout service if needed
  }

  convertPriceVnd(price: number | null | undefined): string {
    if (price == null) return 'Đang cập nhật';
    const formatted = this.currency.transform(price, 'VND', 'symbol', '1.0-0') ?? '';
    return formatted.replace('₫', 'đ');
  }

  deleteMail() {
    this.user!.email = "";
  }

  goInfor(event: any) {
    this.isPaymentInfor.emit(true);
  }

  onPayment() {
    if (this.paymentForm.valid) {
      this.checkoutService.updatePaymentMethod(this.paymentForm.value.paymentMethod);
      if (this.paymentForm.value.coupon) {
        this.checkoutService.updateCoupon(this.paymentForm.value.coupon);
      }
      console.log('Payment Submit:', this.paymentForm.value);
      // Proceed to backend or next step
    } else {
      this.paymentForm.markAllAsTouched();
    }
  }
}
