// === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
export interface ReviewResponse {
  id: number;
  userId: string;
  userEmail: string;
  userName: string;
  productId: number;
  rating: number;
  comment: string | null;
  createdDate: string;
}

export interface CreateReviewRequest {
  productId: number;
  rating: number;
  comment: string | null;
}
// === END NEW FEATURE: PRODUCT REVIEWS ===
