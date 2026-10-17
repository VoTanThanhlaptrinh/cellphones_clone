import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { IconComponent } from '@shared/ui/icon/icon.component';
import { ADMIN_NAV } from '../admin-nav';
import { SidebarState } from '../sidebar.state';

@Component({
  selector: 'app-admin-sidebar',
  imports: [RouterLink, RouterLinkActive, IconComponent],
  templateUrl: './admin-sidebar.component.html',
})
export class AdminSidebarComponent {
  protected readonly sidebar = inject(SidebarState);
  protected readonly nav = ADMIN_NAV;

  get asideClass(): string {
    const mobile = this.sidebar.mobileOpen() ? 'translate-x-0' : '-translate-x-full';
    const desktop = this.sidebar.collapsed() ? 'lg:w-20' : 'lg:w-64';
    return `fixed inset-y-0 left-0 z-50 flex w-64 flex-col border-r border-gray-200 bg-white transition-all duration-200 lg:translate-x-0 ${mobile} ${desktop}`;
  }

  /** Thu gọn (chỉ desktop) thì ẩn chữ, chỉ còn icon; nhãn vẫn có trong aria-label/title. */
  get labelClass(): string {
    return this.sidebar.collapsed() ? 'lg:hidden' : '';
  }
}
