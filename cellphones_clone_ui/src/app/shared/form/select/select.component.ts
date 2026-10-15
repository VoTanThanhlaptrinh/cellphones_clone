import { Component, forwardRef, Input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { IconComponent } from '../../ui/icon/icon.component';
import { Option } from '../option';

/**
 * Ô chọn một giá trị từ danh sách ngắn, dùng được với `formControlName` / `ngModel`.
 * Danh sách dài cần tìm kiếm thì dùng `app-autocomplete-input`.
 */
@Component({
  selector: 'app-select',
  imports: [IconComponent],
  templateUrl: './select.component.html',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectComponent),
      multi: true
    }
  ]
})
export class SelectComponent implements ControlValueAccessor {
  private static nextId = 0;
  /** Id duy nhất cho mỗi instance để label/aria gắn đúng select. */
  readonly selectId = `app-select-${SelectComponent.nextId++}`;

  @Input() label = '';
  @Input() options: Option[] = [];
  @Input() placeholder = 'Chọn';
  @Input() required = false;
  @Input() helperText = '';
  @Input() errorMessage = '';

  /** Giá trị đang chọn; `null` = chưa chọn. Giữ nguyên kiểu (số hoặc chuỗi) của `Option.value`. */
  value: string | number | null = null;
  disabled = false;

  private onChange: (value: string | number | null) => void = () => { };
  private onTouch: () => void = () => { };

  get selectClass(): string {
    const state = this.errorMessage
      ? 'border-danger focus:border-danger focus:ring-danger/20'
      : 'border-gray-300 focus:border-primary-500 focus:ring-primary-100';
    return `h-10 w-full appearance-none rounded-lg border bg-white pl-3 pr-9 text-sm text-gray-900 focus:outline-none focus:ring-2 disabled:bg-gray-100 disabled:text-gray-400 ${state}`;
  }

  isSelected(option: Option): boolean {
    return this.value !== null && this.value === option.value;
  }

  writeValue(value: string | number | null): void {
    this.value = value ?? null;
  }
  registerOnChange(fn: (value: string | number | null) => void): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.onTouch = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onSelect(event: Event) {
    const index = (event.target as HTMLSelectElement).selectedIndex - 1; // -1 vì có dòng placeholder ở đầu
    this.value = index >= 0 ? this.options[index].value : null;
    this.onChange(this.value);
  }

  onBlur() {
    this.onTouch();
  }
}
