import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../services/cart.service';
import { CartView } from '../core/models/cart_request.model';
import { CurrencyPipe, NgClass } from '@angular/common';
import { HeaderCartOrderComponent } from "../header-cart-order/header-cart-order.component";
import { NotifyService } from '../services/notify.service';

@Component({
  selector: 'app-cart',
  imports: [NgClass, HeaderCartOrderComponent],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css',
})
export class CartComponent implements OnInit {
  page: number = 1;
  totalPrice: number = 0;
  totalQuantity: number = 0;
  selectedItems: number[] = [];
  allSelected: boolean = false;
  isDisable = true;
  cartDetails: CartView[] = [];
  isLoadding: boolean = true;
  hasMore: boolean = false;
  private currency = inject(CurrencyPipe);
  constructor(private cartService: CartService, private router: Router,
    private cdr: ChangeDetectorRef,
    private notifyService: NotifyService
  ) {

  }
  ngOnInit(): void {
    this.cartService.getList(this.page).subscribe({
      next: res => {
        this.cartDetails = res.data
      },
      error: err => this.notifyService.error('Có lỗi xảy ra khi tải giỏ hàng'),
      complete: () => {
        this.isLoadding = false;
        this.updateCommon();
      }
    })
  }
  convertPriceVnd(price: number | null | undefined): string {
    if (price == null) return 'Đang cập nhật';
    const formatted = this.currency.transform(price, 'VND', 'symbol', '1.0-0') ?? '';
    return formatted.replace('₫', 'đ');
  }
  goProductDetail(id: number) {
    this.router.navigate(['/product', id]);
  }
  selectAll(event: any) {
    this.isDisable = !this.isDisable;
    if (event.target.checked)
      this.selectedItems = this.cartDetails.map((_, index) => index);
    else
      this.selectedItems = [];
    this.updatePrice();
  }
  selectItem(event: any, index: number) {
    var isSelected = event.target.checked;
    if (isSelected) {
      this.selectedItems.push(index);
      this.isDisable = !isSelected;
    } else {
      this.selectedItems = this.selectedItems.filter(i => i != index);
    }
    this.allSelected = this.selectedItems.length != 0;
    this.isDisable = this.selectedItems.length == 0
    this.updatePrice();
  }
  updatePrice() {
    this.totalPrice = this.selectedItems.map(i => this.cartDetails[i]).reduce((acc, c) => acc + c.quantity * (c.salePrice || 0), 0);
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
        item!.quantity = res.data;

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
      next: (res) => item!.quantity = res.data,
      error: (e) => this.notifyService.error('Không thể cập nhật số lượng'),
      complete: () => {
        this.updateCommon();
      }
    })
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
}
