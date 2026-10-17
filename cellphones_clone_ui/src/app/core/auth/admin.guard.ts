import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

/**
 * Chỉ cho ADMIN vào khu quản trị. Đọc được đồng bộ vì `provideAppInitializer` đã chờ `initializeAuth()`
 * xong trước lần điều hướng đầu tiên. Backend vẫn kiểm tra quyền ở `/api/admin/*`; guard này chỉ để tránh
 * tải code admin và hiện giao diện cho người không có quyền.
 *
 * - Chưa đăng nhập: sang /login kèm `returnUrl` để quay lại sau khi đăng nhập.
 * - Đã đăng nhập nhưng không phải ADMIN: về trang chủ.
 */
export const adminGuard: CanMatchFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.currentUser()) {
    const target = router.getCurrentNavigation()?.extractedUrl.toString() ?? '/admin';
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: target } });
  }
  return authService.isAdmin() ? true : router.createUrlTree(['/home']);
};
