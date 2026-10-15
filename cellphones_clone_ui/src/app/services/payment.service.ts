import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import QRCode from 'qrcode';
import { environment } from '../../environments/environment';
import { ApiResponse } from '@core/models/api.model';
import { NotifyService } from '@core/services/notify.service';

@Injectable({
    providedIn: 'root'
})
export class PaymentService {
    private baseUrl = environment.apiUrl;
    private qr = signal<string | null>(null);
    constructor(private http: HttpClient, private notifyService: NotifyService) { }

    /** Mã đơn mà `qr` đang giữ (hoặc đang tải); đổi đơn thì QR cũ bị bỏ để không hiển thị nhầm đơn đã huỷ. */
    private qrOrderId: number | null = null;

    generateQRPaymentLink(orderId: number) {
        if (this.qrOrderId === orderId) {
            return;
        }
        this.qrOrderId = orderId;
        this.qr.set(null);
        this.http.get<ApiResponse<string>>(`${this.baseUrl}/payment/qr-payment/${orderId}`).subscribe({
            next: async (response) => {
                try {
                    // PayOS trả về chuỗi VietQR (EMV), không phải URL ảnh nên cần vẽ thành ảnh QR.
                    const dataUrl = await QRCode.toDataURL(response.data, { width: 256, margin: 1 });
                    if (this.qrOrderId === orderId) {
                        this.qr.set(dataUrl);
                    }
                } catch {
                    this.qrOrderId = null;
                    this.notifyService.error('Lỗi khi tạo mã QR thanh toán.');
                }
            },
            error: () => {
                this.qrOrderId = null;
                this.notifyService.error('Lỗi khi tạo mã QR thanh toán.');
            }
        });
    }
    getQr() {
        return this.qr();
    }
}
