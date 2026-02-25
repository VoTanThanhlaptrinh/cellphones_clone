import { Component } from '@angular/core';
import { MatDialogContent, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';
@Component({
  selector: 'app-modal-ask-login',
  imports: [MatDialogModule, MatButtonModule],
  templateUrl: './modal-ask-login.component.html',
  styleUrl: './modal-ask-login.component.css',
})
export class ModalAskLoginComponent {
  constructor(private router: Router) { }

  navigate(path: string): void {
    this.router.navigate([path]);
  }
}
