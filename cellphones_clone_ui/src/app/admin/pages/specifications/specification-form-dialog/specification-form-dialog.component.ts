import { Component, inject, signal } from '@angular/core';
import { FormArray, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { NotifyService } from '@core/services/notify.service';
import { InputComponent } from '@shared/form/input/input.component';
import { IconComponent } from '@shared/ui/icon/icon.component';
import { SpecificationDetailItem } from '../../../models/product-details.model';
import { AdminFeaturesService } from '../../../services/admin-features.service';

@Component({
  selector: 'app-specification-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, InputComponent, IconComponent],
  templateUrl: './specification-form-dialog.component.html',
})
export class SpecificationFormDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<SpecificationFormDialogComponent, boolean>);
  private readonly features = inject(AdminFeaturesService);
  private readonly notify = inject(NotifyService);

  protected readonly saving = signal(false);

  protected readonly form = new FormGroup({
    name: new FormControl<string>('', [Validators.required, Validators.maxLength(255)]),
    details: new FormArray<FormGroup>([this.createDetailGroup()]),
  });

  get detailsArray(): FormArray<FormGroup> {
    return this.form.controls.details;
  }

  createDetailGroup(): FormGroup {
    return new FormGroup({
      name: new FormControl<string>('', [Validators.required, Validators.maxLength(255)]),
      value: new FormControl<string>('', [Validators.required, Validators.maxLength(1000)]),
    });
  }

  addDetail() {
    this.detailsArray.push(this.createDetailGroup());
  }

  removeDetail(index: number) {
    if (this.detailsArray.length > 1) {
      this.detailsArray.removeAt(index);
    }
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const name = this.form.controls.name.value?.trim() ?? '';
    const details: SpecificationDetailItem[] = this.detailsArray.controls
      .map(group => ({
        name: group.get('name')?.value?.trim() ?? '',
        value: group.get('value')?.value?.trim() ?? '',
      }))
      .filter(d => d.name.length > 0 && d.value.length > 0);

    this.saving.set(true);
    this.features.createSpecification(name, details).subscribe({
      next: () => {
        this.notify.success('Đã tạo nhóm thông số kỹ thuật thành công');
        this.dialogRef.close(true);
      },
      error: () => this.saving.set(false),
    });
  }
}
