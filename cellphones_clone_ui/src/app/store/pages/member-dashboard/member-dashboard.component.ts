import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '@core/auth/auth.service';
import { IconComponent } from '@shared/ui/icon/icon.component';
import { OrderService } from '@store/services/order.service';
import { OrderHistory } from '@core/models/order.model';
import { VndPipe } from '@shared/pipes/vnd.pipe';

type MemberTab = 'overview' | 'orders' | 'account';

const ORDER_PAGE_SIZE = 10;

const STATUS_LABELS: Record<string, string> = {
  Pending: 'Đang xử lý',
  Paid: 'Đã thanh toán',
  Shipping: 'Đang giao hàng',
  Completed: 'Hoàn thành',
  Cancelled: 'Đã hủy',
};

const STATUS_CLASSES: Record<string, string> = {
  Pending: 'bg-gray-100 text-gray-700',
  Paid: 'bg-green-100 text-success',
  Shipping: 'bg-blue-50 text-info',
  Completed: 'bg-green-100 text-success',
  Cancelled: 'bg-red-50 text-danger',
};

/**
 * Trang tài khoản thành viên: tên và số sản phẩm trong giỏ lấy từ phiên đăng nhập,
 * lịch sử mua hàng lấy từ `GET /orders/history/me`. Hồ sơ chi tiết chưa có API ở backend.
 */
@Component({
  selector: 'app-member-dashboard',
  standalone: true,
  imports: [RouterLink, IconComponent, DatePipe, VndPipe],
  templateUrl: './member-dashboard.component.html',
})
export class MemberDashboardComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly orderService = inject(OrderService);
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

  readonly orders = signal<OrderHistory[]>([]);
  readonly ordersLoading = signal(false);
  readonly ordersFailed = signal(false);
  readonly hasMoreOrders = signal(false);
  private loadedPages = 0;

  /** Ba đơn mới nhất cho khối "Đơn hàng gần đây" ở tab Tổng quan. */
  readonly recentOrders = computed(() => this.orders().slice(0, 3));

  ngOnInit() {
    if (this.user()) {
      this.loadMoreOrders();
    }
  }

  loadMoreOrders() {
    if (this.ordersLoading()) return;
    this.ordersLoading.set(true);
    this.ordersFailed.set(false);
    this.orderService.getMyOrders(this.loadedPages + 1, ORDER_PAGE_SIZE).subscribe({
      next: (page) => {
        this.orders.update(current => [...current, ...page.items]);
        this.loadedPages = page.pageIndex;
        this.hasMoreOrders.set(page.pageIndex < page.totalPages);
        this.ordersLoading.set(false);
      },
      error: () => {
        this.ordersFailed.set(true);
        this.ordersLoading.set(false);
      },
    });
  }

  statusLabel(status: string): string {
    return STATUS_LABELS[status] ?? status;
  }

  statusClass(status: string): string {
    return STATUS_CLASSES[status] ?? 'bg-gray-100 text-gray-700';
  }

  typeLabel(type: string): string {
    return type === 'pickup' ? 'Nhận tại cửa hàng' : 'Giao hàng tận nơi';
  }

  setActiveTab(tab: MemberTab) {
    this.activeTab.set(tab);
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/home']);
  }
}
