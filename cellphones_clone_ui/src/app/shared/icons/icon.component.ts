import { Component, Input } from '@angular/core';

export type IconName =
  | 'arrow-left'
  | 'box'
  | 'cart'
  | 'check-circle'
  | 'chevron-down'
  | 'chevron-right'
  | 'eye'
  | 'eye-off'
  | 'grid'
  | 'search'
  | 'trash'
  | 'user'
  | 'x';

/**
 * Icon nét (stroke) dùng chung, theo DESIGN.md §7: màu lấy từ `currentColor`, luôn `aria-hidden`.
 * Kích thước/màu truyền qua `svgClass` (ví dụ `size-5 text-gray-500`).
 * Host là `display: contents` nên class đặt trên `svgClass`, không phải trên `<app-icon>`.
 */
@Component({
  selector: 'app-icon',
  standalone: true,
  host: { class: 'contents' },
  template: `
    <svg [class]="svgClass" viewBox="0 0 24 24" fill="none" stroke="currentColor" [attr.stroke-width]="strokeWidth"
      stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false">
      @switch (name) {
        @case ('arrow-left') {
          <path d="M19 12H5m0 0 6-6m-6 6 6 6" />
        }
        @case ('box') {
          <path d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2M9 5a2 2 0 0 0 2 2h2a2 2 0 0 0 2-2M9 5a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2" />
        }
        @case ('cart') {
          <circle cx="7.5" cy="20" r="1" />
          <circle cx="17.5" cy="20" r="1" />
          <path d="M2.5 3h.77a1 1 0 0 1 .98.8L4.65 6m0 0 1.55 8.36A2 2 0 0 0 8.16 16h9.24a1 1 0 0 0 .97-.77L20.2 7.2A.75.75 0 0 0 19.45 6H4.65Z" />
        }
        @case ('check-circle') {
          <circle cx="12" cy="12" r="9" />
          <path d="m8.5 12.5 2.5 2.5 4.5-5" />
        }
        @case ('chevron-down') {
          <path d="m6 9 6 6 6-6" />
        }
        @case ('chevron-right') {
          <path d="m9 6 6 6-6 6" />
        }
        @case ('eye') {
          <path d="M2.5 12C3.5 9.5 7 5 12 5s8.5 4.5 9.5 7c-1 2.5-4.5 7-9.5 7s-8.5-4.5-9.5-7Z" />
          <circle cx="12" cy="12" r="3" />
        }
        @case ('eye-off') {
          <path d="M3 3l18 18M10.6 10.6a2 2 0 0 0 2.8 2.8M9.9 5.1A9.8 9.8 0 0 1 12 5c5 0 8.5 4.5 9.5 7a13 13 0 0 1-2.6 3.7M6.1 6.1A13 13 0 0 0 2.5 12C3.5 14.5 7 19 12 19c1.5 0 2.8-.4 4-1" />
        }
        @case ('grid') {
          <path d="M4.7 4h6v6h-6zM14.7 4h6v6h-6zM4.7 14h6v6h-6zM17.7 14a3 3 0 1 0 0 6 3 3 0 0 0 0-6Z" />
        }
        @case ('search') {
          <circle cx="10.5" cy="10.5" r="7" />
          <path d="m21 21-5.5-5.5" />
        }
        @case ('trash') {
          <path d="M4 7h16M10 11v6m4-6v6M6 7l1 12a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2l1-12M9 7V4h6v3" />
        }
        @case ('user') {
          <circle cx="12" cy="12" r="9" />
          <circle cx="12" cy="10" r="3" />
          <path d="M6.2 18.85A4.5 4.5 0 0 1 10 16h4a4.5 4.5 0 0 1 3.8 2.85" />
        }
        @case ('x') {
          <path d="M6 6l12 12M18 6 6 18" />
        }
      }
    </svg>
  `,
})
export class IconComponent {
  @Input({ required: true }) name!: IconName;
  @Input() svgClass = 'size-5';
  @Input() strokeWidth: number | string = 1.5;
}
