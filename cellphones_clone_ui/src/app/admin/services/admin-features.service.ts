import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiResponse } from '@core/models/api.model';
import { environment } from '../../../environments/environment';
import { CommitmentItem, ImageItem, SpecificationDetailItem, SpecificationItem } from '../models/product-details.model';

@Injectable({ providedIn: 'root' })
export class AdminFeaturesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  // ==========================================
  // 1. Commitments (4 APIs Backend Admin)
  // ==========================================

  /** Lấy danh sách cam kết của một sản phẩm (hỗ trợ cả admin và user endpoint) */
  getCommitments(productId: number): Observable<CommitmentItem[]> {
    return this.http
      .get<ApiResponse<CommitmentItem[]>>(`${this.baseUrl}/admin/commitments/product/${productId}`)
      .pipe(map(res => res.data));
  }

  /** POST /api/admin/commitments: Tạo 1 cam kết cho sản phẩm */
  createCommitment(productId: number, context: string): Observable<number> {
    return this.http
      .post<ApiResponse<number>>(`${this.baseUrl}/admin/commitments`, { productId, context })
      .pipe(map(res => res.data));
  }

  /** POST /api/admin/commitments/batch: Tạo nhiều cam kết cùng lúc cho sản phẩm */
  createMultipleCommitments(productId: number, commitments: string[]): Observable<number[]> {
    return this.http
      .post<ApiResponse<number[]>>(`${this.baseUrl}/admin/commitments/batch`, { productId, commitments })
      .pipe(map(res => res.data));
  }

  /** PUT /api/admin/commitments/{id}: Cập nhật nội dung cam kết */
  updateCommitment(commitmentId: number, context: string): Observable<string> {
    return this.http
      .put<ApiResponse<string>>(`${this.baseUrl}/admin/commitments/${commitmentId}`, { context })
      .pipe(map(res => res.message));
  }

  /** DELETE /api/admin/commitments/{id}: Xoá mềm cam kết */
  deleteCommitment(commitmentId: number): Observable<string> {
    return this.http
      .delete<ApiResponse<string>>(`${this.baseUrl}/admin/commitments/${commitmentId}`)
      .pipe(map(res => res.message));
  }

  // ==========================================
  // 2. Specifications (3 APIs Backend Admin)
  // ==========================================

  /** Lấy tất cả nhóm thông số kỹ thuật kèm chi tiết */
  getSpecifications(): Observable<SpecificationItem[]> {
    return this.http
      .get<ApiResponse<SpecificationItem[]>>(`${this.baseUrl}/specifications`)
      .pipe(map(res => res.data));
  }

  /** POST /api/admin/specifications: Tạo nhóm thông số mới với các chi tiết ban đầu */
  createSpecification(name: string, details?: SpecificationDetailItem[]): Observable<number> {
    return this.http
      .post<ApiResponse<number>>(`${this.baseUrl}/admin/specifications`, { name, details })
      .pipe(map(res => res.data));
  }

  /** POST /api/admin/specifications/{id}/details: Thêm chi tiết vào nhóm thông số */
  addSpecificationDetail(specificationId: number, name: string, value: string): Observable<number> {
    return this.http
      .post<ApiResponse<number>>(`${this.baseUrl}/admin/specifications/${specificationId}/details`, { name, value })
      .pipe(map(res => res.data));
  }

  /** DELETE /api/admin/specifications/{id}: Xoá mềm nhóm thông số */
  deleteSpecification(specificationId: number): Observable<string> {
    return this.http
      .delete<ApiResponse<string>>(`${this.baseUrl}/admin/specifications/${specificationId}`)
      .pipe(map(res => res.message));
  }

  // ==========================================
  // 3. Images / Media (3 APIs Backend Admin)
  // ==========================================

  /** Lấy danh sách ảnh trong thư viện */
  getImages(): Observable<ImageItem[]> {
    return this.http
      .get<ApiResponse<ImageItem[]>>(`${this.baseUrl}/admin/images`)
      .pipe(map(res => res.data));
  }

  /** POST /api/admin/images/upload: Upload file ảnh trực tiếp qua FormData */
  uploadImage(file: File, alt?: string): Observable<number> {
    const formData = new FormData();
    formData.append('file', file);
    if (alt && alt.trim() !== '') {
      formData.append('alt', alt.trim());
    }
    return this.http
      .post<ApiResponse<number>>(`${this.baseUrl}/admin/images/upload`, formData)
      .pipe(map(res => res.data));
  }

  /** POST /api/admin/images: Tạo bản ghi ảnh từ URL bên ngoài */
  createImageFromUrl(originUrl: string, name: string, mimeType: string, alt: string): Observable<number> {
    return this.http
      .post<ApiResponse<number>>(`${this.baseUrl}/admin/images`, {
        originUrl: originUrl.trim(),
        name: name.trim(),
        mimeType: mimeType.trim(),
        alt: alt.trim(),
      })
      .pipe(map(res => res.data));
  }

  /** DELETE /api/admin/images/{id}: Xoá mềm ảnh */
  deleteImage(imageId: number): Observable<string> {
    return this.http
      .delete<ApiResponse<string>>(`${this.baseUrl}/admin/images/${imageId}`)
      .pipe(map(res => res.message));
  }
}
