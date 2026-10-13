import { Component } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NotifyService } from '../services/notify.service';

interface RegisterField {
  name: string;
  label: string;
  type: string;
  autocomplete: string;
  placeholder: string;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  registerForm: FormGroup;
  isLoading = false;
  showPassword = false;
  showConfirmPassword = false;

  /** Các trường thông tin cơ bản (mật khẩu và điều khoản được dựng riêng trong template). */
  readonly fields: RegisterField[] = [
    { name: 'name', label: 'Họ và tên', type: 'text', autocomplete: 'name', placeholder: 'Nhập họ và tên' },
    { name: 'birthday', label: 'Ngày sinh', type: 'date', autocomplete: 'bday', placeholder: '' },
    { name: 'phone', label: 'Số điện thoại', type: 'tel', autocomplete: 'tel', placeholder: 'Nhập số điện thoại' },
    { name: 'email', label: 'Email', type: 'email', autocomplete: 'email', placeholder: 'Nhập email của bạn' },
  ];

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private notifyService: NotifyService
  ) {
    this.registerForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      birthday: ['', Validators.required],
      phone: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required],
      termsAccepted: [false, Validators.requiredTrue]
    }, { validators: this.passwordMatchValidator });

    // === BEGIN NEW FEATURE: PASSWORD POLICY ===
    // Khớp cấu hình ASP.NET Identity ở backend: chữ hoa, chữ thường, chữ số và ký tự đặc biệt.
    this.registerForm.get('password')?.addValidators(
      Validators.pattern(/^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^a-zA-Z0-9]).*$/)
    );
    // === END NEW FEATURE: PASSWORD POLICY ===
  }

  // Custom cross-field validator
  passwordMatchValidator(control: AbstractControl) {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ mismatch: true });
    } else {
      if (confirmPassword?.hasError('mismatch')) {
        confirmPassword.setErrors(null);
      }
    }
    return null;
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  registerWithGoogle(): void {
    this.authService.onOAuth2Google();
  }

  /** Thông báo lỗi tiếng Việt cho một trường; chuỗi rỗng nếu chưa chạm vào hoặc hợp lệ. */
  errorFor(name: string): string {
    const control = this.registerForm.get(name);
    if (!control || !control.touched || !control.errors) return '';
    const e = control.errors;
    switch (name) {
      case 'name':
        return e['required'] ? 'Họ và tên là bắt buộc' : 'Họ và tên tối đa 100 ký tự';
      case 'birthday':
        return 'Ngày sinh là bắt buộc';
      case 'phone':
        return e['required'] ? 'Số điện thoại là bắt buộc' : 'Số điện thoại phải có 10 chữ số';
      case 'email':
        return e['required'] ? 'Email là bắt buộc' : 'Email không hợp lệ';
      case 'password':
        if (e['required']) return 'Mật khẩu là bắt buộc';
        if (e['minlength']) return 'Mật khẩu phải dài ít nhất 8 ký tự';
        return 'Mật khẩu phải có chữ hoa, chữ thường, chữ số và ký tự đặc biệt';
      case 'confirmPassword':
        return e['required'] ? 'Xác nhận mật khẩu là bắt buộc' : 'Mật khẩu không khớp';
      case 'termsAccepted':
        return 'Bạn cần đồng ý với điều khoản để tiếp tục';
      default:
        return '';
    }
  }

  /** Class cho ô nhập: viền xám, chuyển đỏ khi có lỗi. */
  inputClass(name: string): string {
    const base = 'h-11 w-full rounded-lg border bg-white px-3 text-sm text-gray-900 placeholder:text-gray-400 focus:outline-none focus:ring-2';
    return this.errorFor(name)
      ? `${base} border-danger focus:ring-red-100`
      : `${base} border-gray-300 focus:border-primary-500 focus:ring-primary-100`;
  }

  onSubmit(): void {
    if (this.isLoading) return;
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      this.notifyService.warning('Vui lòng kiểm tra lại thông tin đăng ký');
      return;
    }

    this.isLoading = true;
    const formValue = this.registerForm.value;

    const registerDTO = {
      FullName: formValue.name,
      BirthDay: formValue.birthday,
      Phone: formValue.phone,
      Email: formValue.email,
      Password: formValue.password,
      ConfPassword: formValue.confirmPassword
    };

    this.authService.register(registerDTO).subscribe({
      next: () => {
        this.isLoading = false;
        this.notifyService.success('Đăng ký thành công');
        this.router.navigate(['/login']);
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  // Helper for template validation access
  get f() { return this.registerForm.controls; }
}
