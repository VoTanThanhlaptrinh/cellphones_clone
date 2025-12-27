import { Component, Input, OnInit, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-autocomplete-input',
  imports: [],
  templateUrl: './autocomplete-input.component.html',
  styleUrl: './autocomplete-input.component.css',
  standalone: true,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AutocompleteInputComponent),
      multi: true
    }
  ]
})
export class AutocompleteInputComponent implements OnInit, ControlValueAccessor {

  data: string = ''
  @Input()
  label: string = ''
  @Input()
  original: string[] = []
  filtered: string[] = []

  onChange: any = () => { }
  onTouch: any = () => { }
  disabled = false

  ngOnInit(): void {
    this.filtered = [... this.original]
    // Don't set data to original[0] by default if using CVA, let writeValue handle it
  }

  // CVA Implementation
  writeValue(value: any): void {
    this.data = value || '';
    if (!this.data && this.original.length > 0) {
      // Optional: set default? No, leave empty if not set.
    }
  }
  registerOnChange(fn: any): void {
    this.onChange = fn
  }
  registerOnTouched(fn: any): void {
    this.onTouch = fn
  }
  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled
  }

  filter(event: any) {
    var value = event.target.value
    this.data = value; // Update internal data as user types
    this.onChange(value); // Propagate change
    this.onTouch();
    if (value)
      this.filtered = this.original?.filter(n => n.toLowerCase().includes(value.toLowerCase())) || ['Không tìm thấy']
    else
      this.filtered = [...this.original];
  }

  choose(item: any, inputElement: HTMLInputElement) {
    if (item == 'Không tìm thấy')
      return;
    this.data = item
    this.onChange(item)
    this.onTouch()
    // Reset filter?
    this.filtered = [...this.original];
    inputElement.blur();
  }
}
