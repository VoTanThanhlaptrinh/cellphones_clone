import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { NotifyService } from '@core/services/notify.service';
import { InputComponent } from '@shared/form/input/input.component';
import { AdminFeaturesService } from '../../../services/admin-features.service';

export interface CommitmentDialogData {
  productId?: number;
  commitmentId?: number;
  initialContext?: string;
}

@Component({
  selector: 'app-commitment-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, InputComponent],
  templateUrl: './commitment-form-dialog.component.html',
})
export class CommitmentFormDialogComponent implements OnInit {
  protected readonly data = inject<CommitmentDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<CommitmentFormDialogComponent, boolean>);
  private readonly features = inject(AdminFeaturesService);
  private readonly notify = inject(NotifyService);

  protected readonly isEdit = !!this.data.commitmentId;
  protected readonly saving = signal(false);

  protected readonly form = new FormGroup({
    productId: new FormControl<number>(this.data.productId ?? 1, [Validators.required, Validators.min(1)]),
    context: new FormControl<string>(this.data.initialContext ?? '', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]),
  });

  ngOnInit() {
    if (this.isEdit) {
      this.form.controls.productId.disable();
    }
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const context = this.form.controls.context.value?.trim() ?? '';
    this.saving.set(true);

    if (this.isEdit) {
      this.features.updateCommitment(this.data.commitmentId!, context).subscribe({
        next: () => {
          this.notify.success('Đã cập nhật cam kết thành công');
          this.dialogRef.close(true);
        },
        error: () => this.saving.set(false),
      });
    } else {
      const productId = Number(this.form.controls.productId.value);
      this.features.createCommitment(productId, context).subscribe({
        next: () => {
          this.notify.success('Đã thêm cam kết mới thành công');
          this.dialogRef.close(true);
        },
        error: () => this.saving.set(false),
      });
    }
  }
}
