import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { OrderHistory } from '@core/models/order.model';
import { VndPipe } from '@shared/pipes/vnd.pipe';
import { BadgeComponent } from '@shared/ui/badge/badge.component';
import { orderStatusLabel, orderStatusTone } from '@shared/ui/badge/order-status';
import { AdminCatalogService } from '../admin-catalog.service';

/** Một đơn hàng trong trang quản trị: giống `OrderHistory` của khách, thêm thông tin người đặt. */
interface AdminOrderDetail extends Omit<OrderHistory, 'createDate'> {
  createDate: string;
  customerName: string | null;
  customerEmail: string | null;
  itemCount: number;
}

/** Hộp thoại xem chi tiết một đơn hàng (chỉ đọc). */
@Component({
  selector: 'app-order-detail-dialog',
  imports: [MatDialogModule, DatePipe, VndPipe, BadgeComponent],
  templateUrl: './order-detail-dialog.component.html',
})
export class OrderDetailDialogComponent implements OnInit {
  protected readonly data = inject<{ id: number | string }>(MAT_DIALOG_DATA);
  private readonly catalog = inject(AdminCatalogService);

  protected readonly order = signal<AdminOrderDetail | null>(null);
  protected readonly failed = signal(false);

  protected readonly statusLabel = orderStatusLabel;
  protected readonly statusTone = orderStatusTone;

  ngOnInit() {
    this.catalog.get<AdminOrderDetail>('admin/orders', this.data.id).subscribe({
      next: order => this.order.set(order),
      error: () => this.failed.set(true),
    });
  }
}
