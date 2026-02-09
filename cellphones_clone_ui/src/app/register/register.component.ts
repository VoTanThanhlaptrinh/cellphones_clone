import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NotifyService } from '../services/notify.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  registerForm: FormGroup;
  isStudentMode = true; // Default checked in HTML
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private notifyService: NotifyService
  ) {
    this.registerForm = this.fb.group({
      // Basic Info
      name: ['', [Validators.required, Validators.maxLength(100)]],
      birthday: ['', Validators.required],
      phone: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      email: ['', [Validators.required, Validators.email]], // Required by backend
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required],
      promotionConsent: [false],

      // Student/Teacher Info
      isStudent: [true], // Toggle
      educationType: ['student'], // student, teacher, freshman
      educationLevel: [''], // Hidden input in HTML
      schoolName: [''],
      educationCode: [''], // ID Student/Teacher
      cardName: [''],
      // cardExpiryDate: [''], // HTML uses text input with dd/mm/yyyy
      cardExpiryDate: [''],
      schoolEmail: ['', [Validators.email, Validators.pattern(/^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.edu(\.[a-z]{2,})?$/)]], // Optional in HTML but specific regex in backend
      frontCard: [null],
      backCard: [null]
    }, { validators: this.passwordMatchValidator });
  }

  // Custom Validator
  passwordMatchValidator(control: AbstractControl) {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ mismatch: true });
    } else {
      // Clear mismatch error if previously set
      if (confirmPassword?.hasError('mismatch')) {
        confirmPassword.setErrors(null);
      }
    }
    return null;
  }

  // File Handling
  onFileChange(event: any, field: string) {
    if (event.target.files && event.target.files.length > 0) {
      const file = event.target.files[0];
      // Check file size (2MB limit from backend)
      if (file.size > 2 * 1024 * 1024) {
        this.notifyService.error('File size exceeds 2MB limit');
        event.target.value = ''; // Clear input
        return;
      }
      this.registerForm.patchValue({
        [field]: file
      });
    }
  }

  toggleStudentMode() {
    this.isStudentMode = !this.isStudentMode;
    this.registerForm.patchValue({ isStudent: this.isStudentMode });

    // Update validators based on mode
    const studentFields = ['educationType', 'schoolName', 'educationCode', 'cardName', 'cardExpiryDate', 'emailSchool', 'frontCard', 'backCard'];
    if (this.isStudentMode) {
      // Add validators if needed, or rely on onSubmit check
    } else {
      // Clear validators?
    }
  }

  onSubmit() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      this.notifyService.warning('Vui lòng kiểm tra lại thông tin đăng ký');
      return;
    }

    this.isLoading = true;
    const formValue = this.registerForm.value;

    // Construct DTOs
    const registerDTO = {
      FullName: formValue.name,
      BirthDay: formValue.birthday, // Need to ensure format matches DateOnly (YYYY-MM-DD or standard)
      Phone: formValue.phone,
      Email: formValue.email,
      Password: formValue.password,
      ConfPassword: formValue.confirmPassword
    };

    if (this.isStudentMode) {
      // Helper to convert file to byte[] is tricky in pure JSON Post. 
      // Usually requires FormData for file uploads.
      // Backend expects [FromBody] DTO which contains byte[]. 
      // This implies we need to convert File to Base64 or byte array client-side.

      this.convertFilesAndSubmit(registerDTO, formValue);
    } else {
      this.authService.register(registerDTO).subscribe({
        next: () => {
          this.isLoading = false;
          this.notifyService.success('Đăng ký thành công');
          this.router.navigate(['/login']);
        },
        error: () => this.isLoading = false
      });
    }
  }

  private async convertFilesAndSubmit(registerDTO: any, formValue: any) {
    try {
      const frontCardBytes = await this.fileToByteArray(formValue.frontCard);
      const backCardBytes = await this.fileToByteArray(formValue.backCard);

      // Map format dd/mm/yyyy to YYYY-MM-DD for backend DateOnly
      // Assuming input date is dd/mm/yyyy
      const expiryDate = this.formatDate(formValue.cardExpiryDate);

      const emailSchool = formValue.schoolEmail || formValue.email; // Fallback? Backend requires EmailSchool.

      if (formValue.educationType === 'teacher') {
        const teacherDTO = {
          Register: registerDTO,
          TypeUser: 'Teacher',
          TypeSchool: formValue.educationLevel || 'University', // Default
          NameSchool: formValue.schoolName,
          IdTeacher: formValue.educationCode,
          NameInCard: formValue.cardName,
          EmailSchool: emailSchool,
          FrontFaceCard: frontCardBytes, // This logic expects Backend to accept Base64 string if mapped to byte[] in some frameworks, OR actual byte array. 
          // In C# DTO byte[] usually implies JSON array of numbers or Base64 string. 
          // Let's assume Base64 string for now as it's standard for JSON.
          BehindFaceCard: backCardBytes
        };

        // Convert byte[] (JS Array) to Base64 string if backend needs it, 
        // BUT if backend DTO is byte[], ASP.NET Core JSON model binder expects Base64 string.
        // fileToByteArray returns number[], need to convert to Base64.

        const teacherPayload = {
          ...teacherDTO,
          FrontFaceCard: this.arrayBufferToBase64(frontCardBytes as ArrayBuffer),
          BehindFaceCard: this.arrayBufferToBase64(backCardBytes as ArrayBuffer)
        };

        this.authService.teacherRegister(teacherPayload).subscribe({
          next: () => { /*...*/ this.router.navigate(['/login']); },
          error: () => this.isLoading = false
        });
      } else {
        // Student
        const studentDTO = {
          Register: registerDTO,
          TypeUser: 'Student', // or Freshman?
          TypeSchool: formValue.educationLevel || 'University',
          NameSchool: formValue.schoolName,
          IdStudent: formValue.educationCode,
          NameInCard: formValue.cardName,
          ExpiredDateCard: expiryDate,
          EmailSchool: emailSchool,
          FrontFaceCard: this.arrayBufferToBase64(frontCardBytes as ArrayBuffer),
          BehindFaceCard: this.arrayBufferToBase64(backCardBytes as ArrayBuffer)
        };

        this.authService.studentRegister(studentDTO).subscribe({
          next: () => { /*...*/ this.router.navigate(['/login']); },
          error: () => this.isLoading = false
        });
      }

    } catch (e) {
      this.isLoading = false;
      this.notifyService.error('Lỗi xử lý ảnh thẻ');
    }
  }

  private formatDate(dateStr: string): string {
    // Input dd/mm/yyyy -> YYYY-MM-DD
    if (!dateStr) return '';
    const parts = dateStr.split('/');
    if (parts.length === 3) {
      return `${parts[2]}-${parts[1]}-${parts[0]}`;
    }
    return dateStr;
  }

  private fileToByteArray(file: File): Promise<ArrayBuffer> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsArrayBuffer(file);
      reader.onload = () => resolve(reader.result as ArrayBuffer);
      reader.onerror = error => reject(error);
    });
  }

  private arrayBufferToBase64(buffer: ArrayBuffer): string {
    let binary = '';
    const bytes = new Uint8Array(buffer);
    const len = bytes.byteLength;
    for (let i = 0; i < len; i++) {
      binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
  }

  // Helper for template
  get f() { return this.registerForm.controls; }
}
