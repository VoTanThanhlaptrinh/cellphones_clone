import { Component, inject, OnInit, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { NotifyService } from '@core/services/notify.service';
import { BadgeComponent } from '@shared/ui/badge/badge.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '@shared/ui/confirm-dialog/confirm-dialog.component';
import { IconComponent } from '@shared/ui/icon/icon.component';
import { SpecificationItem } from '../../models/product-details.model';
import { AdminFeaturesService } from '../../services/admin-features.service';
import { SpecificationDetailDialogComponent, SpecificationDetailDialogData } from './specification-detail-dialog/specification-detail-dialog.component';
import { SpecificationFormDialogComponent } from './specification-form-dialog/specification-form-dialog.component';

@Component({
  selector: 'app-admin-specifications',
  standalone: true,
  imports: [IconComponent, BadgeComponent],
  templateUrl: './specifications.component.html',
})
export class AdminSpecificationsComponent implements OnInit {
  private readonly dialog = inject(MatDialog);
  private readonly features = inject(AdminFeaturesService);
  private readonly notify = inject(NotifyService);

  protected readonly specifications = signal<SpecificationItem[]>([]);
  protected readonly loading = signal(false);

  ngOnInit() {
    this.loadSpecifications();
  }

  loadSpecifications() {
    this.loading.set(true);
    this.features.getSpecifications().subscribe({
      next: (list) => {
        this.specifications.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.specifications.set([]);
        this.loading.set(false);
      },
    });
  }

  openCreateDialog() {
    this.dialog
      .open(SpecificationFormDialogComponent, { width: '38rem', maxWidth: '95vw' })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) this.loadSpecifications();
      });
  }

  openAddDetailDialog(spec: SpecificationItem) {
    if (!spec.id) return;
    const data: SpecificationDetailDialogData = {
      specificationId: spec.id,
      specificationName: spec.name,
    };
    this.dialog
      .open(SpecificationDetailDialogComponent, { data, width: '30rem', maxWidth: '95vw' })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) this.loadSpecifications();
      });
  }

  confirmDelete(spec: SpecificationItem) {
    if (!spec.id) return;
    this.dialog
      .open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
        data: {
          title: `Xoá nhóm thông số "${spec.name}"?`,
          message: `Nhóm thông số này cùng tất cả các thuộc tính chi tiết bên trong sẽ bị xoá. Thao tác không thể hoàn tác trên giao diện.`,
          confirmLabel: 'Xoá nhóm thông số',
          destructive: true,
        },
        maxWidth: '95vw',
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (confirmed) {
          this.features.deleteSpecification(spec.id!).subscribe({
            next: () => {
              this.notify.success('Đã xoá nhóm thông số thành công');
              this.loadSpecifications();
            },
          });
        }
      });
  }
}
