import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { IconComponent } from '../../icons/icon.component';

/** Bỏ dấu tiếng Việt và hạ chữ thường để gõ "ha noi" vẫn tìm ra "Hà Nội". */
function normalize(text: string): string {
  return text
    .toLowerCase()
    .normalize('NFD')
    .replace(/\p{M}/gu, '')
    .replace(/đ/g, 'd');
}

/**
 * Ô chọn có gợi ý (combobox) cho các danh sách dài như tỉnh/quận/phường.
 * - Giá trị của form chỉ nhận một mục có trong danh sách; gõ dở rồi rời ô thì quay lại giá trị đã chọn.
 * - Bàn phím: ↑/↓ di chuyển, Enter chọn, Esc đóng và hoàn tác.
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
export class AutocompleteInputComponent implements ControlValueAccessor, OnChanges {
  private static nextId = 0;
  /** Id duy nhất cho mỗi instance để label/aria gắn đúng input. */
  readonly inputId = `app-autocomplete-${AutocompleteInputComponent.nextId++}`;

  @Input() label = '';
  @Input() original: string[] = [];
  @Input() required = false;
  @Input() placeholder = 'Chọn';
  /** Nội dung hiện khi danh sách rỗng (ví dụ chưa chọn cấp cha). */
  @Input() emptyText = 'Không có dữ liệu';
  /** Thông báo lỗi tiếng Việt do form cha truyền vào (rỗng = không lỗi). */
  @Input() errorMessage = '';
  /** Phát ra mục được chọn; phát chuỗi rỗng khi người dùng xoá lựa chọn. */
  @Output() dataChosen = new EventEmitter<string>();

  /** Chữ đang hiển thị trong ô nhập. */
  text = '';
  /** Giá trị đã chọn (đồng bộ với form). */
  value = '';
  filtered: string[] = [];
  open = false;
  /** Mục đang được tô sáng bằng phím ↑/↓ (-1 = chưa có). */
  activeIndex = -1;
  disabled = false;

  private onChange: (value: string) => void = () => { };
  private onTouch: () => void = () => { };

  get inputClass(): string {
    const state = this.errorMessage
      ? 'border-danger focus:border-danger focus:ring-danger/20'
      : 'border-gray-300 focus:border-primary-500 focus:ring-primary-100';
    return `h-10 w-full rounded-lg border bg-white pl-3 pr-14 text-sm text-gray-900 placeholder:text-gray-400 focus:outline-none focus:ring-2 disabled:bg-gray-100 disabled:text-gray-400 ${state}`;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['original']) {
      this.refilter(this.open ? this.text : '');
    }
  }

  writeValue(value: string | null): void {
    this.value = value || '';
    this.text = this.value;
  }
  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.onTouch = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onFocus(input: HTMLInputElement) {
    this.refilter('');
    this.open = true;
    // Chọn hết chữ để gõ đè lên là tìm lại ngay.
    input.select();
  }

  onBlur() {
    this.open = false;
    this.activeIndex = -1;
    this.commitText();
    this.onTouch();
  }

  onInput(event: Event) {
    this.text = (event.target as HTMLInputElement).value;
    this.open = true;
    this.refilter(this.text);
  }

  toggle(input: HTMLInputElement) {
    if (this.disabled) return;
    if (this.open) {
      this.open = false;
    } else {
      input.focus();
      this.refilter('');
      this.open = true;
    }
  }

  clear(input: HTMLInputElement) {
    this.text = '';
    this.applyValue('');
    this.refilter('');
    input.focus();
  }

  onKeydown(event: KeyboardEvent) {
    const count = this.filtered.length;
    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        if (!this.open) {
          this.refilter('');
          this.open = true;
        }
        if (count > 0) this.activeIndex = (this.activeIndex + 1) % count;
        this.scrollActiveIntoView();
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.open = true;
        if (count > 0) this.activeIndex = this.activeIndex <= 0 ? count - 1 : this.activeIndex - 1;
        this.scrollActiveIntoView();
        break;
      case 'Enter': {
        if (!this.open) return;
        // Không có mục được tô sáng nhưng chỉ còn đúng một kết quả thì chọn luôn.
        const index = this.activeIndex >= 0 ? this.activeIndex : (count === 1 ? 0 : -1);
        if (index >= 0) {
          event.preventDefault();
          this.choose(this.filtered[index]);
        }
        break;
      }
      case 'Escape':
        if (this.open) {
          event.preventDefault();
          this.open = false;
          this.activeIndex = -1;
          this.text = this.value;
        }
        break;
    }
  }

  choose(item: string) {
    this.text = item;
    this.applyValue(item);
    this.refilter('');
    // Giữ focus trên ô nhập để người dùng bàn phím điền tiếp; chỉ đóng danh sách.
    this.open = false;
    this.activeIndex = -1;
  }

  /** Rời ô: chữ khớp đúng một mục thì nhận, ô trống thì xoá, còn lại hoàn tác về giá trị đã chọn. */
  private commitText() {
    const typed = this.text.trim();
    if (typed === this.value) {
      this.text = this.value;
      return;
    }
    if (!typed) {
      this.applyValue('');
      return;
    }
    const match = this.original.find(item => normalize(item) === normalize(typed));
    if (match) {
      this.text = match;
      this.applyValue(match);
    } else {
      this.text = this.value;
    }
  }

  private applyValue(item: string) {
    if (item === this.value) return;
    this.value = item;
    this.onChange(item);
    this.dataChosen.emit(item);
  }

  private refilter(query: string) {
    const q = normalize(query.trim());
    this.filtered = q ? this.original.filter(item => normalize(item).includes(q)) : [...this.original];
    this.activeIndex = -1;
  }

  private scrollActiveIntoView() {
    const id = `${this.inputId}-opt-${this.activeIndex}`;
    setTimeout(() => document.getElementById(id)?.scrollIntoView({ block: 'nearest' }));
  }
}
