import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, EMPTY, firstValueFrom, Subscription, switchMap, timer } from 'rxjs';
import QRCode from 'qrcode';
import { HeaderCartOrderComponent } from '@store/layout/header-cart-order/header-cart-order.component';
import { PaymentService, PENDING_PAYMENT_ORDER_KEY } from '@store/services/payment.service';
import { NotifyService } from '@core/services/notify.service';
import { OnlinePayment } from '@core/models/payment.model';
import { VndPipe } from '@shared/pipes/vnd.pipe';

/** Số giây chờ giữa lúc báo hết hạn và lúc chuyển về trang chủ. */
const REDIRECT_DELAY_SECONDS = 5;

/** Tên ngân hàng theo mã BIN (VietQR) để hiển thị cạnh số tài khoản; BIN lạ thì hiện chính mã BIN. */
const BANK_NAMES: Record<string, string> = {
  '970415': 'VietinBank', '970418': 'BIDV', '970422': 'MB Bank', '970423': 'TPBank', '970426': 'MSB',
  '970432': 'VPBank', '970436': 'Vietcombank', '970407': 'Techcombank', '970416': 'ACB', '970403': 'Sacombank',
  '970441': 'VIB', '970405': 'Agribank', '970448': 'OCB', '970437': 'HDBank', '970431': 'Eximbank',
};

/**
 * Trang giao dịch thanh toán online (/payment/:orderId). Mã đơn nằm trên URL nên tải lại trang vẫn lấy lại đúng giao dịch.
 * Hiển thị thông tin thanh toán do gateway trả (QR, tài khoản nhận, nội dung) thay vì nhúng trang của cổng; đếm ngược theo hạn
 * giao dịch, hết hạn thì báo rồi tự chuyển về trang chủ sau vài giây.
 */
@Component({
  selector: 'app-online-payment',
  standalone: true,
  imports: [HeaderCartOrderComponent, RouterLink, VndPipe],
  templateUrl: './online-payment.component.html',
})
export class OnlinePaymentComponent implements OnInit, OnDestroy {
  payment = signal<OnlinePayment | null>(null);
  qrImage = signal<string | null>(null);
  /** Số giây còn lại tới hạn giao dịch; null khi chưa biết hạn. */
  secondsLeft = signal<number | null>(null);
  expired = signal(false);
  /** Số giây còn lại trước khi chuyển về trang chủ (chỉ có sau khi hết hạn). */
  redirectIn = signal<number | null>(null);
  loadFailed = signal(false);

  private orderId = 0;
  private expiresAtMs: number | null = null;
  private qrSource: string | null = null;
  private poll?: Subscription;
  private tick?: Subscription;
  private redirect?: Subscription;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private paymentService: PaymentService,
    private notifyService: NotifyService) { }

  ngOnInit() {
    this.orderId = Number(this.route.snapshot.paramMap.get('orderId'));
    if (!Number.isInteger(this.orderId) || this.orderId <= 0) {
      this.router.navigate(['/home']);
      return;
    }
    this.poll = timer(0, 3000).pipe(
      switchMap(() => this.paymentService.getPayment(this.orderId, true).pipe(
        catchError(() => {
          // Lần tải đầu lỗi (không có quyền, không tồn tại, mất kết nối) thì báo; các lần hỏi định kỳ sau lỗi tạm thời cứ bỏ qua.
          if (!this.payment()) this.loadFailed.set(true);
          return EMPTY;
        })),
      ),
    ).subscribe(payment => this.apply(payment));
    this.tick = timer(1000, 1000).subscribe(() => this.updateCountdown());
  }

  ngOnDestroy() {
    this.stopAll();
  }

  get bankName(): string {
    const bin = this.payment()?.paymentInfo?.bankBin;
    return bin ? (BANK_NAMES[bin] ?? `Ngân hàng (BIN ${bin})`) : '';
  }

  get countdownText(): string {
    const total = this.secondsLeft();
    if (total === null) return '';
    const m = Math.floor(total / 60).toString().padStart(2, '0');
    const s = (total % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  }

  copy(value: string | null | undefined, label: string) {
    if (!value) return;
    navigator.clipboard?.writeText(value).then(
      () => this.notifyService.success(`Đã sao chép ${label}`),
      () => this.notifyService.error('Không sao chép được, vui lòng chép thủ công.'),
    );
  }

  /** VNPay không có QR để hiển thị: chuyển hướng cả trang sang cổng, quay lại qua /payment/return. */
  goToProvider() {
    const url = this.payment()?.checkoutUrl;
    if (!url || !url.startsWith('https://')) return;
    sessionStorage.setItem(PENDING_PAYMENT_ORDER_KEY, String(this.orderId));
    window.location.href = url;
  }

  private apply(payment: OnlinePayment) {
    this.payment.set(payment);
    this.loadFailed.set(false);

    if (payment.status === 'PAID') {
      this.stopAll();
      // Trang thành công đọc mã đơn từ phiên trình duyệt (PayOS không đi qua goToProvider nên chưa lưu).
      sessionStorage.setItem(PENDING_PAYMENT_ORDER_KEY, String(payment.orderId));
      this.router.navigate(['/payment/success'], { replaceUrl: true });
      return;
    }
    if (payment.status === 'EXPIRED') {
      this.expire();
      return;
    }
    if (payment.status !== 'PENDING') {
      // FAILED, CANCELLED, REVIEW_REQUIRED: dừng hỏi, giao diện hiện thông báo tương ứng.
      this.poll?.unsubscribe();
      this.tick?.unsubscribe();
      return;
    }

    this.expiresAtMs = payment.expiresAt ? new Date(payment.expiresAt).getTime() : null;
    this.updateCountdown();
    this.renderQr(payment.paymentInfo?.qrCode ?? null);
  }

  private updateCountdown() {
    if (this.expiresAtMs === null || this.expired()) return;
    const left = Math.max(0, Math.floor((this.expiresAtMs - Date.now()) / 1000));
    this.secondsLeft.set(left);
    if (left === 0) {
      void this.expireAfterFinalCheck();
    }
  }

  /** Hết giờ: hỏi lại một lần cuối (khách có thể vừa trả tiền) rồi mới báo hết hạn. */
  private async expireAfterFinalCheck() {
    this.tick?.unsubscribe();
    this.poll?.unsubscribe();
    try {
      const latest = await firstValueFrom(this.paymentService.getPayment(this.orderId, true));
      if (latest.status === 'PAID') {
        this.apply(latest);
        return;
      }
      this.payment.set(latest);
    } catch {
      // Không hỏi được thì vẫn báo hết hạn theo đồng hồ.
    }
    this.expire();
  }

  private expire() {
    if (this.expired()) return;
    this.expired.set(true);
    this.secondsLeft.set(0);
    this.poll?.unsubscribe();
    this.tick?.unsubscribe();
    sessionStorage.removeItem(PENDING_PAYMENT_ORDER_KEY);
    this.redirectIn.set(REDIRECT_DELAY_SECONDS);
    this.redirect = timer(1000, 1000).subscribe(() => {
      const left = (this.redirectIn() ?? 1) - 1;
      this.redirectIn.set(left);
      if (left <= 0) {
        this.redirect?.unsubscribe();
        this.router.navigate(['/home']);
      }
    });
  }

  /** Vẽ chuỗi VietQR (EMV) của payOS thành ảnh; chỉ vẽ lại khi chuỗi đổi. */
  private async renderQr(source: string | null) {
    if (!source || source === this.qrSource) return;
    this.qrSource = source;
    try {
      this.qrImage.set(await QRCode.toDataURL(source, { width: 256, margin: 1 }));
    } catch {
      this.qrSource = null;
      this.notifyService.error('Lỗi khi tạo mã QR thanh toán.');
    }
  }

  private stopAll() {
    this.poll?.unsubscribe();
    this.tick?.unsubscribe();
    this.redirect?.unsubscribe();
  }
}
