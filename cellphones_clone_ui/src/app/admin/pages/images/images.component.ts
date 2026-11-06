import { Component, inject, OnInit, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { NotifyService } from '@core/services/notify.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '@shared/ui/confirm-dialog/confirm-dialog.component';
import { IconComponent } from '@shared/ui/icon/icon.component';
import { ImageItem } from '../../models/product-details.model';
import { AdminFeaturesService } from '../../services/admin-features.service';
import { ImageUploadDialogComponent } from './image-upload-dialog/image-upload-dialog.component';
import { ImageUrlDialogComponent } from './image-url-dialog/image-url-dialog.component';

@Component({
  selector: 'app-admin-images',
  standalone: true,
  imports: [IconComponent],
  templateUrl: './images.component.html',
})
export class AdminImagesComponent implements OnInit {
  private readonly dialog = inject(MatDialog);
  private readonly features = inject(AdminFeaturesService);
  private readonly notify = inject(NotifyService);

  protected readonly images = signal<ImageItem[]>([]);
  protected readonly loading = signal(false);

  ngOnInit() {
    this.loadImages();
  }

  loadImages() {
    this.loading.set(true);
    this.features.getImages().subscribe({
      next: (list) => {
        this.images.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.images.set([]);
        this.loading.set(false);
      },
    });
  }

  openUploadDialog() {
    this.dialog
      .open(ImageUploadDialogComponent, { width: '32rem', maxWidth: '95vw' })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) this.loadImages();
      });
  }

  openUrlDialog() {
    this.dialog
      .open(ImageUrlDialogComponent, { width: '34rem', maxWidth: '95vw' })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) this.loadImages();
      });
  }

  copyUrl(url: string | undefined) {
    if (!url) return;
    navigator.clipboard.writeText(url).then(() => {
      this.notify.success('Đã sao chép đường dẫn hình ảnh');
    });
  }

  confirmDelete(img: ImageItem) {
    if (!img.id) return;
    this.dialog
      .open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
        data: {
          title: `Xoá hình ảnh #${img.id}?`,
          message: `Hình ảnh "${img.name || img.alt || 'này'}" sẽ bị xoá khỏi hệ thống. Thao tác không thể hoàn tác trên giao diện.`,
          confirmLabel: 'Xoá ảnh',
          destructive: true,
        },
        maxWidth: '95vw',
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (confirmed) {
          this.features.deleteImage(img.id!).subscribe({
            next: () => {
              this.notify.success('Đã xoá ảnh thành công');
              this.loadImages();
            },
          });
        }
      });
  }
}
