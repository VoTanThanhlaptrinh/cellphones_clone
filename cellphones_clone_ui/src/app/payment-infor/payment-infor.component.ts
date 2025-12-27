import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { UserView } from '../core/models/user_response.model';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormGroup, FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AutocompleteInputComponent } from "../shared/custom/autocomplete-input/autocomplete-input.component";
import { Router } from '@angular/router';
import { CartView } from '../core/models/cart_request.model';
import { CheckoutService, PaymentInfo } from '../services/checkout.service';
import { StoreView } from '../core/models/store.model';
import { CartService } from '../services/cart.service';

import { InputComponent } from "../shared/custom/input/input.component";

@Component({
  selector: 'app-payment-infor',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, AutocompleteInputComponent, InputComponent],
  templateUrl: './payment-infor.component.html',
  styleUrl: './payment-infor.component.css',
})
export class PaymentInforComponent implements OnInit {
  @Input() storeViews?: StoreView[]

  get deliveryMethod() {
    return this.paymentInfoForm?.get('deliveryMethod')?.value;
  }

  @Output() isPaymentInfor = new EventEmitter<boolean>();

  paymentInfoForm!: FormGroup;

  cartItem?: CartView[]
  private currency = inject(CurrencyPipe);
  private fb = inject(FormBuilder);
  private checkoutService = inject(CheckoutService);

  districtLabel = 'Chọn tỉnh/thành phố'
  origins: string[] = ['Khánh Hòa', 'HN', 'HCM', 'DN', 'CT', 'HP']

  user?: UserView = {
    name: "Võ Tấn Thành", mobile: "0796692184", email: "votanthanh32004@gmail.com"
  };

  constructor(private router: Router, private cartService: CartService) { }

  ngOnInit(): void {
    this.initForm();
  }

  initForm() {
    this.paymentInfoForm = this.fb.group({
      name: [this.user?.name, [Validators.required]],
      phone: [this.user?.mobile, [Validators.required, Validators.pattern(/(84|0[3|5|7|8|9])+([0-9]{8})\b/)]],
      email: [this.user?.email, [Validators.required, Validators.email]],
      deliveryMethod: ['at-store', [Validators.required]],
      // At store fields
      city: [''],
      district: [''],
      storeId: [''],
      // Ship key fields
      receiverName: [''],
      receiverPhone: [''],
      shipCity: [''],
      shipDistrict: [''],
      shipWard: [''],
      address: [''],
      note: [''],
      // Invoice
      requestInvoice: [false],
      companyName: [''],
      companyAddress: [''],
      taxCode: ['']
    });

    // Subscribe to delivery method changes to validation logic if needed
    this.paymentInfoForm.get('deliveryMethod')?.valueChanges.subscribe(val => {
      this.updateValidators(val);
    });
    this.updateValidators('at-store'); // Initial state
  }

  updateValidators(method: string) {
    const cityControl = this.paymentInfoForm.get('city');
    const districtControl = this.paymentInfoForm.get('district');
    const storeIdControl = this.paymentInfoForm.get('storeId');

    const receiverNameControl = this.paymentInfoForm.get('receiverName');
    const receiverPhoneControl = this.paymentInfoForm.get('receiverPhone');
    const shipCityControl = this.paymentInfoForm.get('shipCity');
    const shipDistrictControl = this.paymentInfoForm.get('shipDistrict');
    const addressControl = this.paymentInfoForm.get('address');

    if (method === 'at-store') {
      cityControl?.setValidators([Validators.required]);
      districtControl?.setValidators([Validators.required]);
      storeIdControl?.setValidators([Validators.required]);

      receiverNameControl?.clearValidators();
      receiverPhoneControl?.clearValidators();
      shipCityControl?.clearValidators();
      shipDistrictControl?.clearValidators();
      addressControl?.clearValidators();
    } else {
      cityControl?.clearValidators();
      districtControl?.clearValidators();
      storeIdControl?.clearValidators();

      receiverNameControl?.setValidators([Validators.required]);
      receiverPhoneControl?.setValidators([Validators.required, Validators.pattern(/(84|0[3|5|7|8|9])+([0-9]{8})\b/)]);
      shipCityControl?.setValidators([Validators.required]);
      shipDistrictControl?.setValidators([Validators.required]);
      addressControl?.setValidators([Validators.required]);
    }

    // Refresh validity
    cityControl?.updateValueAndValidity();
    districtControl?.updateValueAndValidity();
    storeIdControl?.updateValueAndValidity();
    receiverNameControl?.updateValueAndValidity();
    receiverPhoneControl?.updateValueAndValidity();
    shipCityControl?.updateValueAndValidity();
    shipDistrictControl?.updateValueAndValidity();
    addressControl?.updateValueAndValidity();
  }

  convertPriceVnd(price: number | null | undefined): string {
    if (price == null) return 'Đang cập nhật';
    const formatted = this.currency.transform(price, 'VND', 'symbol', '1.0-0') ?? '';
    return formatted.replace('₫', 'đ');
  }

  deleteMail() {
    this.paymentInfoForm.patchValue({ email: '' });
  }

  goPayment() {
    if (this.paymentInfoForm.invalid) {
      this.paymentInfoForm.markAllAsTouched();
      return;
    }

    const val = this.paymentInfoForm.value;
    let address = '';
    let name = this.user?.name || '';
    let phone = this.user?.mobile || '';

    if (val.deliveryMethod === 'at-store') {
      address = `Cửa hàng: ${val.storeId || ''}, ${val.district}, ${val.city}`;
    } else {
      address = `${val.address}, ${val.shipWard}, ${val.shipDistrict}, ${val.shipCity}`;
      name = val.receiverName;
      phone = val.receiverPhone;
    }

    const info: PaymentInfo = {
      name: name,
      phone: phone,
      email: val.email || this.user?.email || '',
      address: address,
      deliveryMethod: val.deliveryMethod,
      note: val.note,
      city: val.deliveryMethod === 'at-store' ? val.city : val.shipCity,
      district: val.deliveryMethod === 'at-store' ? val.district : val.shipDistrict
    };

    this.checkoutService.updatePaymentInfo(info);
    this.isPaymentInfor.emit(false);
  }

  goCart() {
    this.router.navigate(['/cart']);
  }
}
