import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { IconComponent } from '@shared/ui/icon/icon.component';
import { ADMIN_NAV } from '../../layout/admin-nav';

/** Trang chủ khu quản trị: lối tắt tới từng mục quản lý. Số liệu thống kê sẽ thêm khi backend có API tương ứng. */
@Component({
  selector: 'app-admin-dashboard',
  imports: [RouterLink, IconComponent],
  templateUrl: './dashboard.component.html',
})
export class AdminDashboardComponent {
  /** Mọi mục trừ chính trang này. */
  protected readonly shortcuts = ADMIN_NAV.filter(item => item.path !== 'dashboard');
}
