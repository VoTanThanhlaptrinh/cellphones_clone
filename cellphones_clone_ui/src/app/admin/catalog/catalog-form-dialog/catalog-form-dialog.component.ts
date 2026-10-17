import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { NotifyService } from '@core/services/notify.service';
import { InputComponent } from '@shared/form/input/input.component';
import { Option } from '@shared/form/option';
import { SelectComponent } from '@shared/form/select/select.component';
import { AdminCatalogService } from '../admin-catalog.service';
import { CatalogField, CatalogResource, CatalogRow } from '../catalog.model';

export interface CatalogFormDialogData {
  resource: CatalogResource;
  /** Có `row` là sửa, không có là thêm mới. */
  row?: CatalogRow;
}

/** Số bản ghi tối đa nạp cho một ô chọn (danh mục, thương hiệu…). */
const OPTION_PAGE_SIZE = 100;

/** Form thêm/sửa dùng chung: các ô được sinh từ `CatalogResource.fields`, lưu xong đóng hộp thoại với kết quả `true`. */
@Component({
  selector: 'app-catalog-form-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, InputComponent, SelectComponent],
  templateUrl: './catalog-form-dialog.component.html',
})
export class CatalogFormDialogComponent implements OnInit {
  protected readonly data = inject<CatalogFormDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<CatalogFormDialogComponent, boolean>);
  private readonly catalog = inject(AdminCatalogService);
  private readonly notify = inject(NotifyService);

  protected readonly isEdit = !!this.data.row;
  /** Khi sửa thì ẩn các trường `createOnly`. */
  protected readonly fields: CatalogField[] = this.data.resource.fields.filter(f => !(this.isEdit && f.createOnly));
  protected readonly form = new FormGroup<Record<string, FormControl>>({});
  protected readonly options = signal<Record<string, Option[] | undefined>>({});
  protected readonly saving = signal(false);

  constructor() {
    for (const field of this.fields) {
      const validators: ValidatorFn[] = [];
      if (field.required) validators.push(Validators.required);
      if (field.maxLength) validators.push(Validators.maxLength(field.maxLength));
      const initial = this.data.row?.[field.key] ?? (field.type === 'select' ? null : '');
      this.form.addControl(field.key, new FormControl(initial, validators));
    }
  }

  ngOnInit() {
    for (const field of this.fields.filter(f => f.type === 'select' && f.optionsFrom)) {
      const source = field.optionsFrom!;
      this.catalog.list(source.listUrl, 1, OPTION_PAGE_SIZE).subscribe(page => {
        const list = page.items.map(row => ({ value: Number(row.id), label: source.label(row) }));
        this.options.update(current => ({ ...current, [field.key]: list }));
      });
    }
  }

  errorFor(field: CatalogField): string {
    const control = this.form.get(field.key);
    if (!control || !control.touched || !control.errors) return '';
    if (control.errors['required']) {
      return field.type === 'select' ? `Vui lòng chọn ${field.label.toLowerCase()}` : `Vui lòng nhập ${field.label.toLowerCase()}`;
    }
    if (control.errors['maxlength']) return `${field.label} tối đa ${field.maxLength} ký tự`;
    return '';
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const body: Record<string, unknown> = {};
    for (const field of this.fields) {
      const value = this.form.get(field.key)?.value;
      const cleaned = typeof value === 'string' ? value.trim() : value;
      if (cleaned !== '' && cleaned !== null && cleaned !== undefined) body[field.key] = cleaned;
    }

    const { resource, row } = this.data;
    const request$ = row
      ? this.catalog.update(resource.adminUrl, Number(row.id), body)
      : this.catalog.create(resource.adminUrl, body);

    this.saving.set(true);
    request$.subscribe({
      next: () => {
        this.notify.success(row ? `Đã cập nhật ${resource.singular}` : `Đã thêm ${resource.singular}`);
        this.dialogRef.close(true);
      },
      // Lỗi đã được errorInterceptor báo bằng toast; ở lại hộp thoại để người dùng sửa và lưu lại.
      error: () => this.saving.set(false),
    });
  }
}
