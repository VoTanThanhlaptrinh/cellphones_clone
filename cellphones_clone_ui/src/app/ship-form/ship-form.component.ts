import { Component, effect, inject, Input, OnInit } from '@angular/core';
import { InputComponent } from "../shared/custom/input/input.component";
import { AutocompleteInputComponent } from "../shared/custom/autocomplete-input/autocomplete-input.component";

import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { VietnamProvinceDto } from '../core/models/store.model';
import { StoreService } from '../services/store.service';

@Component({
  selector: 'app-ship-form',
  imports: [InputComponent, AutocompleteInputComponent, ReactiveFormsModule],
  templateUrl: './ship-form.component.html',
  styleUrl: './ship-form.component.css',
})
export class ShipFormComponent implements OnInit {
  @Input() group!: FormGroup | any;

  // Lưu trữ toàn bộ dữ liệu gốc từ API
  vnLocations: VietnamProvinceDto[] = [];
  constructor(private storeService: StoreService) {
    effect(() => {
      this.vnLocations = this.storeService.vietNamProvince()
      if (this.vnLocations && this.vnLocations.length > 0) {
        // Giả sử province model của bạn có thuộc tính 'name'
        this.cities = this.vnLocations
          .map(province => province.name)
          .filter((name): name is string => !!name); // Lọc bỏ các giá trị null/undefined nếu có
      }
    })
  }
  // Các mảng string để truyền vào Input
  cities: string[] = [];
  districts: string[] = [];
  wards: string[] = [];
  selectedProvince: any = null;
  ngOnInit() {
    this.fetchVietNamLocations();
  }

  fetchVietNamLocations() {
    if (this.storeService.vietNamProvince().length === 0) {
      this.storeService.initVietnamLocations()
    }
  }

  onCityChosen(event: any) {
    // 1. Ép Form cập nhật ngay lập tức để đồng bộ UI
    this.group.patchValue({
      shipCity: event,
      shipDistrict: '',
      shipWard: ''
    });

    // 2. Tìm và BỎ TÚI LUÔN object Tỉnh vào biến class
    this.selectedProvince = this.vnLocations.find(p => p.name === event);

    // 3. Lấy danh sách Quận từ biến đã bỏ túi
    this.districts = this.selectedProvince
      ? this.selectedProvince.districts.map((d: any) => d.name)
      : [];

    // 4. Reset mảng Phường/Xã
    this.wards = [];
  }

  onDistrictChosen(event: any) {
    // 1. Ép Form cập nhật Quận
    this.group.patchValue({ shipDistrict: event, shipWard: '' });

    // 2. TÌM QUẬN TỪ BIẾN TỈNH ĐÃ LƯU (Trực tiếp bỏ qua bước hỏi Form)
    const selectedDistrict = this.selectedProvince?.districts.find((d: any) => d.name === event);

    // 3. Cập nhật danh sách Phường/Xã
    this.wards = selectedDistrict && selectedDistrict.wards
      ? selectedDistrict.wards.map((w: any) => w.name)
      : [];
  }

  onWardChosen(event: any) {
    // Ép Form cập nhật Phường
    this.group.patchValue({ shipWard: event });
  }
}
