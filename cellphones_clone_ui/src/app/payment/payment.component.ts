import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { OrderService } from '../services/order.service';
import { CurrencyPipe, NgClass } from '@angular/common';
import { NotifyService } from '../services/notify.service';
import { FormsModule } from '@angular/forms';
import { CartView } from '../core/models/cart_request.model';
import { DeliveryOrderRequest, PickupOrderRequest } from '../core/models/order.model';
import { PaymentService } from '../services/payment.service';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [NgClass, FormsModule],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.css',
  providers: [CurrencyPipe]
})
export class PaymentComponent implements OnInit {
  cartDetails: CartView[] = [];
  totalPrice: number = 0;
  selectedMethod = signal<'COD' | 'ONLINE'>('COD');
  isLoading = signal<boolean>(false);
  qrCodeUrl = this.paymentService.getQr();

  private currency = inject(CurrencyPipe);

  constructor(
    private router: Router,
    private orderService: OrderService,
    private paymentService: PaymentService,
    private notifyService: NotifyService) {
    effect(() => {
      if (this.selectedMethod() === 'ONLINE') {
        const orderId = this.orderService.getOrderId();
        if (orderId) {
          this.paymentService.generateQRPaymentLink(orderId);
        }
      }
    })
  }

  ngOnInit() {
    const state = history.state;
    if (state && state.cartDetails) {
      this.cartDetails = state.cartDetails;
      this.totalPrice = this.calculateTotals();
    } else {
      this.router.navigate(['/cart']);
    }
  }
  goPaymentInfor() {
    this.orderService.setAppear(true)
  }


  convertPriceVnd(price: number): string {
    const formatted = this.currency.transform(price, 'VND', 'symbol', '1.0-0') ?? '';
    return formatted.replace('₫', 'đ');
  }

  calculateTotals() {
    if (this.cartDetails && this.cartDetails.length > 0) {
      return this.cartDetails.reduce((acc, cart) => acc + ((cart.salePrice || 0) * (cart.quantity || 0)), 0);
    } else {
      return 0;
    }
  }
}
