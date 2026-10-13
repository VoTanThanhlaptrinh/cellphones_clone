import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

type MemberTab = 'overview' | 'orders' | 'account';

/**
 * Trang tài khoản thành viên. Chỉ hiển thị dữ liệu thật có sẵn từ phiên đăng nhập
 * (tên, số sản phẩm trong giỏ). Lịch sử đơn hàng và hồ sơ chi tiết chưa có API ở backend.
 */
@Component({
  selector: 'app-member-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './member-dashboard.component.html',
})
export class MemberDashboardComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly user = this.authService.currentUser;
  readonly activeTab = signal<MemberTab>('overview');

  readonly tabs: { id: MemberTab; label: string }[] = [
    { id: 'overview', label: 'Tổng quan' },
    { id: 'orders', label: 'Lịch sử mua hàng' },
    { id: 'account', label: 'Thông tin tài khoản' },
  ];

  /** Chữ cái đầu của tên để làm ảnh đại diện. */
  readonly initial = computed(() => (this.user()?.fullName?.trim().charAt(0) || '?').toUpperCase());

  setActiveTab(tab: MemberTab) {
    this.activeTab.set(tab);
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/home']);
  }
}
