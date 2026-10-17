/** Một lựa chọn của các ô chọn (select, và các control tương tự sau này). */
export interface Option<T = string | number> {
  value: T;
  label: string;
}
