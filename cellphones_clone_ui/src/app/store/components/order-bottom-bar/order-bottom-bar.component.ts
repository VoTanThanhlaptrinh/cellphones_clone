import { Component, EventEmitter, Input, Output } from '@angular/core';
import { VndPipe } from '@shared/pipes/vnd.pipe';

/** Thanh dính đáy của luồng đặt hàng: tổng tiền và một nút hành động chính (DESIGN.md §5.10). Nhãn nút là nội dung chiếu vào. */
@Component({
  selector: 'app-order-bottom-bar',
  imports: [VndPipe],
  templateUrl: './order-bottom-bar.component.html',
})
export class OrderBottomBarComponent {
  @Input() totalLabel = 'Tổng tiền tạm tính:';
  @Input() total = 0;
  @Input() disabled = false;
  @Output() action = new EventEmitter<void>();
}
