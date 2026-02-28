import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-header-member-dashboard',
  imports: [],
  templateUrl: './header-member-dashboard.component.html',
  styleUrl: './header-member-dashboard.component.css',
})
export class HeaderMemberDashboardComponent {
  constructor(private router: Router) { }

  goHome() {
    this.router.navigate(['/home']);
  }
}
