import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../core/models/api-response.model';

@Injectable({
    providedIn: 'root'
})
export class PaymentService {
    private baseUrl = environment.apiUrl;

    constructor(private http: HttpClient) { }

    generateQRPaymentLink(orderId: number): Observable<ApiResponse<string>> {
        return this.http.get<ApiResponse<string>>(`${this.baseUrl}/payment/qr-payment/${orderId}`);
    }
}
