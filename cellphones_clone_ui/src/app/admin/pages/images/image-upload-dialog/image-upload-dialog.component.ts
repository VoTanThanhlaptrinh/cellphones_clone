import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { NotifyService } from '@core/services/notify.service';
import { InputComponent } from '@shared/form/input/input.component';
import { IconComponent } from '@shared/ui/icon/icon.component';
import { AdminFeaturesService, UploadStorageError } from '../../../services/admin-features.service';

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
const MAX_SIZE = 2 * 1024 * 1024; // khớp luật context product-gallery của file-service

@Component({
  selector: 'app-image-upload-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, InputComponent, IconComponent],
  templateUrl: './image-upload-dialog.component.html',
})
export class ImageUploadDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<ImageUploadDialogComponent, boolean>);
  private readonly features = inject(AdminFeaturesService);
  private readonly notify = inject(NotifyService);

  protected readonly selectedFile = signal<File | null>(null);
  protected readonly previewUrl = signal<string | null>(null);
  protected readonly uploading = signal(false);

  protected readonly form = new FormGroup({
    alt: new FormControl<string>('', [Validators.maxLength(255)]),
  });

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      if (!ALLOWED_TYPES.includes(file.type)) {
        this.notify.error('Chỉ hỗ trợ ảnh JPG, PNG hoặc WEBP');
        input.value = '';
        return;
      }
      if (file.size > MAX_SIZE) {
        this.notify.error('Ảnh không được vượt quá 2MB');
        input.value = '';
        return;
      }
      this.selectedFile.set(file);

      // Create object URL for preview
      const reader = new FileReader();
      reader.onload = () => this.previewUrl.set(reader.result as string);
      reader.readAsDataURL(file);
    }
  }

  submit() {
    const file = this.selectedFile();
    if (!file) {
      this.notify.error('Vui lòng chọn một file ảnh');
      return;
    }

    const alt = this.form.controls.alt.value?.trim() || file.name;
    this.uploading.set(true);

    this.features.uploadImage(file, alt).subscribe({
      next: (id) => {
        this.notify.success(`Đã upload ảnh thành công (ID: #${id})`);
        this.dialogRef.close(true);
      },
      error: (err: unknown) => {
        // Lỗi gọi backend đã được errorInterceptor báo; riêng lỗi PUT lên R2 thì tự báo ở đây.
        if (err instanceof UploadStorageError) this.notify.error(err.message);
        this.uploading.set(false);
      },
    });
  }
}
