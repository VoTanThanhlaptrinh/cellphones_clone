import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { NotifyService } from '../services/notify.service';
import { AuthService } from '../services/auth.service';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const notifyService = inject(NotifyService);
    const authService = inject(AuthService);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            // Handle 401 Unauthorized
            if (error.status === 401) {
                if (!isRefreshing) {
                    isRefreshing = true;
                    refreshTokenSubject.next(null);
                    return authService.refreshToken().pipe(
                        switchMap((newToken: any) => {
                            isRefreshing = false;
                            if (newToken) {
                                refreshTokenSubject.next(newToken);
                                // Retry the original request
                                return next(addToken(req, newToken));
                            } else {
                                // Refresh failed (already handled in authService but explicit logout here for safety)
                                authService.handleLogout();
                                return throwError(() => error);
                            }
                        }),
                        catchError((refreshError) => {
                            isRefreshing = false;
                            // Refresh failed
                            authService.handleLogout();
                            return throwError(() => refreshError);
                        })
                    );
                } else {
                    // Wait for the token to be refreshed
                    return refreshTokenSubject.pipe(
                        filter(token => token !== null),
                        take(1),
                        switchMap(token => {
                            return next(addToken(req, token!));
                        })
                    );
                }
            }

            let errorMessage = '';
            if (error.error) {
                if (typeof error.error === 'string') {
                    errorMessage = error.error;
                } else if (error.error.message) {
                    errorMessage = error.error.message;
                }
            }

            // Priority 2: Fallback to generic message based on Status Code
            if (!errorMessage) {
                if (error.error instanceof ErrorEvent) {
                    // Client-side/Network error
                    errorMessage = `Network Error: ${error.error.message}`;
                } else {
                    // Server-side error
                    switch (error.status) {
                        case 0:
                            errorMessage = 'Lỗi kết nối: Không thể kết nối đến máy chủ.';
                            break;
                        case 400:
                            errorMessage = '400 Bad Request: Yêu cầu không hợp lệ.';
                            break;
                        case 401:
                            errorMessage = '401 Unauthorized: Phiên đăng nhập hết hạn.';
                            break;
                        case 403:
                            errorMessage = '403 Forbidden: Bạn không có quyền truy cập.';
                            break;
                        case 404:
                            errorMessage = '404 Not Found: Không tìm thấy tài nguyên.';
                            break;
                        case 500:
                            errorMessage = '500 Server Error: Lỗi máy chủ nội bộ.';
                            break;
                        case 503:
                            errorMessage = '503 Service Unavailable: Dịch vụ tạm thời không khả dụng.';
                            break;
                        default:
                            errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
                    }
                }
            }

            // Display the error notification only if not 401 (handled by refresh logic) 
            // OR if refresh failed (which throws error)
            // Since we re-throw error if refresh fails, it might come here? 
            // Actually, if refresh fails, we throwError to the subscriber. 
            // But we should notify the user why they are being logged out if it's a 401 related issue?
            // Usually, if refresh fails, we just redirect to login. NotifyService might be annoying if multiple requests fail.
            // But if it's NOT a 401 that triggered a refresh, we notify.

            // If error is 401 and we are here, it means we didn't enter the refresh logic (unlikely unless recursive) 
            // OR re-thrown.
            // However, typical pattern:
            // If 401 -> try refresh.
            // If refresh fails -> logout.
            // The original request subscription will error out.

            // We should probably notify for non-401 errors, or if 401 persists.
            if (error.status !== 401) {
                notifyService.error(errorMessage);
            }

            // Re-throw the error so specific components can check it if needed
            return throwError(() => error);
        })
    );
};

function addToken(req: HttpRequest<any>, token: string) {
    return req.clone({
        setHeaders: {
            Authorization: `Bearer ${token}`
        }
    });
}
