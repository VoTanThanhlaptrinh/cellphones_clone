import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { map, Observable, take } from 'rxjs';
import { CategoryDetailView, CategoryView } from '../core/models/category_view.model';
import { ApiResponse } from '../core/models/api-response.model';
import { ProductView } from '../core/models/product.model';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private baseUrl = environment.apiUrl;
  constructor(private http: HttpClient) {
   }

  GetCategoryDetail(categoryId: number): Observable<CategoryDetailView | null> {
    return this.http.get<ApiResponse<CategoryDetailView>>(`${this.baseUrl}/category/${categoryId}/${1}`).pipe(take(1), map(response => {
      if (response.data === null || response.data === undefined) {
        return null;
      }
      return response.data;
    }));
  }
  LoadMoreProduct(categoryId: number, page: number) {
    return this.http.get<ApiResponse<ProductView[]>>(`${this.baseUrl}/category/loadMore/${categoryId}/${page}`).pipe(take(1), map(response => {
      if (response.data === null || response.data === undefined) {
        return null;
      }
      return response.data;
    }));
  }

}
