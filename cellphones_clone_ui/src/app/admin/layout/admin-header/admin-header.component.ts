import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '@core/auth/auth.service';
import { IconComponent } from '@shared/ui/icon/icon.component';
import { SidebarState } from '../sidebar.state';

@Component({
  selector: 'app-admin-header',
  imports: [RouterLink, IconComponent],
  templateUrl: './admin-header.component.html',
})
export class AdminHeaderComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  protected readonly sidebar = inject(SidebarState);

  protected readonly user = this.authService.currentUser;
  /** Chữ cái đầu của tên để làm ảnh đại diện. */
  protected readonly initial = computed(() => (this.user()?.fullName?.trim().charAt(0) || '?').toUpperCase());

  logout() {
    this.authService.logout();
    this.router.navigate(['/home']);
  }
}
