import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { CategoryDetailView } from '../core/models/category_view.model';
import { ApiResponse } from '../core/models/api-response.model';
import { ProductView } from '../core/models/product.model';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private baseUrl = environment.apiUrl;
  constructor(private http: HttpClient) {
  }

  GetCategoryDetail(slugName: string): Observable<CategoryDetailView | null> {
    return this.http.get<ApiResponse<CategoryDetailView>>(`${this.baseUrl}/category/${slugName}`).pipe(
      map(response => response.data || null)
    );
  }
  LoadMoreProduct(slugName: string, page: number): Observable<ProductView[] | null> {
    return this.http.get<ApiResponse<ProductView[]>>(`${this.baseUrl}/category/loadMore/${slugName}/${page}`).pipe(
      map(response => response.data || null)
    );
  }
}
