import { Injectable, signal } from '@angular/core';

export interface PaymentInfo {
  name: string;
  phone: string;
  email: string;
  address: string;
  city?: string;
  district?: string;
  note?: string;
  deliveryMethod: string;
}

export interface CheckoutState {
  paymentMethod: string;
  couponCode: string;
  paymentInfo: PaymentInfo | null;
}

@Injectable({
  providedIn: 'root'
})
export class CheckoutService {
  checkoutState = signal<CheckoutState>({
    paymentMethod: '',
    couponCode: '',
    paymentInfo: null
  });

  updatePaymentMethod(method: string) {
    this.checkoutState.update(state => ({ ...state, paymentMethod: method }));
  }

  updateCoupon(code: string) {
    this.checkoutState.update(state => ({ ...state, couponCode: code }));
  }

  updatePaymentInfo(info: PaymentInfo) {
    this.checkoutState.update(state => ({ ...state, paymentInfo: info }));
  }
}
