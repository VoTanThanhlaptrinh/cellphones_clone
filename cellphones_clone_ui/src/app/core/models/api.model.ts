/** Vỏ chung của mọi response từ backend (`ApiResponse<T>` bên .NET). */
export interface ApiResponse<T> {
  message: string;
  data: T;
}

/** Kết quả phân trang (`PagedResult<T>` bên .NET); `pageIndex` bắt đầu từ 1. */
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  totalPages: number;
}
