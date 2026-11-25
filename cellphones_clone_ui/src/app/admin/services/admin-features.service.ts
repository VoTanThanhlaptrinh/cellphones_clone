import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { from, map, Observable, switchMap } from 'rxjs';
import { ApiResponse } from '@core/models/api.model';
import { environment } from '../../../environments/environment';
import { CommitmentItem, ImageItem, SpecificationDetailItem, SpecificationItem } from '../models/product-details.model';

interface PresignedUpload {
  tempKey: string;
  uploadUrl: string;
  expiresIn: number;
  previewUrl?: string | null;
}

/** Lỗi ở bước PUT lên R2 (không đi qua errorInterceptor nên nơi gọi phải tự báo cho người dùng). */
export class UploadStorageError extends Error {}

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

  /**
   * Upload file ảnh lên R2 qua file-service, 3 bước:
   * 1) POST /api/admin/images/upload/presign: backend xin link upload tạm
   * 2) PUT file thẳng lên R2 (không đi qua backend)
   * 3) POST /api/admin/images/upload/confirm: backend xác nhận rồi lưu bản ghi ảnh, trả về id
   */
  uploadImage(file: File, alt?: string): Observable<number> {
    return this.http
      .post<ApiResponse<PresignedUpload>>(`${this.baseUrl}/admin/images/upload/presign`, {
        fileName: file.name,
        mimeType: file.type,
        size: file.size,
      })
      .pipe(
        map(res => res.data),
        switchMap(presigned =>
          from(this.putToStorage(presigned.uploadUrl, file)).pipe(map(() => presigned.tempKey)),
        ),
        switchMap(tempKey =>
          this.http.post<ApiResponse<number>>(`${this.baseUrl}/admin/images/upload/confirm`, {
            tempKey,
            name: file.name,
            mimeType: file.type,
            alt: alt?.trim() || undefined,
          }),
        ),
        map(res => res.data),
      );
  }

  /**
   * Dùng fetch thay vì HttpClient: authInterceptor gắn "Authorization: Bearer ..." vào mọi request HttpClient,
   * còn link presigned của R2 đã được ký sẵn và sẽ từ chối header lạ.
   */
  private async putToStorage(uploadUrl: string, file: File): Promise<void> {
    let res: Response;
    try {
      res = await fetch(uploadUrl, {
        method: 'PUT',
        headers: { 'Content-Type': file.type },
        body: file,
      });
    } catch {
      throw new UploadStorageError('Không kết nối được kho lưu trữ ảnh. Kiểm tra mạng hoặc cấu hình CORS của bucket.');
    }
    if (!res.ok) {
      throw new UploadStorageError(
        res.status === 403
          ? 'Link upload đã hết hạn hoặc file không khớp, vui lòng thử lại.'
          : `Upload lên kho lưu trữ thất bại (mã ${res.status}).`,
      );
    }
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
