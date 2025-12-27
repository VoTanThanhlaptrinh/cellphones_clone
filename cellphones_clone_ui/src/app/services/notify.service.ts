import { Injectable } from '@angular/core';
import { GlobalConfig, ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root',
})
export class NotifyService {
  constructor(private toastr: ToastrService) { }
  private successTitle: string = 'Thành Công'
  private errorTitle: string = 'Lỗi';
  private infoTitle: string = 'Thông Tin';
  private warningTitle: string = 'Cảnh Báo';
  private validationTitle: string = 'Kiểm Tra';
  private confirmTitle: string = 'Xác Nhận';
  private progressTitle: string = 'Đang Xử Lý';

  success(message: string) {
    this.toastr.success(message, this.successTitle)
  }
  validation(message: string) {
    this.toastr.warning(message, this.validationTitle);
  }

  confirm(message: string) {
    this.toastr.info(message, this.confirmTitle);
  }

  progress(message: string) {
    this.toastr.info(message, this.progressTitle);
  }
  error(message: string) {
    this.toastr.error(message, this.errorTitle);
  }

  info(message: string) {
    this.toastr.info(message, this.infoTitle);
  }

  warning(message: string) {
    this.toastr.warning(message, this.warningTitle);
  }
}
