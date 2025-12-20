import { computed, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, take } from 'rxjs';
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
  addToCart(request: CartRequest): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/carts/addToCart`, request, { headers: headers }).pipe(take(1));
  }
  getList(page: number): Observable<ApiResponse<CartView[]>> {
    return this.http.get<ApiResponse<CartView[]>>(`${this.baseUrl}/carts/${page}`).pipe(take(1));
  }
  plusQuantity(cartDetailId: number) {
    return this.http.patch<ApiResponse<number>>(`${this.baseUrl}/carts/plusQuantity/${cartDetailId}`, null).pipe(take(1));
  }
  minusQuantity(cartDetailId: number) {
    return this.http.patch<ApiResponse<number>>(`${this.baseUrl}/carts/minusQuantity/${cartDetailId}`, null).pipe(take(1));
  }
  updateCartItems(items: CartView[]) {
    this.cartItems.set(items);
  }
  cartQuantity = computed(() => {
    return this.cartItems().reduce((total, item) => total + (item.quantity || 0), 0);
  });
}
