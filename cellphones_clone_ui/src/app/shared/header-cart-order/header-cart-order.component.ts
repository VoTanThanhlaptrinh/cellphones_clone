import { Component, inject, Input } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-header-cart-order',
  imports: [],
  templateUrl: './header-cart-order.component.html',
  styleUrl: './header-cart-order.component.css'
})
export class HeaderCartOrderComponent {
  constructor(private router: Router) {

  }
  @Input() totalQuantity?: number;
  private readonly authService = inject(AuthService);
  /** Truyền vào để ghi đè; mặc định lấy tên người dùng đang đăng nhập. */
  @Input() Username?: string;

  get displayName(): string {
    return this.Username ?? this.authService.currentUser()?.fullName ?? '';
  }

  goHome() {
    this.router.navigate(['/home']);
  }
}
