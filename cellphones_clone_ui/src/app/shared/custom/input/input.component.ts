import { Component, forwardRef, Input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

/** Ô nhập văn bản theo recipe Input của DESIGN.md §5.6 (nhãn phía trên, viền bo góc). */
@Component({
  selector: 'app-input',
  imports: [],
  templateUrl: './input.component.html',
  styleUrl: './input.component.css',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputComponent),
      multi: true
    }
  ]
})
export class InputComponent implements ControlValueAccessor {
  private static nextId = 0;
  /** Id duy nhất cho mỗi instance để label/aria gắn đúng input. */
  readonly inputId = `app-input-${InputComponent.nextId++}`;

  @Input() label: string = '';
  @Input() placeholder: string = '';
  @Input() helperText: string = '';
  @Input() errorMessage: string = '';
  @Input() required: boolean = false;
  @Input() type: 'text' | 'tel' | 'email' = 'text';
  @Input() autocomplete: string = 'off';

  value: string = '';
  disabled: boolean = false;

  onChange: any = () => { };
  onTouch: any = () => { };

  get inputClass(): string {
    const state = this.errorMessage
      ? 'border-danger focus:border-danger focus:ring-danger/20'
      : 'border-gray-300 focus:border-primary-500 focus:ring-primary-100';
    return `h-10 w-full rounded-lg border bg-white px-3 text-sm text-gray-900 placeholder:text-gray-400 focus:outline-none focus:ring-2 disabled:bg-gray-100 disabled:text-gray-400 ${state}`;
  }

  writeValue(value: any): void {
    this.value = value || '';
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouch = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onInput(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.value = value;
    this.onChange(value);
  }
}
