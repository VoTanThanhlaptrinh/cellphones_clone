import { Component, inject, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '@core/auth/auth.service';
import { IconComponent } from '@shared/ui/icon/icon.component';

@Component({
  selector: 'app-header-cart-order',
  imports: [RouterLink, IconComponent],
  templateUrl: './header-cart-order.component.html',
  styleUrl: './header-cart-order.component.css'
})
export class HeaderCartOrderComponent {
  @Input() totalQuantity?: number;
  private readonly authService = inject(AuthService);
  /** Truyền vào để ghi đè; mặc định lấy tên người dùng đang đăng nhập. */
  @Input() Username?: string;

  get displayName(): string {
    return this.Username ?? this.authService.currentUser()?.fullName ?? '';
  }

}
