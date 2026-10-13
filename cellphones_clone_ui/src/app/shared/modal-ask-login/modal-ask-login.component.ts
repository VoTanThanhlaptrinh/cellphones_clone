import { Component } from '@angular/core';
import { MatDialogModule } from '@angular/material/dialog';
import { RouterLink } from '@angular/router';

/** Hộp thoại mời khách đăng nhập/đăng ký trước khi mua hàng hoặc vào giỏ hàng. */
@Component({
  selector: 'app-modal-ask-login',
  imports: [MatDialogModule, RouterLink],
  templateUrl: './modal-ask-login.component.html',
  styleUrl: './modal-ask-login.component.css',
})
export class ModalAskLoginComponent { }
