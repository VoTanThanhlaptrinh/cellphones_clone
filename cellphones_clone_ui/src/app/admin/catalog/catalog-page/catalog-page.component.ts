import { Component, inject, OnInit, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, Router } from '@angular/router';
import { NotifyService } from '@core/services/notify.service';
import { AlertComponent } from '@shared/ui/alert/alert.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '@shared/ui/confirm-dialog/confirm-dialog.component';
import { IconComponent } from '@shared/ui/icon/icon.component';
import { PaginationComponent } from '@shared/ui/pagination/pagination.component';
import { TableComponent } from '@shared/ui/table/table.component';
import { AdminCatalogService } from '../admin-catalog.service';
import { CatalogFormDialogComponent, CatalogFormDialogData } from '../catalog-form-dialog/catalog-form-dialog.component';
import { findCatalogResource } from '../catalog-resources';
import { CatalogResource, CatalogRow } from '../catalog.model';

const PAGE_SIZE = 10;

/**
 * Trang quản lý dùng chung cho mọi `CatalogResource`: bảng phân trang, thêm/sửa trong hộp thoại, xoá có xác nhận.
 * Tài nguyên nào được hiển thị lấy từ `data.resource` của route (khoá trong catalog-resources.ts).
 */
@Component({
  selector: 'app-catalog-page',
  imports: [TableComponent, PaginationComponent, AlertComponent, IconComponent],
  templateUrl: './catalog-page.component.html',
})
export class CatalogPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly catalog = inject(AdminCatalogService);
  private readonly notify = inject(NotifyService);

  protected resource!: CatalogResource;
  protected readonly rows = signal<CatalogRow[]>([]);
  protected readonly pageIndex = signal(1);
  protected readonly totalPages = signal(1);
  protected readonly totalCount = signal(0);
  protected readonly loading = signal(false);
  protected readonly failed = signal(false);

  ngOnInit() {
    const resource = findCatalogResource(this.route.snapshot.data['resource']);
    if (!resource) {
      this.router.navigate(['/admin']);
      return;
    }
    this.resource = resource;
    this.load(1);
  }

  load(pageIndex: number) {
    this.loading.set(true);
    this.failed.set(false);
    this.catalog.list(this.resource.listUrl, pageIndex, PAGE_SIZE).subscribe({
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

  confirmRemove(row: CatalogRow) {
    const name = this.resource.columns[0].cell(row);
    this.dialog
      .open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
        data: {
          title: `Xoá ${this.resource.singular}?`,
          message: `"${name}" sẽ bị xoá khỏi danh sách. Thao tác này không thể hoàn tác trên giao diện.`,
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
    this.catalog.remove(this.resource.adminUrl, row.id).subscribe(() => {
      this.notify.success(`Đã xoá ${this.resource.singular}`);
      // Xoá dòng cuối của trang thì lùi về trang trước thay vì hiện trang trống.
      const page = this.rows().length === 1 && this.pageIndex() > 1 ? this.pageIndex() - 1 : this.pageIndex();
      this.load(page);
    });
  }
}
