import { ChangeDetectorRef, Component, EventEmitter, inject, Input, OnInit, Output, signal } from '@angular/core';
import { UserView } from '../core/models/user_response.model';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormGroup, FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { InputComponent } from "../shared/custom/input/input.component";
import { OrderService } from '../services/order.service';
import { StoreFormComponent } from "../store-form/store-form.component";
import { ShipFormComponent } from "../ship-form/ship-form.component";
import { CartView } from '../core/models/cart_request.model';
import { PaymentFormData } from '../core/models/payment.model';
import { DeliveryOrderRequest, PickupOrderRequest } from '../core/models/order.model';
import { PaymentService } from '../services/payment.service';
import { NotifyService } from '../services/notify.service';

@Component({
  selector: 'app-payment-infor',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, InputComponent, StoreFormComponent, ShipFormComponent],
  templateUrl: './payment-infor.component.html',
  styleUrl: './payment-infor.component.css',
})
export class PaymentInforComponent implements OnInit {
  paymentForm!: FormGroup;
  private currency = inject(CurrencyPipe);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);
  isLoading = signal<boolean>(false);
  cartDetails: CartView[] = [];
  city: string[] = [];
  districtLabel = 'Chọn tỉnh/thành phố'
  user?: UserView = {
    name: "Võ Tấn Thành", mobile: "0796692184", email: "votanthanh32004@gmail.com"
  };
  constructor(private router: Router, private orderService: OrderService,
    private paymentService: PaymentService,
    private notifyService: NotifyService) { }

  ngOnInit(): void {
    const state = history.state;
    if (state && state.cartDetails) {
      this.cartDetails = state.cartDetails;
    } else {
      this.router.navigate(['/home']);
    }
    this.initForm();
    this.restoreFormData();
    this.listenToDeliveryMethod();
  }
  initForm() {
    this.paymentForm = this.fb.group({
      // 1. Phân loại
      deliveryMethod: ['at-store'],

      // 2. Thông tin dùng chung
      email: [this.user?.email, [Validators.required, Validators.email]],
      // 3. Form Con 1 (Nhận tại quán)
      storeInfo: this.fb.group({
        city: ['', Validators.required],
        district: ['', Validators.required],
        storeId: ['', Validators.required],
        streetName: [''],
        note: [''],
      }),

      // 4. Form Con 2 (Giao hàng)
      shipInfo: this.fb.group({
        receiverName: [this.user?.name, Validators.required],
        receiverPhone: [this.user?.mobile, [Validators.required, Validators.pattern(/(84|0[3|5|7|8|9])+([0-9]{8})\b/)]],
        shipCity: ['', Validators.required],
        shipDistrict: ['', Validators.required],
        shipWard: [''],
        address: ['', Validators.required],
        note: [''],
      })
    });

    // Vừa vào thì tắt Form Giao hàng đi
    this.paymentForm.get('shipInfo')?.disable();
  }
  listenToDeliveryMethod() {
    this.paymentForm.get('deliveryMethod')?.valueChanges.subscribe(method => {
      if (method === 'at-store') {
        this.paymentForm.get('shipInfo')?.disable(); // Tắt validate ship
        this.paymentForm.get('storeInfo')?.enable(); // Bật validate store
      } else {
        this.paymentForm.get('storeInfo')?.disable();
        this.paymentForm.get('shipInfo')?.enable();
      }
    });
  }

  convertPriceVnd(price: number | null | undefined): string {
    if (price == null) return 'Đang cập nhật';
    const formatted = this.currency.transform(price, 'VND', 'symbol', '1.0-0') ?? '';
    return formatted.replace('₫', 'đ');
  }

  deleteMail() {
    this.paymentForm.patchValue({ email: '' });
  }

  goPayment() {
    if (this.paymentForm.invalid) {
      this.paymentForm.markAllAsTouched();
      console.log('Form bị lỗi ở các trường:', this.findInvalidControls());
      return;
    }
    this.saveDataBeforeLeave()
    this.orderService.setAppear(false);
    this.createOrder();
  }

  goCart() {
    this.router.navigate(['/cart']);
  }
  calculateTotals() {
    if (this.cartDetails && this.cartDetails.length > 0) {
      return this.cartDetails.reduce((acc, cart) => acc + ((cart.salePrice || 0) * (cart.quantity || 0)), 0);
    } else {
      return 0;
    }
  }
  public findInvalidControls() {
    const invalid = [];
    // Kiểm tra form cha
    const controls = this.paymentForm.controls;
    for (const name in controls) {
      if (controls[name].invalid) {
        invalid.push(name);
      }
    }

    // Soi chi tiết vào trong storeInfo
    const storeGroup = this.paymentForm.get('storeInfo') as FormGroup;
    if (storeGroup && storeGroup.invalid) {
      for (const name in storeGroup.controls) {
        if (storeGroup.controls[name].invalid) {
          console.log(`Lỗi ở trường con của storeInfo: ${name}`);
        }
      }
    }
    return invalid;
  }
  saveDataBeforeLeave() {
    const formValue: PaymentFormData = this.paymentForm.getRawValue();
    this.orderService.setPaymentData(formValue);
  }

  restoreFormData() {
    const savedData = this.orderService.getPaymentData();

    if (savedData) {
      this.paymentForm.get('deliveryMethod')?.setValue(savedData.deliveryMethod, { emitEvent: false });

      if (savedData.deliveryMethod === 'at-store') {
        this.paymentForm.get('storeInfo')?.enable();
        this.paymentForm.get('shipInfo')?.disable();
      } else {
        this.paymentForm.get('shipInfo')?.enable();
        this.paymentForm.get('storeInfo')?.disable();
      }
      setTimeout(() => {
        this.paymentForm.patchValue(savedData, { emitEvent: false });
        this.cdr.detectChanges();
      }, 0); // 0 miligiây là đủ để Angular đẩy việc này xuống cuối hàng đợi render
    }
  }
  createOrder() {
    this.isLoading.set(true);
    const paymentData = this.orderService.getPaymentData();

    if (!paymentData) {
      this.notifyService.error('Không tìm thấy thông tin giao hàng!');
      this.isLoading.set(false);
      return;
    }

    const cartIds = this.cartDetails.map(c => c.cartDetailId);

    if (paymentData.deliveryMethod === 'at-store') {
      const payload: PickupOrderRequest = {
        cartDetailIds: cartIds,
        storeHouseId: Number(paymentData.storeInfo?.storeId)
      };

      this.orderService.createPickupOrder(payload).subscribe({
        next: (response) => this.orderService.setOrderId(response.data.id),
        error: (err) => this.handleOrderError(err)
      });
    } else {
      const payload: DeliveryOrderRequest = {
        cartDetailIds: cartIds,
        provinceName: paymentData.shipInfo?.shipCity || '',
        districtName: paymentData.shipInfo?.shipDistrict || '',
        street: paymentData.shipInfo?.address || ''
      };

      this.orderService.createDeliveryOrder(payload).subscribe({
        next: (response) => this.orderService.setOrderId(response.data.id),
        error: (err) => this.handleOrderError(err)
      });
    }
  }

  private handleOrderError(err: any) {
    this.notifyService.error(err.error?.message || 'Có lỗi xảy ra khi tạo đơn hàng');
    this.isLoading.set(false);
  }

} 
