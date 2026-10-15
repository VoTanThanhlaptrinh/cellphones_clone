import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { InputComponent } from "@shared/form/input/input.component";
import { AutocompleteInputComponent } from "@shared/form/autocomplete-input/autocomplete-input.component";
import { OrderStepHeaderComponent } from '@store/components/order-step-header/order-step-header.component';
import { OrderBottomBarComponent } from '@store/components/order-bottom-bar/order-bottom-bar.component';
import { OrderService } from '@store/services/order.service';
import { StoreService } from '@store/services/store.service';
import { NotifyService } from '@core/services/notify.service';
import { CartView } from '@core/models/cart_request.model';

const isName = (name: string | undefined): name is string => !!name;

/** Trang 2 (giao hàng tận nơi): thông tin người nhận và địa chỉ giao hàng. */
@Component({
  selector: 'app-ship-form',
  imports: [InputComponent, AutocompleteInputComponent, ReactiveFormsModule, OrderStepHeaderComponent, OrderBottomBarComponent],
  templateUrl: './ship-form.component.html',
  styleUrl: './ship-form.component.css',
})
export class ShipFormComponent implements OnInit {
  private router = inject(Router);
  private orderService = inject(OrderService);
  private storeService = inject(StoreService);
  private notifyService = inject(NotifyService);

  readonly group = this.orderService.paymentForm.get('shipInfo') as FormGroup;
  cartDetails: CartView[] = [];
  isLoading = signal(false);

  private city = signal('');
  private district = signal('');

  private readonly provinces = this.storeService.vietNamProvince;
  readonly cities = computed(() => this.provinces().map(p => p.name).filter(isName));
  private readonly districtDtos = computed(() => this.provinces().find(p => p.name === this.city())?.districts ?? []);
  readonly districts = computed(() => this.districtDtos().map(d => d.name).filter(isName));
  readonly wards = computed(() =>
    (this.districtDtos().find(d => d.name === this.district())?.wards ?? []).map(w => w.name).filter(isName));

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
    this.city.set(this.group.value.shipCity ?? '');
    this.district.set(this.group.value.shipDistrict ?? '');
    if (this.provinces().length === 0) {
      this.storeService.initVietnamLocations();
    }
  }

  /** Thông báo lỗi tiếng Việt cho một trường; rỗng nếu chưa chạm vào hoặc hợp lệ. */
  errorFor(name: string): string {
    const control = this.group.get(name);
    if (!control || !control.touched || !control.errors) return '';
    switch (name) {
      case 'receiverName': return 'Vui lòng nhập tên người nhận';
      case 'receiverPhone':
        return control.errors['required']
          ? 'Vui lòng nhập số điện thoại'
          : 'Số điện thoại phải gồm 10 chữ số, bắt đầu bằng 03, 05, 07, 08 hoặc 09';
      case 'shipCity': return 'Vui lòng chọn tỉnh/thành phố';
      case 'shipDistrict': return 'Vui lòng chọn quận/huyện';
      case 'address': return 'Vui lòng nhập số nhà, tên đường';
      default: return '';
    }
  }

  onCityChosen(city: string) {
    this.group.patchValue({ shipCity: city, shipDistrict: '', shipWard: '' });
    this.city.set(city);
    this.district.set('');
  }

  onDistrictChosen(district: string) {
    this.group.patchValue({ shipDistrict: district, shipWard: '' });
    this.district.set(district);
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
