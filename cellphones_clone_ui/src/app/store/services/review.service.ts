import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { map, Observable } from 'rxjs';
import { ApiResponse } from '@core/models/api.model';
import { ReviewResponse, CreateReviewRequest } from '@core/models/review.model';

// === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getReviews(productId: number): Observable<ReviewResponse[]> {
    return this.http.get<ApiResponse<ReviewResponse[]>>(`${this.baseUrl}/reviews/product/${productId}`).pipe(
      map(response => response.data || [])
    );
  }

  getAverageRating(productId: number): Observable<number> {
    return this.http.get<ApiResponse<number>>(`${this.baseUrl}/reviews/product/${productId}/average`).pipe(
      map(response => response.data !== undefined ? response.data : 0)
    );
  }

  createReview(request: CreateReviewRequest): Observable<ReviewResponse> {
    return this.http.post<ApiResponse<ReviewResponse>>(`${this.baseUrl}/reviews`, request).pipe(
      map(response => response.data)
    );
  }
}
// === END NEW FEATURE: PRODUCT REVIEWS ===
