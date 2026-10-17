import { Component, EventEmitter, Input, Output } from '@angular/core';
import { IconComponent } from '@shared/ui/icon/icon.component';

/** Đầu trang của luồng đặt hàng: nút quay lại, tiêu đề (h1) và stepper 2 bước (DESIGN.md §5.11). */
@Component({
  selector: 'app-order-step-header',
  imports: [IconComponent],
  templateUrl: './order-step-header.component.html',
})
export class OrderStepHeaderComponent {
  @Input({ required: true }) title = '';
  @Input() backLabel = 'Quay lại';
  /** Bước hiện tại: 1 = Thông tin (gồm cả trang địa chỉ), 2 = Thanh toán. */
  @Input() step: 1 | 2 = 1;
  @Output() back = new EventEmitter<void>();
  /** Bấm vào bước 1 khi đang ở bước 2 (chỉ bước đã hoàn thành mới bấm được). */
  @Output() infoClick = new EventEmitter<void>();
}
