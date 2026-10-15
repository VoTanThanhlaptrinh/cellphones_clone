import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiResponse, PagedResult } from '@core/models/api.model';
import { environment } from '../../../environments/environment';
import { CatalogRow } from './catalog.model';

/** Gọi API CRUD của các tài nguyên danh mục; mọi tài nguyên dùng chung một service vì cùng một quy ước route. */
@Injectable({ providedIn: 'root' })
export class AdminCatalogService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  /** `pageIndex` bắt đầu từ 1 (quy ước của backend). */
  list(listUrl: string, pageIndex = 1, pageSize = 10): Observable<PagedResult<CatalogRow>> {
    return this.http
      .get<ApiResponse<PagedResult<CatalogRow>>>(`${this.baseUrl}/${listUrl}`, { params: { pageIndex, pageSize } })
      .pipe(map(response => response.data));
  }

  create(adminUrl: string, body: Record<string, unknown>): Observable<unknown> {
    return this.http.post<ApiResponse<unknown>>(`${this.baseUrl}/admin/${adminUrl}`, body);
  }

  /** Backend yêu cầu `id` trong body trùng `id` trên URL. */
  update(adminUrl: string, id: number, body: Record<string, unknown>): Observable<unknown> {
    return this.http.put<ApiResponse<string>>(`${this.baseUrl}/admin/${adminUrl}/${id}`, { ...body, id });
  }

  remove(adminUrl: string, id: number): Observable<unknown> {
    return this.http.delete<ApiResponse<string>>(`${this.baseUrl}/admin/${adminUrl}/${id}`);
  }
}
