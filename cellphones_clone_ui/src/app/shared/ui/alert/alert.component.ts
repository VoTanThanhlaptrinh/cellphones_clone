import { Component, Input } from '@angular/core';

export type AlertTone = 'success' | 'danger' | 'warning' | 'info';

const TONES: Record<AlertTone, string> = {
  success: 'border-success bg-green-50',
  danger: 'border-danger bg-red-50',
  warning: 'border-warning bg-amber-50',
  info: 'border-info bg-blue-50',
};

/** Khối thông báo trong trang (lỗi tải dữ liệu, cảnh báo…). Nội dung chiếu vào; `title` là dòng đậm phía trên. */
@Component({
  selector: 'app-alert',
  templateUrl: './alert.component.html',
})
export class AlertComponent {
  @Input() tone: AlertTone = 'info';
  @Input() title = '';

  get classes(): string {
    return `rounded-lg border-l-4 p-3 text-sm ${TONES[this.tone]}`;
  }

  /** Lỗi cần đọc ngay thì dùng role `alert`; còn lại là `status`. */
  get role(): string {
    return this.tone === 'danger' ? 'alert' : 'status';
  }
}
