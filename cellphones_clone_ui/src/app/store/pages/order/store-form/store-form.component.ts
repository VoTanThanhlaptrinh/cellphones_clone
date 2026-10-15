import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AutocompleteInputComponent } from "@shared/form/autocomplete-input/autocomplete-input.component";
import { InputComponent } from "@shared/form/input/input.component";
import { OrderStepHeaderComponent } from '@store/components/order-step-header/order-step-header.component';
import { OrderBottomBarComponent } from '@store/components/order-bottom-bar/order-bottom-bar.component';
import { OrderService } from '@store/services/order.service';
import { StoreService } from '@store/services/store.service';
import { NotifyService } from '@core/services/notify.service';
import { CartView } from '@core/models/cart_request.model';

/** Trang 2 (nhận tại cửa hàng): chọn tỉnh/thành, quận/huyện rồi cửa hàng nhận máy. */
@Component({
  selector: 'app-store-form',
  imports: [AutocompleteInputComponent, InputComponent, ReactiveFormsModule, OrderStepHeaderComponent, OrderBottomBarComponent],
  templateUrl: './store-form.component.html',
  styleUrl: './store-form.component.css',
  standalone: true
})
export class StoreFormComponent implements OnInit {
  private router = inject(Router);
  private orderService = inject(OrderService);
  private storeService = inject(StoreService);
  private notifyService = inject(NotifyService);

  readonly group = this.orderService.paymentForm.get('storeInfo') as FormGroup;
  cartDetails: CartView[] = [];
  isLoading = signal(false);

  private city = signal('');
  private district = signal('');

  readonly cityList = computed(() => [...new Set(this.storeService.storeViews().map(s => s.city))]);
  readonly districtList = computed(() => {
    const store = this.storeService.storeViews().find(s => s.city === this.city());
    return store ? [...new Set(store.districts.map(d => d.district))] : [];
  });
  readonly streetList = computed(() => {
    const store = this.storeService.storeViews().find(s => s.city === this.city());
    const district = store?.districts.find(d => d.district === this.district());
    return district ? [...new Set(district.streets.map(s => s.street))] : [];
  });

  get total(): number {
    return this.orderService.totalOf(this.cartDetails);
  }

  ngOnInit() {
    const state = history.state;
    if (!state?.cartDetails) {
      this.router.navigate(['/home']);
      return;
    }
    this.cartDetails = state.cartDetails;
    // Chưa qua bước nhập email (tải lại trang, gõ thẳng URL) thì về bước 1.
    if (this.orderService.paymentForm.get('email')?.invalid) {
      this.goBack();
      return;
    }
    this.city.set(this.group.value.city ?? '');
    this.district.set(this.group.value.district ?? '');
    this.storeService.initStoreView();
  }

  /** Thông báo lỗi tiếng Việt; lỗi `storeId` hiển thị dưới ô chọn cửa hàng. */
  errorFor(name: string): string {
    const control = this.group.get(name);
    if (!control || !control.touched || !control.errors) return '';
    switch (name) {
      case 'city': return 'Vui lòng chọn tỉnh/thành phố';
      case 'district': return 'Vui lòng chọn quận/huyện';
      case 'storeId': return 'Vui lòng chọn cửa hàng nhận hàng';
      default: return '';
    }
  }

  onCityChosen(city: string) {
    this.group.patchValue({ city, district: '', storeId: '', streetName: '' });
    this.city.set(city);
    this.district.set('');
  }

  onDistrictChosen(district: string) {
    this.group.patchValue({ district, storeId: '', streetName: '' });
    this.district.set(district);
  }

  onStreetChosen(street: string) {
    const store = this.storeService.storeViews().find(s => s.city === this.city());
    const district = store?.districts.find(d => d.district === this.district());
    const storeId = district?.streets.find(s => s.street === street)?.id ?? '';
    this.group.patchValue({ storeId, streetName: street });
    // Ô cửa hàng bind vào `streetName`, còn lỗi nằm ở `storeId`: đánh dấu để lỗi hiện đúng lúc.
    this.group.get('storeId')?.markAsTouched();
  }

  goBack() {
    this.router.navigate(['/order'], { state: { cartDetails: this.cartDetails } });
  }

  goNext() {
    if (this.group.invalid) {
      this.group.markAllAsTouched();
      return;
    }
    this.isLoading.set(true);
    this.orderService.submitOrder(this.cartDetails).subscribe({
      next: () => {
        this.router.navigate(['/order', 'payment'], { state: { cartDetails: this.cartDetails } });
      },
      error: (err) => {
        this.notifyService.error(err.error?.message || err.message || 'Có lỗi xảy ra khi tạo đơn hàng');
        this.isLoading.set(false);
      }
    });
  }
}
