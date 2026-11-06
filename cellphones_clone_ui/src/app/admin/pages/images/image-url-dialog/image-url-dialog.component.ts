import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { NotifyService } from '@core/services/notify.service';
import { InputComponent } from '@shared/form/input/input.component';
import { SelectComponent } from '@shared/form/select/select.component';
import { Option } from '@shared/form/option';
import { AdminFeaturesService } from '../../../services/admin-features.service';

@Component({
  selector: 'app-image-url-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, InputComponent, SelectComponent],
  templateUrl: './image-url-dialog.component.html',
})
export class ImageUrlDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<ImageUrlDialogComponent, boolean>);
  private readonly features = inject(AdminFeaturesService);
  private readonly notify = inject(NotifyService);

  protected readonly saving = signal(false);

  protected readonly mimeOptions: Option[] = [
    { value: 'image/jpeg', label: 'JPEG / JPG' },
    { value: 'image/png', label: 'PNG' },
    { value: 'image/webp', label: 'WEBP' },
    { value: 'image/gif', label: 'GIF' },
    { value: 'image/svg+xml', label: 'SVG' },
  ];

  protected readonly form = new FormGroup({
    originUrl: new FormControl<string>('', [Validators.required, Validators.maxLength(1000)]),
    name: new FormControl<string>('', [Validators.required, Validators.maxLength(255)]),
    mimeType: new FormControl<string>('image/jpeg', [Validators.required]),
    alt: new FormControl<string>('', [Validators.required, Validators.maxLength(255)]),
  });

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { originUrl, name, mimeType, alt } = this.form.getRawValue();
    this.saving.set(true);

    this.features.createImageFromUrl(originUrl!, name!, mimeType!, alt!).subscribe({
      next: (id) => {
        this.notify.success(`Đã thêm ảnh từ URL thành công (ID: #${id})`);
        this.dialogRef.close(true);
      },
      error: () => this.saving.set(false),
    });
  }
}
