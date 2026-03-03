import { Component, inject, Input, OnInit } from '@angular/core';
import { InputComponent } from "../shared/custom/input/input.component";
import { AutocompleteInputComponent } from "../shared/custom/autocomplete-input/autocomplete-input.component";

import { FormGroup } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-ship-form',
  imports: [InputComponent, AutocompleteInputComponent],
  templateUrl: './ship-form.component.html',
  styleUrl: './ship-form.component.css',
})
export class ShipFormComponent implements OnInit {
  @Input() group!: FormGroup | any;
  private http = inject(HttpClient);

  // Lưu trữ toàn bộ dữ liệu gốc từ API
  vnLocations: any[] = [];

  // Các mảng string để truyền vào Input
  cities: string[] = [];
  districts: string[] = [];
  wards: string[] = [];
  selectedProvince: any = null;
  ngOnInit() {
    this.fetchVietNamLocations();
  }

  fetchVietNamLocations() {
    // Đổi từ 'https://provinces.open-api.vn/api/?depth=3' thành:
    const apiUrl = '/api/provinces/?depth=3';

    this.http.get<any[]>(apiUrl).subscribe({
      next: (data) => {
        this.vnLocations = data;
        this.cities = data.map(p => p.name);
      },
      error: (err) => {
        console.error('Lỗi khi tải dữ liệu địa giới hành chính:', err);
      }
    });
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
