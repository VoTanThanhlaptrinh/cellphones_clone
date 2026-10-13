import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { IconComponent } from '../shared/icons/icon.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule, IconComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  loginForm: FormGroup;
  isLoading = false;
  showPassword = false;
  /** Lỗi hiển thị ngay trong form (sai email/mật khẩu). Lỗi khác do interceptor thông báo bằng toast. */
  errorMessage = '';

  readonly benefits = [
    { highlight: 'Chiết khấu đến 5%', text: 'khi mua các sản phẩm tại CellphoneS' },
    { highlight: 'Miễn phí giao hàng', text: 'cho thành viên SMEM, SVIP và đơn hàng từ 300.000đ' },
    { highlight: 'Tặng voucher sinh nhật đến 500.000đ', text: 'cho khách hàng thành viên' },
    { highlight: 'Trợ giá thu cũ lên đời', text: 'đến 1 triệu' },
    { highlight: 'Thăng hạng nhận voucher', text: 'đến 300.000đ' },
    { highlight: 'Đặc quyền S-Student/S-Teacher', text: 'ưu đãi thêm đến 10%' },
  ];

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.isLoading) return;
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';
      this.authService.login(this.loginForm.value).subscribe({
        next: () => {
          this.isLoading = false;
          this.router.navigate(['/']);
        },
        error: (err: HttpErrorResponse) => {
          this.isLoading = false;
          if (err.status === 401) {
            this.errorMessage = 'Email hoặc mật khẩu không đúng.';
          }
        }
      });
    } else {
      this.loginForm.markAllAsTouched();
    }
  }

  loginWithGoogle() {
    this.authService.onOAuth2Google();
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  /** Class cho ô nhập: viền xám, chuyển đỏ khi có lỗi. */
  inputClass(name: 'email' | 'password'): string {
    const control = this.loginForm.get(name);
    const hasError = !!control && control.touched && control.invalid;
    const base = 'h-11 w-full rounded-lg border bg-white px-3 text-sm text-gray-900 placeholder:text-gray-400 focus:outline-none focus:ring-2';
    return hasError
      ? `${base} border-danger focus:ring-red-100`
      : `${base} border-gray-300 focus:border-primary-500 focus:ring-primary-100`;
  }

  // Helper for template access
  get f() { return this.loginForm.controls; }
}
