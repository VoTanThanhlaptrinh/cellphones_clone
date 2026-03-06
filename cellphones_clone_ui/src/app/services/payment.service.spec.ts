import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PaymentService } from './payment.service';
import { environment } from '../../environments/environment';

describe('PaymentService', () => {
    let service: PaymentService;
    let httpMock: HttpTestingController;
    const baseUrl = environment.apiUrl;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [PaymentService]
        });
        service = TestBed.inject(PaymentService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should call generateQRPaymentLink API', () => {
        const orderId = 123;
        const mockResponse = { data: 'https://payos.vn/qr/123456', message: 'Success', success: true };

        service.generateQRPaymentLink(orderId).subscribe(res => {
            expect(res).toEqual(mockResponse);
        });

        const req = httpMock.expectOne(`${baseUrl}/payment/qr-payment/${orderId}`);
        expect(req.request.method).toBe('GET');
        req.flush(mockResponse);
    });
});
