import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { OrderService } from './order.service';
import { PaymentFormData } from '../core/models/payment.model';
import { DeliveryOrderRequest, PickupOrderRequest } from '../core/models/order.model';
import { environment } from '../../environments/environment';

describe('OrderService', () => {
    let service: OrderService;
    let httpMock: HttpTestingController;
    const baseUrl = environment.apiUrl;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [OrderService]
        });
        service = TestBed.inject(OrderService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should store and retrieve payment data', () => {
        const mockData: PaymentFormData = {
            deliveryMethod: 'at-store',
            email: 'test@example.com',
            storeInfo: { city: 'HCM', district: 'Q1', storeId: '1', streetName: 'Le Loi', note: '' },
            shipInfo: null
        };

        service.setPaymentData(mockData);
        expect(service.getPaymentData()).toEqual(mockData);

        service.clearPaymentData();
        expect(service.getPaymentData()).toBeNull();
    });

    it('should correctly build and call createPickupOrder', () => {
        const payload: PickupOrderRequest = { cartDetailIds: [1, 2], storeHouseId: 10 };
        const mockResponse = { data: { id: 100, createDate: new Date(), orderDetails: [] }, message: 'Success', success: true };

        service.createPickupOrder(payload).subscribe(res => {
            expect(res).toEqual(mockResponse);
        });

        const req = httpMock.expectOne(`${baseUrl}/orders/pickup`);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(payload);
        req.flush(mockResponse);
    });

    it('should correctly build and call createDeliveryOrder', () => {
        const payload: DeliveryOrderRequest = { cartDetailIds: [3], provinceName: 'HCM', districtName: 'Q1', street: 'Ton Duc Thang' };
        const mockResponse = { data: { id: 200, createDate: new Date(), orderDetails: [] }, message: 'Success', success: true };

        service.createDeliveryOrder(payload).subscribe(res => {
            expect(res).toEqual(mockResponse);
        });

        const req = httpMock.expectOne(`${baseUrl}/orders/delivery`);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(payload);
        req.flush(mockResponse);
    });
});
