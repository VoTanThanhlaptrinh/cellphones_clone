import { Injectable, signal } from '@angular/core';

/** Chiều rộng từ đó sidebar cố định bên trái (Tailwind `lg` = 64rem = 1024px). */
const DESKTOP_QUERY = '(min-width: 64rem)';

/** Trạng thái sidebar của khu quản trị. Được cấp bởi AdminShellComponent nên mỗi lần vào /admin là một trạng thái mới. */
@Injectable()
export class SidebarState {
  /** Desktop: thu gọn thành cột icon. */
  readonly collapsed = signal(false);
  /** Mobile: mở dạng ngăn kéo phủ lên nội dung. */
  readonly mobileOpen = signal(false);

  /** Nút ở header: desktop thì thu gọn/mở rộng, mobile thì mở/đóng ngăn kéo. */
  toggle() {
    if (window.matchMedia(DESKTOP_QUERY).matches) {
      this.collapsed.update(v => !v);
    } else {
      this.mobileOpen.update(v => !v);
    }
  }

  closeMobile() {
    this.mobileOpen.set(false);
  }
}
