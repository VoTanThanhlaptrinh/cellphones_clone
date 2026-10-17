import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, Router } from '@angular/router';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { NotifyService } from '@core/services/notify.service';
import { InputComponent } from '@shared/form/input/input.component';
import { SelectComponent } from '@shared/form/select/select.component';
import { AlertComponent } from '@shared/ui/alert/alert.component';
import { BadgeComponent } from '@shared/ui/badge/badge.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '@shared/ui/confirm-dialog/confirm-dialog.component';
import { IconComponent } from '@shared/ui/icon/icon.component';
import { PaginationComponent } from '@shared/ui/pagination/pagination.component';
import { TableComponent } from '@shared/ui/table/table.component';
import { AdminCatalogService } from '../admin-catalog.service';
import { CatalogFormDialogComponent, CatalogFormDialogData } from '../catalog-form-dialog/catalog-form-dialog.component';
import { findCatalogResource } from '../catalog-resources';
import { CatalogResource, CatalogRow } from '../catalog.model';
import { OrderDetailDialogComponent } from '../order-detail-dialog/order-detail-dialog.component';

const PAGE_SIZE = 10;
/** Chờ người dùng gõ xong rồi mới gọi API khi lọc. */
const FILTER_DEBOUNCE_MS = 300;

/**
 * Trang quản lý dùng chung cho mọi `CatalogResource`: bảng phân trang có bộ lọc, thêm/sửa trong hộp thoại,
 * xoá có xác nhận, hoặc chỉ xem (`readOnly`). Tài nguyên nào được hiển thị lấy từ `data.resource` của route.
 */
@Component({
  selector: 'app-catalog-page',
  imports: [
    ReactiveFormsModule, TableComponent, PaginationComponent, AlertComponent, BadgeComponent, IconComponent,
    InputComponent, SelectComponent,
  ],
  templateUrl: './catalog-page.component.html',
})
export class CatalogPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly catalog = inject(AdminCatalogService);
  private readonly notify = inject(NotifyService);
  private readonly destroyRef = inject(DestroyRef);

  protected resource!: CatalogResource;
  protected readonly rows = signal<CatalogRow[]>([]);
  protected readonly pageIndex = signal(1);
  protected readonly totalPages = signal(1);
  protected readonly totalCount = signal(0);
  protected readonly loading = signal(false);
  protected readonly failed = signal(false);
  protected readonly filterForm = new FormGroup<Record<string, FormControl<string | number | null>>>({});

  ngOnInit() {
    const resource = findCatalogResource(this.route.snapshot.data['resource']);
    if (!resource) {
      this.router.navigate(['/admin']);
      return;
    }
    this.resource = resource;
    for (const filter of resource.filters ?? []) {
      this.filterForm.addControl(filter.key, new FormControl<string | number | null>(null));
    }
    this.filterForm.valueChanges
      .pipe(debounceTime(FILTER_DEBOUNCE_MS), distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b)), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load(1));
    this.load(1);
  }

  /** Giá trị các bộ lọc dưới dạng chuỗi; ô chưa chọn là chuỗi rỗng (không lọc). */
  private filterValues(): Record<string, string> {
    const values: Record<string, string> = {};
    for (const [key, value] of Object.entries(this.filterForm.getRawValue())) {
      values[key] = value === null || value === undefined ? '' : String(value);
    }
    return values;
  }

  load(pageIndex: number) {
    this.loading.set(true);
    this.failed.set(false);
    this.catalog.list(this.resource.listUrl, pageIndex, PAGE_SIZE, this.filterValues()).subscribe({
      next: page => {
        this.rows.set(page.items);
        this.pageIndex.set(page.pageIndex);
        this.totalPages.set(page.totalPages);
        this.totalCount.set(page.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.failed.set(true);
        this.loading.set(false);
      },
    });
  }

  protected hasFilters(): boolean {
    return Object.values(this.filterValues()).some(v => v !== '');
  }

  /** Nhãn hiển thị của dòng ở cột đầu tiên, dùng cho aria-label và hộp thoại xác nhận. */
  protected rowLabel(row: CatalogRow): string {
    return this.resource.columns[0].cell(row);
  }

  openForm(row?: CatalogRow) {
    const data: CatalogFormDialogData = { resource: this.resource, row };
    this.dialog
      .open<CatalogFormDialogComponent, CatalogFormDialogData, boolean>(CatalogFormDialogComponent, {
        data,
        width: '28rem',
        maxWidth: '95vw',
      })
      .afterClosed()
      .subscribe(saved => {
        if (saved) this.load(this.pageIndex());
      });
  }

  openDetail(row: CatalogRow) {
    if (this.resource.detail === 'order') {
      this.dialog.open(OrderDetailDialogComponent, { data: { id: row.id }, width: '40rem', maxWidth: '95vw' });
    }
  }

  confirmRemove(row: CatalogRow) {
    this.dialog
      .open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
        data: {
          title: `Xoá ${this.resource.singular}?`,
          message: `"${this.rowLabel(row)}" sẽ bị xoá khỏi danh sách. Thao tác này không thể hoàn tác trên giao diện.`,
          confirmLabel: 'Xoá',
          destructive: true,
        },
        maxWidth: '95vw',
      })
      .afterClosed()
      .subscribe(confirmed => {
        if (confirmed) this.remove(row);
      });
  }

  private remove(row: CatalogRow) {
    this.catalog.remove(this.resource.adminUrl, Number(row.id)).subscribe(() => {
      this.notify.success(`Đã xoá ${this.resource.singular}`);
      // Xoá dòng cuối của trang thì lùi về trang trước thay vì hiện trang trống.
      const page = this.rows().length === 1 && this.pageIndex() > 1 ? this.pageIndex() - 1 : this.pageIndex();
      this.load(page);
    });
  }
}
