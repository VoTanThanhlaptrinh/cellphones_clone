import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { IconComponent } from '../../icons/icon.component';

/**
 * Ô nhập có gợi ý (combobox). Chọn bằng chuột hoặc bàn phím: ↑/↓ di chuyển, Enter chọn, Esc đóng.
 */
@Component({
  selector: 'app-autocomplete-input',
  imports: [IconComponent],
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
  private static nextId = 0;
  /** Id duy nhất cho mỗi instance để label/aria gắn đúng input. */
  readonly inputId = `app-autocomplete-${AutocompleteInputComponent.nextId++}`;

  @Output() dataChosen = new EventEmitter<object>();
  data: string = ''
  @Input()
  label: string = ''
  @Input()
  original: string[] = []
  /** Thông báo lỗi tiếng Việt do form cha truyền vào (rỗng = không lỗi). */
  @Input() errorMessage: string = ''
  filtered: string[] = []
  open = false
  /** Mục đang được chọn bằng phím ↑/↓ (-1 = chưa chọn). */
  activeIndex = -1

  onChange: any = () => { }
  onTouch: any = () => { }
  disabled = false

  ngOnInit(): void {
    this.filtered = [... this.original]
  }
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['original']) {
      // Chỉ cập nhật lại danh sách hiển thị, tuyệt đối KHÔNG can thiệp xóa this.data
      this.filtered = [...this.original];
      this.activeIndex = -1;
    }
  }

  writeValue(value: any): void {
    this.data = value || '';
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

  onFocus() {
    this.open = true;
  }

  onBlur() {
    this.open = false;
    this.activeIndex = -1;
    this.onTouch();
  }

  filter(event: any) {
    var value = event.target.value
    this.data = value; // Update internal data as user types
    this.onChange(value); // Propagate change
    this.onTouch();
    this.open = true;
    this.activeIndex = -1;
    if (value)
      this.filtered = (this.original ?? []).filter(n => n.toLowerCase().includes(value.toLowerCase()))
    else
      this.filtered = [...this.original];
  }

  onKeydown(event: KeyboardEvent, inputElement: HTMLInputElement) {
    const count = this.filtered.length;
    if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.open = true;
      if (count > 0) this.activeIndex = (this.activeIndex + 1) % count;
      this.scrollActiveIntoView();
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.open = true;
      if (count > 0) this.activeIndex = this.activeIndex <= 0 ? count - 1 : this.activeIndex - 1;
      this.scrollActiveIntoView();
    } else if (event.key === 'Enter') {
      if (this.open && this.activeIndex >= 0 && this.activeIndex < count) {
        event.preventDefault();
        this.choose(this.filtered[this.activeIndex], inputElement);
      }
    } else if (event.key === 'Escape') {
      if (this.open) {
        event.preventDefault();
        this.open = false;
        this.activeIndex = -1;
      }
    }
  }

  choose(item: any, inputElement: HTMLInputElement) {
    this.data = item
    inputElement.value = item
    this.onChange(item)
    this.dataChosen.emit(item);
    this.onTouch()
    this.filtered = [...this.original];
    // Giữ focus trên ô nhập để người dùng bàn phím tiếp tục điền form; chỉ đóng danh sách.
    this.open = false;
    this.activeIndex = -1;
  }

  private scrollActiveIntoView() {
    const id = `${this.inputId}-opt-${this.activeIndex}`;
    setTimeout(() => document.getElementById(id)?.scrollIntoView({ block: 'nearest' }));
  }
}
