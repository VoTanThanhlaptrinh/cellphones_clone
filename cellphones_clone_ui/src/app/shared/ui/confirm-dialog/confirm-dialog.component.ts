import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  /** Hành động phá huỷ (xoá…): nút xác nhận dùng màu cảnh báo thay vì màu chính. */
  destructive?: boolean;
}

/**
 * Hộp thoại xác nhận dùng chung. Mở bằng `MatDialog.open(ConfirmDialogComponent, { data })`;
 * `afterClosed()` trả `true` khi người dùng xác nhận.
 */
@Component({
  selector: 'app-confirm-dialog',
  imports: [MatDialogModule],
  templateUrl: './confirm-dialog.component.html',
})
export class ConfirmDialogComponent {
  protected readonly data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);
}
