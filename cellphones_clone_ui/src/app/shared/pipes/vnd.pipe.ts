import { Pipe, PipeTransform } from '@angular/core';

/**
 * Định dạng tiền VND: 29990000 -> "29.990.000đ".
 * - null/undefined luôn trả về `empty` ("Liên hệ").
 * - 0 cũng trả về `empty`, trừ khi `zeroAsEmpty = false` (dùng cho tổng tiền: `total | vnd:false`).
 */
@Pipe({ name: 'vnd' })
export class VndPipe implements PipeTransform {
  private static readonly formatter = new Intl.NumberFormat('vi-VN');

  transform(value: number | null | undefined, zeroAsEmpty = true, empty = 'Liên hệ'): string {
    if (value == null || Number.isNaN(value) || (zeroAsEmpty && value === 0)) {
      return empty;
    }
    return `${VndPipe.formatter.format(value)}đ`;
  }
}
