import { HttpClient } from '@angular/common/http';
import { computed, Injectable, signal } from '@angular/core';
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
  private homeSignal = signal<HomeView | null>(null);
  public homeData = computed(() => this.homeSignal());
  constructor(private http: HttpClient) { }

  fetchHomeInit() {
    if (this.homeSignal() !== null) {
      return;
    }
    this.http.get<ApiResponse<HomeView>>(`${this.baseUrl}/home`).subscribe({
      next: (res) => {

        this.homeSignal.set(res.data);
      }
    });
    console.log(this.homeSignal());
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
  LoadMoreProduct(tag: string, slugName: string, cursor: number): Observable<ProductView[] | null> {
    return this.http.get<ApiResponse<ProductView[]>>(`${this.baseUrl}/products/${tag}/${slugName}?cursor=${cursor}`).pipe(
      map(response => response.data || null)
    );
  }
}