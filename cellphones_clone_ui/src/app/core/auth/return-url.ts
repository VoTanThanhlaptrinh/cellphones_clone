const BACKSLASH = String.fromCharCode(92);

/**
 * Chỉ nhận đường dẫn nội bộ của app (bắt đầu bằng một dấu `/`) để `?returnUrl=` không biến trang đăng nhập
 * thành open redirect sang domain khác. Giá trị không hợp lệ thì trả về `fallback`.
 */
export function safeReturnUrl(value: string | null | undefined, fallback = '/'): string {
  if (!value || !value.startsWith('/') || value.startsWith('//') || value.includes(BACKSLASH)) {
    return fallback;
  }
  return value;
}
