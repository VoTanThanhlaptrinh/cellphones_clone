import { Component, HostListener, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AdminHeaderComponent } from '../admin-header/admin-header.component';
import { AdminSidebarComponent } from '../admin-sidebar/admin-sidebar.component';
import { SidebarState } from '../sidebar.state';

/** Khung của khu quản trị: sidebar bên trái, header dính phía trên, nội dung ở giữa. */
@Component({
  selector: 'app-admin-shell',
  imports: [RouterOutlet, AdminHeaderComponent, AdminSidebarComponent],
  providers: [SidebarState],
  templateUrl: './admin-shell.component.html',
})
export class AdminShellComponent {
  protected readonly sidebar = inject(SidebarState);

  get contentClass(): string {
    return `transition-[padding] duration-200 ${this.sidebar.collapsed() ? 'lg:pl-20' : 'lg:pl-64'}`;
  }

  @HostListener('document:keydown.escape')
  onEscape() {
    this.sidebar.closeMobile();
  }
}
