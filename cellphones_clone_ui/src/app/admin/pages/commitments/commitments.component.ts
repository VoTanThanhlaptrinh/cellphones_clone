import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute } from '@angular/router';
import { NotifyService } from '@core/services/notify.service';
import { InputComponent } from '@shared/form/input/input.component';
import { BadgeComponent } from '@shared/ui/badge/badge.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '@shared/ui/confirm-dialog/confirm-dialog.component';
import { IconComponent } from '@shared/ui/icon/icon.component';
import { CommitmentItem } from '../../models/product-details.model';
import { AdminFeaturesService } from '../../services/admin-features.service';
import { CommitmentBatchDialogComponent, CommitmentBatchDialogData } from './commitment-batch-dialog/commitment-batch-dialog.component';
import { CommitmentDialogData, CommitmentFormDialogComponent } from './commitment-form-dialog/commitment-form-dialog.component';

@Component({
  selector: 'app-admin-commitments',
  standalone: true,
  imports: [ReactiveFormsModule, IconComponent, BadgeComponent, InputComponent],
  templateUrl: './commitments.component.html',
})
export class AdminCommitmentsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly dialog = inject(MatDialog);
  private readonly features = inject(AdminFeaturesService);
  private readonly notify = inject(NotifyService);

  protected readonly productIdControl = new FormControl<number>(1, [Validators.required, Validators.min(1)]);
  protected readonly commitments = signal<CommitmentItem[]>([]);
  protected readonly loading = signal(false);
  protected readonly searched = signal(false);

  ngOnInit() {
    const qParam = this.route.snapshot.queryParamMap.get('productId');
    if (qParam) {
      const id = parseInt(qParam, 10);
      if (!isNaN(id)) {
        this.productIdControl.setValue(id);
      }
    }
    this.loadCommitments();
  }

  loadCommitments() {
    const id = this.productIdControl.value;
    if (!id || id < 1) return;

    this.loading.set(true);
    this.searched.set(true);

    this.features.getCommitments(id).subscribe({
      next: (list) => {
        this.commitments.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.commitments.set([]);
        this.loading.set(false);
      },
    });
  }

  openAddDialog() {
    const data: CommitmentDialogData = {
      productId: this.productIdControl.value ?? 1,
    };
    this.dialog
      .open(CommitmentFormDialogComponent, { data, width: '32rem', maxWidth: '95vw' })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) this.loadCommitments();
      });
  }

  openBatchDialog() {
    const data: CommitmentBatchDialogData = {
      productId: this.productIdControl.value ?? 1,
    };
    this.dialog
      .open(CommitmentBatchDialogComponent, { data, width: '34rem', maxWidth: '95vw' })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) this.loadCommitments();
      });
  }

  openEditDialog(item: CommitmentItem) {
    const data: CommitmentDialogData = {
      productId: item.productId,
      commitmentId: item.id,
      initialContext: item.context,
    };
    this.dialog
      .open(CommitmentFormDialogComponent, { data, width: '32rem', maxWidth: '95vw' })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) this.loadCommitments();
      });
  }

  confirmDelete(item: CommitmentItem) {
    this.dialog
      .open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
        data: {
          title: `Xoá cam kết #${item.id}?`,
          message: `Cam kết "${item.context}" sẽ bị xoá khỏi sản phẩm này. Thao tác không thể hoàn tác trên giao diện.`,
          confirmLabel: 'Xoá cam kết',
          destructive: true,
        },
        maxWidth: '95vw',
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (confirmed) {
          this.features.deleteCommitment(item.id).subscribe({
            next: () => {
              this.notify.success('Đã xoá cam kết thành công');
              this.loadCommitments();
            },
          });
        }
      });
  }
}
