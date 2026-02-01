import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { map, Observable, take } from 'rxjs';
import { ApiResponse } from '../core/models/api-response.model';
import { ProductView, ProductViewDetail } from '../core/models/product.model';
import { HomeView } from '../core/models/home_view.model';
import * as e from 'express';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private baseUrl = environment.apiUrl;
  constructor(private http: HttpClient) { }
  HomeInit(): Observable<HomeView | null> {
    return this.http.get<ApiResponse<HomeView>>(`${this.baseUrl}/home`).pipe(take(1), map(response => {
      if (response.data === null || response.data === undefined) {
        return null;
      }
      return response.data;
    }));
  }
  getProductDetail(productId: number): Observable<ProductViewDetail | null> {
    return this.http.get<ApiResponse<ProductViewDetail>>(`${this.baseUrl}/products/${productId}`).pipe(take(1), map(response => {
      if (response.data === null || response.data === undefined) {
        return null;
      }
      return response.data;
    }));
  }
  getProductsByCategory(categoryId: number, page: number, size: number): Observable<ProductView[]> {
    return this.http.get<ApiResponse<ProductView[]>>(`${this.baseUrl}/products/category/${categoryId}/${page}/${size}`).pipe(take(1), map(response => {
      if (response.data === null || response.data === undefined) {
        return [];
      }
      return response.data.map(product => {
        product.imgUrl = product.imgUrl;
        product.id = product.id;
        product.productName = product.productName;
        product.basePrice = product.basePrice;
        product.salePrice = product.salePrice;
        return product;
      });
    }));
  }
  getProductsByBrand(brandId: number, page: number, size: number): Observable<ProductView[]> {
    return this.http.get<ApiResponse<ProductView[]>>(`${this.baseUrl}/products/brand/${brandId}/${page}/${size}`).pipe(take(1), map(response => {
      if (response.data === null || response.data === undefined) {
        return [];
      }
      return response.data.map(product => {
        product.imgUrl = product.imgUrl;
        product.id = product.id;
        product.productName = product.productName;
        product.basePrice = product.basePrice;
        product.salePrice = product.salePrice;
        return product;
      });
    }));
  }
  getProductsBySeries(seriesId: number, page: number, size: number): Observable<ProductView[]> {
    return this.http.get<ApiResponse<ProductView[]>>(`${this.baseUrl}/products/series/${seriesId}/${page}/${size}`).pipe(take(1), map(response => {
      if (response.data === null || response.data === undefined) {
        return [];
      }
      return response.data.map(product => {
        product.imgUrl = product.imgUrl;
        product.id = product.id;
        product.productName = product.productName;
        product.basePrice = product.basePrice;
        product.salePrice = product.salePrice;
        return product;
      });
    }));
  }
  searchProducts(searchTerm: string): Observable<ProductView[]> {
    return this.http.get<ApiResponse<ProductView[]>>(`${this.baseUrl}/search?keyword=${encodeURIComponent(searchTerm)}`).pipe(take(1), map(response => {
      if(response.data === null || response.data === undefined) {
        return [];
      }
      return response.data.map(product => {
        product.imgUrl = product.imgUrl;
        product.id = product.id;
        product.productName = product.productName;
        product.basePrice = product.basePrice;
        product.salePrice = product.salePrice;
        return product;
      });
    }));
  }
}