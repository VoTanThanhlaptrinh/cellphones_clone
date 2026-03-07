import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../core/models/api-response.model';
import { NotifyService } from './notify.service';

@Injectable({
    providedIn: 'root'
})
export class PaymentService {
    private baseUrl = environment.apiUrl;
    private qr = signal<string | null>(null);
    constructor(private http: HttpClient, private notifyService: NotifyService) { }

    generateQRPaymentLink(orderId: number) {
        if (this.getQr() !== null) {
            return;
        }
        this.http.get<ApiResponse<string>>(`${this.baseUrl}/payment/qr-payment/${orderId}`).subscribe({
            next: (response) => {
                this.qr.set(response.data);
            },
            error: (err) => {
                this.notifyService.error('Lỗi khi tạo mã QR thanh toán.');
            }
        });
    }
    getQr() {
        return this.qr();
    }
}
