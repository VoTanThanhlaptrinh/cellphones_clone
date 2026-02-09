import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { map, Observable } from 'rxjs';
import { ApiResponse } from '../core/models/api-response.model';
import { ProductView, ProductViewDetail } from '../core/models/product.model';
import { HomeView } from '../core/models/home_view.model';


@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private baseUrl = environment.apiUrl;
  constructor(private http: HttpClient) { }

  homeInit(): Observable<HomeView | null> {
    return this.http.get<ApiResponse<HomeView>>(`${this.baseUrl}/home`).pipe(
      map(response => response.data || null)
    );
  }

  getProductDetail(productId: number): Observable<ProductViewDetail | null> {
    return this.http.get<ApiResponse<ProductViewDetail>>(`${this.baseUrl}/products/${productId}`).pipe(
      map(response => response.data ?? null)
    );
  }

  getProductsByCategory(categoryId: number, page: number, size: number): Observable<ProductView[]> {
    return this.http.get<ApiResponse<ProductView[]>>(`${this.baseUrl}/products/category/${categoryId}/${page}/${size}`).pipe(
      map(response => response.data || [])
    );
  }

  getProductsByBrand(brandId: number, page: number, size: number): Observable<ProductView[]> {
    return this.http.get<ApiResponse<ProductView[]>>(`${this.baseUrl}/products/brand/${brandId}/${page}/${size}`).pipe(
      map(response => response.data || [])
    );
  }

  getProductsBySeries(seriesId: number, page: number, size: number): Observable<ProductView[]> {
    return this.http.get<ApiResponse<ProductView[]>>(`${this.baseUrl}/products/series/${seriesId}/${page}/${size}`).pipe(
      map(response => response.data || [])
    );
  }

  searchProducts(searchTerm: string): Observable<ProductView[]> {
    return this.http.get<ApiResponse<ProductView[]>>(`${this.baseUrl}/search?keyword=${encodeURIComponent(searchTerm)}`).pipe(
      map(response => response.data || [])
    );
  }
}