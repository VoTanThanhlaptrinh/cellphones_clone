import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { StoreView } from '../core/models/store.model';
import { Observable, take } from 'rxjs';
import { ApiResponse } from '../core/models/api-response.model';

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private baseUrl = environment.apiUrl;
  constructor(private http: HttpClient) {
  }
  getStoreView(): Observable<ApiResponse<StoreView[]>>{
    return this.http.get<ApiResponse<StoreView[]>>(`${this.baseUrl}/order/getStoreViews`).pipe(take(1));
  }
}
