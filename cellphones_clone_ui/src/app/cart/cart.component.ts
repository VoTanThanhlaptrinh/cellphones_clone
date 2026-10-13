import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../services/cart.service';
import { CartView } from '../core/models/cart_request.model';
import { NotifyService } from '../services/notify.service';
import { HeaderCartOrderComponent } from '../shared/header-cart-order/header-cart-order.component';
import { VndPipe } from '../shared/pipes/vnd.pipe';
import { IconComponent } from '../shared/icons/icon.component';

@Component({
  selector: 'app-cart',
  imports: [RouterLink, HeaderCartOrderComponent, VndPipe, IconComponent],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css',
})
export class CartComponent implements OnInit {
  page: number = 1;
  totalPrice: number = 0;
  totalQuantity: number = 0;
  /** Chọn theo cartDetailId (không theo chỉ số mảng) để không lệch sau khi xoá item. */
  selectedIds: number[] = [];
  cartDetails: CartView[] = [];
  isLoadding: boolean = true;
  hasMore: boolean = false;

  constructor(private cartService: CartService, private router: Router,
    private cdr: ChangeDetectorRef,
    private notifyService: NotifyService
  ) {

  }

  get allSelected(): boolean {
    return this.cartDetails.length > 0 && this.selectedIds.length === this.cartDetails.length;
  }

  get isDisable(): boolean {
    return this.selectedIds.length === 0;
  }

  ngOnInit(): void {
    this.cartService.getList(this.page).subscribe({
      next: res => {
        this.cartDetails = res
      },
      error: err => {
        this.isLoadding = false;
        this.notifyService.error('Có lỗi xảy ra khi tải giỏ hàng');
      },
      complete: () => {
        this.isLoadding = false;
        this.updateCommon();
      }
    })
  }

  isSelected(cartDetailId: number): boolean {
    return this.selectedIds.includes(cartDetailId);
  }

  toggleAll(checked: boolean) {
    this.selectedIds = checked ? this.cartDetails.map(c => c.cartDetailId) : [];
    this.updatePrice();
  }

  toggleItem(cartDetailId: number, checked: boolean) {
    if (checked) {
      if (!this.selectedIds.includes(cartDetailId)) this.selectedIds = [...this.selectedIds, cartDetailId];
    } else {
      this.selectedIds = this.selectedIds.filter(id => id !== cartDetailId);
    }
    this.updatePrice();
  }

  updatePrice() {
    this.totalPrice = this.cartDetails
      .filter(c => this.selectedIds.includes(c.cartDetailId))
      .reduce((acc, c) => acc + c.quantity * (c.salePrice || 0), 0);
  }

  minusQuantity(cartDetailId: number) {
    var item = this.cartDetails.find(c => c.cartDetailId == cartDetailId)
    if (!item)
      return;
    if (item.quantity <= 1)
      return;
    this.totalQuantity -= 1;
    this.cartService.minusQuantity(cartDetailId).subscribe({
      next: (res) => {
        item!.quantity = res;

      },
      error: (e) => this.notifyService.error('Không thể cập nhật số lượng'),
      complete: () => {
        this.updateCommon();
      }
    })
  }

  plusQuantity(cartDetailId: number) {
    var item = this.cartDetails.find(c => c.cartDetailId == cartDetailId)
    if (!item)
      return;
    this.totalQuantity += 1;
    this.cartService.plusQuantity(cartDetailId).subscribe({
      next: (res) => item!.quantity = res,
      error: (e) => this.notifyService.error('Không thể cập nhật số lượng'),
      complete: () => {
        this.updateCommon();
      }
    })
  }

  removeItem(item: CartView) {
    if (!confirm(`Xoá "${item.productName}" khỏi giỏ hàng?`)) return;
    this.cartService.removeItem(item.cartDetailId).subscribe({
      next: () => {
        this.cartDetails = this.cartDetails.filter(c => c.cartDetailId !== item.cartDetailId);
        this.selectedIds = this.selectedIds.filter(id => id !== item.cartDetailId);
        this.updateCommon();
        this.notifyService.success('Đã xoá sản phẩm khỏi giỏ hàng');
      },
      error: () => this.notifyService.error('Không thể xoá sản phẩm khỏi giỏ hàng'),
    });
  }

  updateTotalQuantity() {
    this.totalQuantity = this.cartDetails.reduce((acc, c) => acc + (c.quantity || 0), 0);
  }

  updateCommon() {
    this.updateTotalQuantity();
    this.updatePrice();
    this.cartService.updateCartItems(this.cartDetails)
    this.cdr.detectChanges();
  }

  goToCheckout() {
    let cartDetails = this.cartDetails.filter(c => this.selectedIds.includes(c.cartDetailId));
    if (cartDetails.length > 0) {
      this.router.navigate(['/order'], { state: { cartDetails: cartDetails } });
    }
  }
}
