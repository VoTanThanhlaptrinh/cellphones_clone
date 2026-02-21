import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { StoreView } from '../core/models/store.model';
import { map, Observable } from 'rxjs';
import { ApiResponse } from '../core/models/api-response.model';
import { Order } from '../core/models/order.model';

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private baseUrl = environment.apiUrl;
  constructor(private http: HttpClient) {
  }
  getStoreView(): Observable<StoreView[]> {
    return this.http.get<ApiResponse<StoreView[]>>(`${this.baseUrl}/carts/getStoreViews`).pipe(
      map(response => response.data || [])
    );
  }

  createOrder(cartDetailIds: number[]): Observable<ApiResponse<Order>> {
    return this.http.post<ApiResponse<Order>>(`${this.baseUrl}/orders`, cartDetailIds);
  }
}
