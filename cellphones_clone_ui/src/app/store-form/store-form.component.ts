import { Component, effect, Input, OnInit } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
// import { StoreView } from '../core/models/store.model'; // Mở lại nếu cần
import { AutocompleteInputComponent } from "../shared/custom/autocomplete-input/autocomplete-input.component";
import { InputComponent } from "../shared/custom/input/input.component";
import { OrderService } from '../services/order.service';

@Component({
  selector: 'app-store-form',
  imports: [AutocompleteInputComponent, InputComponent, ReactiveFormsModule],
  templateUrl: './store-form.component.html',
  styleUrl: './store-form.component.css',
  standalone: true
})
export class StoreFormComponent implements OnInit {
  @Input() group!: FormGroup | any;

  cityList: string[] = [];
  districtList: string[] = [];
  streetList: string[] = [];

  constructor(private orderService: OrderService) {
    effect(() => {
      const stores = this.orderService.storeViews();

      if (stores && stores.length > 0) {
        this.cityList = [...new Set(stores.map(s => s.city))];
        const storeInfo = this.orderService?.getPaymentData()?.storeInfo

        const savedCity = storeInfo?.city;
        if (savedCity) {
          const selectedStore = stores.find(s => s.city === savedCity);
          this.districtList = selectedStore ? [...new Set(selectedStore.districts.map(d => d.district))] : [];
        }

        const savedDistrict = storeInfo?.district;
        if (savedCity && savedDistrict) {
          const selectedStore = stores.find(s => s.city === savedCity);
          const selectedDistrict = selectedStore?.districts.find(d => d.district === savedDistrict);
          this.streetList = selectedDistrict ? [...new Set(selectedDistrict.streets.map((s: any) => s.street))] : [];
        }
      }
    });
  }

  ngOnInit() {
    this.orderService.initStoreView();
  }

  onCityChosen(event: any) {
    this.group.patchValue({ city: event, district: '', storeId: '', streetName: '' });

    const stores = this.orderService.storeViews();
    const selectedStore = stores.find(s => s.city === event);

    this.districtList = selectedStore ? [...new Set(selectedStore.districts.map(d => d.district))] : [];
    this.streetList = [];
  }

  onDistrictChosen(event: any) {
    this.group.patchValue({ district: event, storeId: '', streetName: '' });

    const currentCity = this.group.get('city')?.value;
    const stores = this.orderService.storeViews();

    const selectedStore = stores.find(s => s.city === currentCity);
    const selectedDistrict = selectedStore?.districts.find(d => d.district === event);

    this.streetList = selectedDistrict ? [...new Set(selectedDistrict.streets.map((s: any) => s.street))] : [];
  }

  onStreetChosen(event: any) {
    const currentCity = this.group.get('city')?.value;
    const currentDistrict = this.group.get('district')?.value;
    const stores = this.orderService.storeViews();

    const selectedStore = stores.find(s => s.city === currentCity);
    const selectedDistrict = selectedStore?.districts.find(d => d.district === currentDistrict);

    const storeId = selectedDistrict?.streets.find((s: any) => s.street === event)?.id;
    this.group.patchValue({ storeId: storeId, streetName: event });
  }
}