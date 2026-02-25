import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MemberTabsComponent } from "./member-tabs/member-tabs.component";
import { HeaderMemberDashboardComponent } from '../shared/header-member-dashboard/header-member-dashboard.component';

interface UserInfo {
  name: string;
  rank: string;
  points: number;
  avatarUrl?: string; // Optional for now
}

interface SidebarItem {
  id: string;
  label: string;
  iconClass: string; // Using generic class strings for icons (e.g., 'fas fa-home' or custom SVG)
}

@Component({
  selector: 'app-member-dashboard',
  standalone: true,
  imports: [CommonModule, HeaderMemberDashboardComponent, MemberTabsComponent],
  templateUrl: './member-dashboard.component.html',
  styleUrls: ['./member-dashboard.component.scss']
})
export class MemberDashboardComponent {
  // User info state remains here as it's used in the header strip (which is still in this component's template)
  userInfo = signal<UserInfo>({
    name: 'Võ Tấn Thành',
    rank: 'S-NULL',
    points: 0
  });
}
