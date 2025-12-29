import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, forwardRef } from '@angular/core';
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
export class AutocompleteInputComponent implements OnInit, ControlValueAccessor, OnChanges {
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['original']) {
      this.filtered = [...this.original];
      if (this.data && !this.original.includes(this.data)) {
        this.data = ''
        this.onChange('')
      }
    }
  }
  @Output() dataChosen = new EventEmitter<object>();
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
    this.dataChosen.emit(item);
    this.onTouch()
    // Reset filter?
    this.filtered = [...this.original];
    inputElement.blur();
  }
}
