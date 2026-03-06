import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { NotifyService } from './notify.service';
import { ApiResponse } from './auth.service';
import { StoreView, VietnamProvinceDto } from '../core/models/store.model';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root',
})
export class StoreService {
  private baseUrl = environment.apiUrl;
  vietNamProvince = signal<VietnamProvinceDto[]>([])
  storeViews = signal<StoreView[]>([]);
  constructor(private http: HttpClient, private notifyServce: NotifyService) {
  }
  initStoreView() {
    if (this.storeViews().length === 0) {
      this.http.get<ApiResponse<StoreView[]>>(`${this.baseUrl}/stores/views`).subscribe({
        next: (res) => {
          this.storeViews.set(res.data)
        },
        error: (err) => console.error('Lỗi lấy cửa hàng:', err)
      });
    }
  }
  initVietnamLocations(depth: number = 3) {
    let params = new HttpParams().set('depth', depth.toString());
    this.http.get<ApiResponse<VietnamProvinceDto[]>>(`${this.baseUrl}/stores/provinces`, { params }).subscribe({
      next: (res) => this.vietNamProvince.set(res.data)
    });

  }
}
