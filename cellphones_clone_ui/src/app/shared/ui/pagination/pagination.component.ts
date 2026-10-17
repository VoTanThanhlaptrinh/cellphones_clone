import { Component, EventEmitter, Input, Output } from '@angular/core';
import { IconComponent } from '../icon/icon.component';

/** Phân trang cho `PagedResult` của backend: `pageIndex` bắt đầu từ 1. */
@Component({
  selector: 'app-pagination',
  imports: [IconComponent],
  templateUrl: './pagination.component.html',
})
export class PaginationComponent {
  @Input({ required: true }) pageIndex = 1;
  @Input({ required: true }) totalPages = 1;
  @Input() totalCount: number | null = null;
  @Output() pageChange = new EventEmitter<number>();

  /** Trang đầu, trang cuối và các trang quanh trang hiện tại; `null` là dấu "…". */
  get pages(): (number | null)[] {
    const total = Math.max(this.totalPages, 1);
    const wanted = new Set([1, total, this.pageIndex - 1, this.pageIndex, this.pageIndex + 1]);
    const sorted = [...wanted].filter(p => p >= 1 && p <= total).sort((a, b) => a - b);
    const result: (number | null)[] = [];
    sorted.forEach((p, i) => {
      if (i > 0 && p - sorted[i - 1] > 1) result.push(null);
      result.push(p);
    });
    return result;
  }

  go(page: number) {
    if (page < 1 || page > this.totalPages || page === this.pageIndex) return;
    this.pageChange.emit(page);
  }
}
