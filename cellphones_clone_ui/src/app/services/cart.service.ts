import { computed, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { CartRequest, CartView } from '../core/models/cart_request.model';
import { ApiResponse } from '../core/models/api-response.model';

const headers = new HttpHeaders({
  'Content-Type': 'application/json',
  'Accept': 'application/json'
});

@Injectable({
  providedIn: 'root'
})

export class CartService {
  private baseUrl = environment.apiUrl;
  private cartItems = signal<CartView[]>([]);
  constructor(private http: HttpClient) { }
  addToCart(request: CartRequest): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/carts/addToCart`, request, { headers: headers }).pipe(
      map(res => res.data)
    );
  }
  getList(page: number): Observable<CartView[]> {
    return this.http.get<ApiResponse<CartView[]>>(`${this.baseUrl}/carts/${page}`).pipe(
      map(res => res.data || [])
    );
  }
  plusQuantity(cartDetailId: number): Observable<number> {
    return this.http.patch<ApiResponse<number>>(`${this.baseUrl}/carts/plusQuantity/${cartDetailId}`, null).pipe(
      map(res => res.data)
    );
  }
  minusQuantity(cartDetailId: number): Observable<number> {
    return this.http.patch<ApiResponse<number>>(`${this.baseUrl}/carts/minusQuantity/${cartDetailId}`, null).pipe(
      map(res => res.data)
    );
  }
  updateCartItems(items: CartView[]) {
    this.cartItems.set(items);
  }
  cartQuantity = computed(() => {
    return this.cartItems().reduce((total, item) => total + (item.quantity || 0), 0);
  });
}
