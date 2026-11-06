import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { NotifyService } from '@core/services/notify.service';
import { InputComponent } from '@shared/form/input/input.component';
import { AdminFeaturesService } from '../../../services/admin-features.service';

export interface CommitmentBatchDialogData {
  productId?: number;
}

@Component({
  selector: 'app-commitment-batch-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, InputComponent],
  templateUrl: './commitment-batch-dialog.component.html',
})
export class CommitmentBatchDialogComponent {
  protected readonly data = inject<CommitmentBatchDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<CommitmentBatchDialogComponent, boolean>);
  private readonly features = inject(AdminFeaturesService);
  private readonly notify = inject(NotifyService);

  protected readonly saving = signal(false);

  protected readonly form = new FormGroup({
    productId: new FormControl<number>(this.data.productId ?? 1, [Validators.required, Validators.min(1)]),
    commitmentsText: new FormControl<string>('', [Validators.required]),
  });

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const rawText = this.form.controls.commitmentsText.value ?? '';
    const lines = rawText
      .split('\n')
      .map(line => line.trim())
      .filter(line => line.length > 0);

    if (lines.length === 0) {
      this.notify.error('Vui lòng nhập ít nhất một dòng cam kết');
      return;
    }

    const productId = Number(this.form.controls.productId.value);
    this.saving.set(true);

    this.features.createMultipleCommitments(productId, lines).subscribe({
      next: (ids) => {
        this.notify.success(`Đã thêm thành công ${ids.length} cam kết`);
        this.dialogRef.close(true);
      },
      error: () => this.saving.set(false),
    });
  }
}
