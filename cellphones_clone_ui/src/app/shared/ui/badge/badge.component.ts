import { Component, Input } from '@angular/core';

export type BadgeTone = 'neutral' | 'success' | 'danger' | 'info' | 'brand';

const TONES: Record<BadgeTone, string> = {
  neutral: 'bg-gray-100 text-gray-700',
  success: 'bg-green-100 text-success',
  danger: 'bg-red-50 text-danger',
  info: 'bg-blue-50 text-info',
  brand: 'bg-primary-50 text-primary-500',
};

/** Nhãn trạng thái dạng viên thuốc (DESIGN.md §5.3). Nội dung là chữ chiếu vào. */
@Component({
  selector: 'app-badge',
  template: `<span [class]="classes"><ng-content /></span>`,
})
export class BadgeComponent {
  @Input() tone: BadgeTone = 'neutral';

  get classes(): string {
    return `inline-flex items-center rounded-full px-3 py-1 text-xs font-medium ${TONES[this.tone]}`;
  }
}
