import { Component, computed, inject, OnInit } from '@angular/core';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { IconComponent } from '../icons/icon.component';

/** Danh sách danh mục lấy từ API trang chủ (cùng nguồn với menu ở trang chủ). */
@Component({
  selector: 'app-modal-categories',
  imports: [MatDialogModule, RouterLink, IconComponent],
  templateUrl: './modal-categories.component.html',
  styleUrl: './modal-categories.component.css',
})
export class ModalCategoriesComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly dialogRef = inject(MatDialogRef<ModalCategoriesComponent>);

  categories = computed(() => this.productService.homeData()?.categories ?? []);

  ngOnInit(): void {
    this.productService.fetchHomeInit();
  }

  close(): void {
    this.dialogRef.close();
  }
}
