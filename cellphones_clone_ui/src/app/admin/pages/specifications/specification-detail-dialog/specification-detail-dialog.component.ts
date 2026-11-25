import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { NotifyService } from '@core/services/notify.service';
import { InputComponent } from '@shared/form/input/input.component';
import { AdminFeaturesService } from '../../../services/admin-features.service';

export interface SpecificationDetailDialogData {
  specificationId: number;
  specificationName: string;
}

@Component({
  selector: 'app-specification-detail-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, InputComponent],
  templateUrl: './specification-detail-dialog.component.html',
})
export class SpecificationDetailDialogComponent {
  protected readonly data = inject<SpecificationDetailDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<SpecificationDetailDialogComponent, boolean>);
  private readonly features = inject(AdminFeaturesService);
  private readonly notify = inject(NotifyService);

  protected readonly saving = signal(false);

  protected readonly form = new FormGroup({
    name: new FormControl<string>('', [Validators.required, Validators.maxLength(255)]),
    value: new FormControl<string>('', [Validators.required, Validators.maxLength(1000)]),
  });

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const name = this.form.controls.name.value?.trim() ?? '';
    const value = this.form.controls.value?.value?.trim() ?? '';

    this.saving.set(true);
    this.features.addSpecificationDetail(this.data.specificationId, name, value).subscribe({
      next: () => {
        this.notify.success('Đã thêm chi tiết thông số thành công');
        this.dialogRef.close(true);
      },
      error: () => this.saving.set(false),
    });
  }
}
