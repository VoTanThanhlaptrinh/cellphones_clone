import { Component, inject, OnDestroy, OnInit, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { OrderService } from '../services/order.service';
import { CurrencyPipe, NgClass } from '@angular/common';
import { NotifyService } from '../services/notify.service';
import { FormsModule } from '@angular/forms';
import { interval, Subscription } from 'rxjs';
import { HeaderCartOrderComponent } from '../shared/header-cart-order/header-cart-order.component';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [NgClass, FormsModule, HeaderCartOrderComponent],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.css',
  providers: [CurrencyPipe]
})
export class PaymentComponent implements OnInit, OnDestroy {
  cartDetailIds: number[] = [];
  totalPrice: number = 0;

  selectedMethod = signal<'BANK' | 'MOMO' | 'ZALOPAY'>('BANK');
  isLoading = signal<boolean>(false);

  // Timer logic (10 minutes)
  timeLeft = signal<number>(600);
  formattedTime = computed(() => {
    const m = Math.floor(this.timeLeft() / 60);
    const s = this.timeLeft() % 60;
    return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
  });
  private timerSubscription?: Subscription;

  // Mocked details
  transactionId = `TXN${Math.floor(Date.now() / 1000)}`;
  transferContent = `Thanh toan don hang ${this.transactionId}`;

  private currency = inject(CurrencyPipe);

  constructor(
    private router: Router,
    private orderService: OrderService,
    private notifyService: NotifyService
  ) { }

  ngOnInit() {
    const state = history.state;
    if (state && state.cartDetailIds && state.totalPrice) {
      this.cartDetailIds = state.cartDetailIds;
      this.totalPrice = state.totalPrice;
      this.startTimer();
    } else {
      this.router.navigate(['/cart']);
    }
  }

  ngOnDestroy() {
    this.timerSubscription?.unsubscribe();
  }

  startTimer() {
    this.timerSubscription = interval(1000).subscribe(() => {
      if (this.timeLeft() > 0) {
        this.timeLeft.update(v => v - 1);
      } else {
        this.timerSubscription?.unsubscribe();
      }
    });
  }

  convertPriceVnd(price: number): string {
    const formatted = this.currency.transform(price, 'VND', 'symbol', '1.0-0') ?? '';
    return formatted.replace('₫', 'đ');
  }

  simulateSuccess() {
    if (this.timeLeft() === 0) {
      this.notifyService.error('Đã hết thời gian thanh toán. Vui lòng thử lại.');
      return;
    }

    this.isLoading.set(true);
    // Call the original createOrder API upon mocked success
    this.orderService.createOrder(this.cartDetailIds).subscribe({
      next: (response) => {
        this.notifyService.success('Thanh toán thành công!');
        // Route to success page
        this.router.navigate(['/checkout'], { state: { orderId: response.data.id, success: true } });
      },
      error: (err) => {
        this.notifyService.error('Có lỗi xảy ra khi tạo đơn hàng');
        this.isLoading.set(false);
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }
}
