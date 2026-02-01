import { AfterViewInit, Component, ElementRef, HostListener, NgModule, OnInit, ViewChild } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ProductService } from '../services/product.service';
import { ProductView } from '../core/models/product.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, Subscription, switchMap } from 'rxjs';


@Component({
  selector: 'app-header',
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})

export class HeaderComponent implements OnInit {
  keyword: string = '';
  private searchSubject = new Subject<string>();
  private searchSubscription?: Subscription;
  isCalled = false;
  showTab = false;
  i = 0;
  useSearch = false;
  searchResults: ProductView[] = [];
  constructor(private router: Router, private productService: ProductService) {
  }
  categories = [
    {
      name: ['Điện thoại,', 'Tablet'],
      src: 'https://img.icons8.com/ios/50/smartphone--v2.png',
      tabs: [
        {
          title: 'Hãng điện thoại',
          items: [
            { name: 'iPhone', badge: null },
            { name: 'Samsung', badge: null },
            { name: 'Xiaomi', badge: null },
            { name: 'OPPO', badge: null },
            { name: 'realme', badge: null },
            { name: 'TECNO', badge: null },
            { name: 'Honor', badge: null },
            { name: 'vivo', badge: null },
            { name: 'Infinix', badge: null },
            { name: 'Nokia', badge: null },
            { name: 'Nubia', badge: null },
            { name: 'Nothing Phone', badge: null },
            { name: 'Masstel', badge: null },
          ],
        },
        {
          title: 'Mức giá điện thoại',
          items: [
            { name: 'Dưới 2 triệu', badge: null },
            { name: 'Từ 2 - 4 triệu', badge: null },
            { name: 'Từ 4 - 7 triệu', badge: null },
            { name: 'Từ 7 - 13 triệu', badge: null },
            { name: 'Từ 13 - 20 triệu', badge: null },
            { name: 'Trên 20 triệu', badge: null },
          ],
        },
        {
          title: 'Điện thoại HOT',
          items: [
            { name: 'iPhone 17', badge: 'MỚI' },
            { name: 'iPhone Air', badge: 'MỚI' },
            { name: 'iPhone 16', badge: null },
            { name: 'Galaxy Z Fold7', badge: 'MỚI' },
            { name: 'S25 Ultra', badge: null },
            { name: 'OPPO Reno14', badge: null },
            { name: 'Xiaomi 15T', badge: null },
            { name: 'OPPO Find N5', badge: null },
            { name: 'Samsung Galaxy S25 FE', badge: null },
            { name: 'Redmi Note 14', badge: null },
            { name: 'Galaxy Z Flip7', badge: 'MỚI' },
            { name: 'iPhone 17 Pro', badge: 'HOT' },
            { name: 'Tecno Pova 7', badge: null },
          ],
        },
        {
          title: 'Hãng máy tính bảng',
          items: [
            { name: 'iPad', badge: null },
            { name: 'Samsung', badge: null },
            { name: 'Xiaomi', badge: null },
            { name: 'Huawei', badge: null },
            { name: 'Lenovo', badge: null },
            { name: 'HONOR', badge: null },
            { name: 'Teclast', badge: null },
            { name: 'Máy đọc sách', badge: null },
            { name: 'Kindle', badge: null },
            { name: 'Boox', badge: null },
            { name: 'Xem thêm tất cả Tablet', badge: null },
          ],
        },
        {
          title: 'Máy tính bảng HOT',
          items: [
            { name: 'iPad Air M3', badge: 'HOT' },
            { name: 'iPad A16', badge: 'HOT' },
            { name: 'iPad Air 2024', badge: null },
            { name: 'iPad Pro 2024', badge: null },
            { name: 'iPad mini 7', badge: null },
            { name: 'Galaxy Tab S11 Series', badge: null },
            { name: 'Galaxy Tab S10 Series', badge: null },
            { name: 'Lenovo Idea Tab Wifi', badge: null },
            { name: 'Xiaomi Redmi Pad 2', badge: null },
            { name: 'Huawei MatePad Pro 2025', badge: null },
            { name: 'Teclast Wifi P30', badge: null },
            { name: 'HONOR Pad X7', badge: 'MỚI' },
          ],
        },
      ],
    },
    {
      name: ['Laptop'],
      src: 'https://img.icons8.com/material-outlined/24/laptop.png',
      tabs: [
        {
          title: 'Thương hiệu',
          items: [
            { name: 'Mac', badge: null },
            { name: 'ASUS', badge: null },
            { name: 'Lenovo', badge: null },
            { name: 'Dell', badge: null },
            { name: 'HP', badge: null },
            { name: 'Acer', badge: null },
            { name: 'LG', badge: null },
            { name: 'MSI', badge: null },
            { name: 'Gigabyte', badge: null },
            { name: 'Masstel', badge: null },
          ],
        },
        {
          title: 'Phân khúc giá',
          items: [
            { name: 'Dưới 10 triệu', badge: null },
            { name: 'Từ 10 - 15 triệu', badge: null },
            { name: 'Từ 15 - 20 triệu', badge: null },
            { name: 'Từ 20 - 25 triệu', badge: null },
            { name: 'Từ 25 - 30 triệu', badge: null },
          ],
        },
        {
          title: 'Nhu cầu sử dụng',
          items: [
            { name: 'Văn phòng', badge: null },
            { name: 'Gaming', badge: null },
            { name: 'Mỏng nhẹ', badge: null },
            { name: 'Đồ họa - kỹ thuật', badge: null },
            { name: 'Sinh viên', badge: null },
            { name: 'Cảm ứng', badge: null },
            { name: 'Laptop AI', badge: 'MỚI' },
            { name: 'Mac CTO - Nâng cấp theo cách của bạn', badge: 'HOT' },
          ],
        },
        {
          title: 'Dòng chip',
          items: [
            { name: 'Laptop Core i3', badge: null },
            { name: 'Laptop Core i5', badge: null },
            { name: 'Laptop Core i7', badge: null },
            { name: 'Laptop Core i9', badge: null },
            { name: 'Apple M1 Series', badge: null },
            { name: 'Apple M3 Series', badge: null },
            { name: 'Apple M4 Series', badge: null },
            { name: 'AMD Ryzen', badge: null },
            { name: 'Intel Core Ultra', badge: 'HOT' },
          ],
        },
        {
          title: 'Kích thước màn hình',
          items: [
            { name: 'Laptop 13 inch', badge: null },
            { name: 'Laptop 14 inch', badge: null },
            { name: 'Laptop 15.6 inch', badge: null },
            { name: 'Laptop 16 inch', badge: null },
          ],
        },
      ],
    },
    {
      name: ['Âm thanh,', 'Mic thu âm'],
      src: 'https://cellphones.com.vn/media/icons/menu/icon-cps-220.svg',
      tabs: [
        {
          title: 'Chọn loại tai nghe',
          items: [
            { name: 'Bluetooth', badge: null },
            { name: 'Chụp tai', badge: null },
            { name: 'Nhét tai', badge: null },
            { name: 'Có dây', badge: null },
            { name: 'Thể thao', badge: null },
            { name: 'Gaming', badge: null },
            { name: 'Xem tất cả tai nghe', badge: null },
          ],
        },
        {
          title: 'Mic',
          items: [
            { name: 'Mic cài áo', badge: null },
            { name: 'Mic phòng thu, podcast', badge: null },
            { name: 'Mic livestream', badge: null },
            { name: 'Micro không dây', badge: null },
          ],
        },
        {
          title: 'Hãng tai nghe',
          items: [
            { name: 'Apple', badge: null },
            { name: 'Sony', badge: null },
            { name: 'JBL', badge: null },
            { name: 'Samsung', badge: null },
            { name: 'Marshall', badge: null },
            { name: 'Soundpeats', badge: null },
            { name: 'Bose', badge: null },
            { name: 'Edifier', badge: null },
            { name: 'Xiaomi', badge: null },
            { name: 'Huawei', badge: null },
            { name: 'Sennheiser', badge: null },
            { name: 'Havit', badge: null },
            { name: 'Beats', badge: null },
          ],
        },
        {
          title: 'Chọn theo giá',
          items: [
            { name: 'Tai nghe dưới 200K', badge: null },
            { name: 'Tai nghe dưới 500K', badge: null },
            { name: 'Tai nghe dưới 1 triệu', badge: null },
            { name: 'Tai nghe dưới 2 triệu', badge: null },
            { name: 'Tai nghe dưới 5 triệu', badge: null },
          ],
        },
        {
          title: 'Chọn loại loa',
          items: [
            { name: 'Loa Bluetooth', badge: null },
            { name: 'Loa Karaoke', badge: null },
            { name: 'Loa kéo', badge: null },
            { name: 'Loa Soundbar', badge: null },
            { name: 'Loa vi tính', badge: null },
            { name: 'Xem tất cả loa', badge: null },
          ],
        },
        {
          title: 'Hãng loa',
          items: [
            { name: 'JBL', badge: null },
            { name: 'Marshall', badge: null },
            { name: 'Harman Kardon', badge: null },
            { name: 'Acnos', badge: null },
            { name: 'Samsung', badge: null },
            { name: 'Sony', badge: null },
            { name: 'Arirang', badge: null },
            { name: 'LG', badge: null },
            { name: 'Alpha Works', badge: null },
            { name: 'Edifier', badge: null },
            { name: 'Bose', badge: null },
            { name: 'Tronsmart', badge: null },
          ],
        },
        {
          title: 'Sản phẩm nổi bật',
          items: [
            { name: 'AirPods Pro 3', badge: 'MỚI' },
            { name: 'AirPods 4', badge: null },
            { name: 'Galaxy Buds 3 pro', badge: null },
            { name: 'JBL Tune Buds 2 - Chỉ có tại CellphoneS', badge: null },
            { name: 'Sony WH-1000XM6', badge: null },
            {
              name: 'OPPO Enco Air 2 Pro - Chỉ có tại CellphoneS',
              badge: 'HOT',
            },
            { name: 'Redmi Buds 6 Pro', badge: null },
            { name: 'Onyx Studio 8', badge: null },
            { name: 'Marshall Kilburn III', badge: null },
            { name: 'JBL Partybox Encore 2', badge: null },
            { name: 'Galaxy Buds Core', badge: 'MỚI' },
            { name: 'Xiaomi OpenWear Stereo', badge: null },
          ],
        },
      ],
    },
    {
      name: ['Đồng hồ,', ' Camera'],
      src: 'https://cellphones.com.vn/media/icons/menu/icon-cps-610.svg',
      tabs: [
        {
          title: 'Loại đồng hồ',
          items: [
            { name: 'Đồng hồ thông minh', badge: null },
            { name: 'Vòng đeo tay thông minh', badge: null },
            { name: 'Đồng hồ định vị trẻ em', badge: null },
            { name: 'Dây đeo', badge: null },
          ],
        },
        {
          title: 'Chọn theo thương hiệu',
          items: [
            { name: 'Apple Watch', badge: null },
            { name: 'Samsung', badge: null },
            { name: 'Xiaomi', badge: null },
            { name: 'Huawei', badge: null },
            { name: 'Coros', badge: null },
            { name: 'Garmin', badge: null },
            { name: 'Kieslect', badge: null },
            { name: 'Amazfit', badge: null },
            { name: 'Black Shark', badge: null },
            { name: 'Mibro', badge: null },
            { name: 'Masstel', badge: null },
            { name: 'imoo', badge: null },
            { name: 'Kospet', badge: null },
          ],
        },
        {
          title: 'Sản phẩm nổi bật',
          items: [
            { name: 'Apple Watch Series 11', badge: 'HOT' },
            { name: 'Apple Watch SE 3', badge: 'MỚI' },
            { name: 'Apple Watch Ultra 3', badge: 'MỚI' },
            { name: 'Samsung Galaxy Watch 8', badge: 'HOT' },
            { name: 'Samsung Galaxy Watch 8 Classic', badge: null },
            { name: 'Samsung Galaxy Watch Ultra', badge: null },
            { name: 'Huawei Watch Fit 4', badge: null },
            { name: 'Huawei Watch Fit 4 Pro', badge: null },
            { name: 'ELFDIGI DINO 1', badge: 'MỚI' },
            { name: 'Viettel MyKID 4G Lite', badge: null },
            { name: 'Garmin Forerunner 165', badge: null },
            { name: 'Xiaomi Watch S4 41mm', badge: 'MỚI' },
          ],
        },
        {
          title: 'Camera',
          items: [
            { name: 'Camera an ninh', badge: null },
            { name: 'Camera hành trình', badge: null },
            { name: 'Action Camera', badge: null },
            { name: 'Camera AI', badge: 'MỚI' },
            { name: 'Gimbal', badge: null },
            { name: 'Tripod', badge: null },
            { name: 'Máy ảnh', badge: null },
            { name: 'Flycam', badge: null },
            { name: 'Xem tất cả camera', badge: null },
          ],
        },
        {
          title: 'Camera nổi bật',
          items: [
            { name: 'Camera an ninh Imou', badge: null },
            { name: 'Camera an ninh Ezviz', badge: null },
            { name: 'Camera an ninh Xiaomi', badge: null },
            { name: 'Camera an ninh TP-Link', badge: null },
            { name: 'Camera Tiandy', badge: 'HOT' },
            { name: 'Camera DJI', badge: null },
            { name: 'Camera Insta360', badge: null },
            { name: 'Máy ảnh Fujifilm', badge: null },
            { name: 'Máy ảnh Canon', badge: 'HOT' },
            { name: 'Máy ảnh Sony', badge: 'HOT' },
            { name: 'Gopro Hero 13', badge: null },
            { name: 'Flycam dji', badge: null },
            { name: 'DJI Action 5 Pro', badge: null },
          ],
        },
      ],
    },
    {
      name: ['Đồ gia dụng'],
      src: 'https://cellphones.com.vn/media/icons/menu/icon-cps-845.svg',
      tabs: [
        {
          title: 'Gia dụng nhà bếp',
          items: [
            { name: 'Nồi chiên không dầu', badge: 'HOT' },
            { name: 'Máy rửa bát', badge: null },
            { name: 'Lò vi sóng', badge: null },
            { name: 'Nồi cơm điện', badge: 'HOT' },
            { name: 'Máy xay sinh tố', badge: null },
            { name: 'Máy ép trái cây', badge: null },
            { name: 'Máy làm sữa hạt', badge: null },
            { name: 'Bếp điện', badge: null },
            { name: 'Ấm siêu tốc', badge: null },
            { name: 'Nồi áp suất', badge: null },
            { name: 'Nồi nấu chậm', badge: null },
            { name: 'Nồi lẩu điện', badge: null },
          ],
        },
        {
          title: 'Sức khỏe - Làm đẹp',
          items: [
            { name: 'Máy đo huyết áp', badge: null },
            { name: 'Máy sấy tóc', badge: null },
            { name: 'Máy massage', badge: null },
            { name: 'Máy cạo râu', badge: null },
            { name: 'Cân sức khỏe', badge: null },
            { name: 'Bàn chải điện', badge: null },
            { name: 'Máy tăm nước', badge: null },
            { name: 'Tông đơ cắt tóc', badge: null },
            { name: 'Máy tỉa lông mũi', badge: null },
            { name: 'Máy rửa mặt', badge: null },
            { name: 'Máy tạo kiểu tóc', badge: null },
            { name: 'Máy triệt lông', badge: null },
          ],
        },
        {
          title: 'Thiết bị gia đình',
          items: [
            { name: 'Robot hút bụi', badge: null },
            { name: 'Máy lọc không khí', badge: null },
            { name: 'Quạt', badge: null },
            { name: 'Máy hút bụi cầm tay', badge: null },
            { name: 'Máy rửa chén', badge: null },
            { name: 'TV Box', badge: null },
            { name: 'Máy chiếu', badge: null },
            { name: 'Đèn thông minh', badge: null },
            { name: 'Bàn ủi', badge: null },
            { name: 'Chăm sóc thú cưng', badge: null },
            { name: 'Máy hút ẩm', badge: null },
          ],
        },
        {
          title: 'Sản phẩm nổi bật',
          items: [
            { name: 'Robot hút bụi Dreame X50 Ultra', badge: null },
            { name: 'Máy chơi game Sony PS5 Slim', badge: null },
            { name: 'Máy chiếu Beecube X2 Max Gen 5', badge: null },
            { name: 'Robot hút bụi Roborock Q Revo EDGE 5V1', badge: null },
            { name: 'Robot hút bụi Ecovacs T30 Pro Omni', badge: null },
            { name: 'Robot hút bụi Xiaomi X20+', badge: null },
            { name: 'Máy lọc không khí Xiaomi', badge: null },
            { name: 'Robot hút bụi Ecovacs', badge: null },
            { name: 'Robot hút bụi Roborock', badge: null },
          ],
        },
        {
          title: 'Thương hiệu gia dụng',
          items: [
            { name: 'Philips', badge: null },
            { name: 'Panasonic', badge: null },
            { name: 'Sunhouse', badge: null },
            { name: 'Sharp', badge: null },
            { name: 'Gaabor', badge: null },
            { name: 'Bear', badge: null },
            { name: 'AQUA', badge: 'MỚI' },
            { name: 'Toshiba', badge: 'MỚI' },
            { name: 'Midea', badge: 'MỚI' },
            { name: 'Dreame', badge: null },
            { name: 'Xiaomi', badge: null },
            { name: 'Cuckoo', badge: null },
          ],
        },
      ],
    },
    {
      name: ['Phụ kiện'],
      src: 'https://cellphones.com.vn/media/icons/menu/icon-cps-30.svg',
      tabs: [
        {
          title: 'Phụ kiện di động',
          items: [
            { name: 'Phụ kiện Apple', badge: null },
            { name: 'Dán màn hình', badge: null },
            { name: 'Ốp lưng - Bao da', badge: null },
            { name: 'Thẻ nhớ', badge: null },
            { name: 'Apple Care+', badge: null },
            { name: 'Samsung Care+', badge: null },
            { name: 'Sim 4G', badge: null },
            { name: 'Cáp, sạc', badge: null },
            { name: 'Pin dự phòng', badge: null },
            { name: 'Trạm sạc dự phòng', badge: null },
            { name: 'Phụ kiện điện thoại', badge: null },
          ],
        },
        {
          title: 'Phụ kiện Laptop',
          items: [
            { name: 'Chuột, bàn phím', badge: null },
            { name: 'Balo Laptop | Túi chống sốc', badge: null },
            { name: 'Phần mềm', badge: null },
            { name: 'Webcam', badge: null },
            { name: 'Giá đỡ', badge: null },
            { name: 'Thảm, lót chuột', badge: null },
            { name: 'Sạc laptop', badge: null },
            { name: 'Camera phòng họp', badge: null },
            { name: 'Hub chuyển đổi', badge: null },
          ],
        },
        {
          title: 'Thiết bị mạng',
          items: [
            { name: 'Thiết bị phát sóng WiFi', badge: null },
            { name: 'Bộ phát wifi di động', badge: null },
            { name: 'Bộ kích sóng WiFi', badge: null },
            { name: 'Xem tất cả thiết bị mạng', badge: null },
          ],
        },
        {
          title: 'Gaming Gear',
          items: [
            { name: 'PlayStation', badge: null },
            { name: 'ROG Ally', badge: null },
            { name: 'MSI Claw', badge: null },
            { name: 'Bàn phím Gaming', badge: null },
            { name: 'Chuột chơi game', badge: null },
            { name: 'Tai nghe Gaming', badge: null },
            { name: 'Tay cầm chơi game', badge: null },
            { name: 'Xem tất cả Gaming Gear', badge: null },
          ],
        },
        {
          title: 'Phụ kiện khác',
          items: [
            { name: 'Dây đeo đồng hồ', badge: null },
            { name: 'Dây đeo Airtag', badge: null },
            { name: 'Phụ kiện tiện ích', badge: null },
            { name: 'Phụ kiện ô tô', badge: null },
            { name: 'Bút cảm ứng', badge: null },
            { name: 'Thiết bị định vị', badge: null },
          ],
        },
        {
          title: 'Thiết bị lưu trữ',
          items: [
            { name: 'Thẻ nhớ', badge: null },
            { name: 'USB', badge: null },
            { name: 'Ổ cứng di động', badge: null },
          ],
        },
      ],
    },
    {
      name: ['PC,', 'Màn hình,', 'Máy in'],
      src: 'https://cdn2.cellphones.com.vn/x/media/icons/menu/icon_cpu.svg',
      tabs: [
        {
          title: 'Loại PC',
          items: [
            { name: 'Build PC', badge: null },
            { name: 'Cấu hình sẵn', badge: null },
            { name: 'All In One', badge: null },
            { name: 'PC bộ', badge: null },
          ],
        },
        {
          title: 'Chọn PC theo nhu cầu',
          items: [
            { name: 'Gaming', badge: null },
            { name: 'Đồ họa', badge: null },
            { name: 'Văn phòng', badge: null },
          ],
        },
        {
          title: 'Linh kiện máy tính',
          items: [
            { name: 'CPU', badge: null },
            { name: 'Main', badge: null },
            { name: 'RAM', badge: null },
            { name: 'Ổ cứng', badge: null },
            { name: 'Nguồn', badge: null },
            { name: 'VGA', badge: null },
            { name: 'Tản nhiệt', badge: null },
            { name: 'Case', badge: null },
            { name: 'Xem tất cả', badge: null },
          ],
        },
        {
          title: 'Chọn màn hình theo hãng',
          items: [
            { name: 'ASUS', badge: null },
            { name: 'Samsung', badge: null },
            { name: 'DELL', badge: null },
            { name: 'LG', badge: null },
            { name: 'MSI', badge: null },
            { name: 'Acer', badge: null },
            { name: 'Xiaomi', badge: null },
            { name: 'ViewSonic', badge: null },
            { name: 'Philips', badge: null },
            { name: 'AOC', badge: null },
            { name: 'Dahua', badge: null },
            { name: 'KOORUI', badge: null },
          ],
        },
        {
          title: 'Chọn màn hình theo nhu cầu',
          items: [
            { name: 'Gaming', badge: null },
            { name: 'Văn phòng', badge: null },
            { name: 'Đồ họa', badge: null },
            { name: 'Lập trình', badge: null },
            { name: 'Màn hình di động', badge: null },
            { name: 'Arm màn hình', badge: null },
            { name: 'Xem tất cả', badge: null },
          ],
        },
        {
          title: 'Gaming Gear',
          items: [
            { name: 'PlayStation', badge: null },
            { name: 'ROG Ally', badge: null },
            { name: 'Bàn phím Gaming', badge: null },
            { name: 'Chuột chơi game', badge: null },
            { name: 'Tai nghe Gaming', badge: null },
            { name: 'Tay cầm chơi Game', badge: null },
            { name: 'Xem tất cả', badge: null },
          ],
        },
        {
          title: 'Thiết bị văn phòng',
          items: [
            { name: 'Máy in', badge: null },
            { name: 'Phần mềm', badge: null },
            { name: 'Decor bàn làm việc', badge: null },
          ],
        },
      ],
    },
    {
      name: ['Tivi,', 'Máy lạnh,', 'Tủ lạnh'],
      src: 'https://cdn2.cellphones.com.vn/x/media/icons/menu/icon_cpu.svg',
      tabs: [
        {
          title: 'Chọn theo hãng',
          items: [
            { name: 'Samsung', badge: null },
            { name: 'LG', badge: null },
            { name: 'Xiaomi', badge: null },
            { name: 'Coocaa', badge: null },
            { name: 'Sony', badge: null },
            { name: 'Toshiba', badge: null },
            { name: 'TCL', badge: null },
            { name: 'Hisense', badge: null },
            { name: 'Aqua', badge: 'MỚI' },
          ],
        },
        {
          title: 'Chọn theo mức giá',
          items: [
            { name: 'Dưới 5 triệu', badge: null },
            { name: 'Từ 5 - 9 triệu', badge: null },
            { name: 'Từ 9 - 12 triệu', badge: null },
            { name: 'Từ 12 - 15 triệu', badge: null },
            { name: 'Trên 15 triệu', badge: null },
          ],
        },
        {
          title: 'Chọn theo độ phân giải',
          items: [
            { name: 'Tivi 4K', badge: null },
            { name: 'Tivi 8K', badge: null },
            { name: 'Tivi Full HD', badge: null },
            { name: 'Tivi OLED', badge: null },
            { name: 'Tivi QLED', badge: null },
            { name: 'Android Tivi', badge: null },
          ],
        },
        {
          title: 'Chọn theo kích thước',
          items: [
            { name: 'Tivi 32 inch', badge: null },
            { name: 'Tivi 43 inch', badge: null },
            { name: 'Tivi 50 inch', badge: null },
            { name: 'Tivi 55 inch', badge: null },
            { name: 'Tivi 65 inch', badge: null },
            { name: 'Tivi 70 inch', badge: null },
            { name: 'Tivi 85 inch', badge: null },
          ],
        },
        {
          title: 'Sản phẩm nổi bật',
          items: [
            { name: 'Máy lạnh Mijia Pro 1.0 HP Inverter 2025', badge: 'HOT' },
            { name: 'Máy lạnh Mijia Pro 1.5 HP Inverter 2025', badge: 'HOT' },
            { name: 'Tủ lạnh Xiaomi Multidoor 510L 2025', badge: 'MỚI' },
            { name: 'Tivi Samsung QLED 4K 65 inch', badge: null },
            { name: 'Tivi LG 43LM5750PTC FHD 43 inch', badge: null },
            { name: 'Tivi Xiaomi A 4K 2025 55 inch', badge: null },
            { name: 'Tivi Coocaa 4K 55 inch 55Y73', badge: null },
            { name: 'Tivi di động LG Stanby Me 27inch', badge: null },
          ],
        },
      ],
    },
    {
      name: ['Thu cũ đổi mới'],
      src: 'https://cellphones.com.vn/media/icons/menu/icon-cps-tcdm.svg',
      tabs: [
        {
          title: 'Chọn theo hãng',
          items: [
            { name: 'Thu cũ iPhone', badge: null },
            { name: 'Thu cũ Samsung', badge: null },
            { name: 'Thu cũ Xiaomi', badge: null },
            { name: 'Thu cũ Laptop', badge: null },
            { name: 'Thu cũ Mac', badge: null },
            { name: 'Thu cũ iPad', badge: null },
            { name: 'Thu cũ đồng hồ', badge: null },
            { name: 'Thu cũ Apple Watch', badge: null },
          ],
        },
        {
          title: 'Sản phẩm trợ giá cao',
          items: [
            { name: 'iPhone 16 Pro Max » 3 triệu', badge: null },
            { name: 'iPhone 15 Pro Max » 3 triệu', badge: null },
            { name: 'Galaxy S25 Ultra » 4 triệu', badge: null },
            { name: 'Galaxy Z Fold 6 » 4 triệu', badge: null },
            { name: 'Galaxy Z Flip 6 » 1.5 triệu', badge: null },
            { name: 'Macbook » 3 triệu', badge: null },
            { name: 'Laptop » 4 triệu', badge: null },
          ],
        },
        {
          title: 'Sản phẩm giá thu cao',
          items: [
            { name: 'iPhone 15 Pro Max', badge: null },
            { name: 'iPhone 14 Pro Max', badge: null },
            { name: 'iPhone 13 Pro Max', badge: null },
            { name: 'Samsung Galaxy Z Fold 5', badge: null },
            { name: 'Samsung Galaxy Z Flip 5', badge: null },
            { name: 'Samsung Galaxy S24 Ultra', badge: null },
            { name: 'Macbook Air M1', badge: null },
          ],
        },
      ],
    },
    {
      name: ['Hàng cũ'],
      src: 'https://cdn2.cellphones.com.vn/x/media/icons/menu/icon-cps-29.svg',
      tabs: [
        {
          title: 'Chọn loại sản phẩm cũ',
          items: [
            { name: 'Điện thoại cũ', badge: null },
            { name: 'Máy tính bảng cũ', badge: null },
            { name: 'Mac cũ', badge: null },
            { name: 'Laptop cũ', badge: null },
            { name: 'Tai nghe cũ', badge: null },
            { name: 'Loa cũ', badge: null },
            { name: 'Đồng hồ thông minh cũ', badge: null },
            { name: 'Đồ gia dụng cũ', badge: null },
            { name: 'Màn hình cũ', badge: null },
            { name: 'Phụ kiện cũ', badge: null },
            { name: 'Tivi cũ', badge: null },
          ],
        },
        {
          title: 'Chọn dòng iPhone cũ',
          items: [
            { name: 'iPhone 17 series cũ', badge: null },
            { name: 'iPhone 16 series cũ', badge: null },
            { name: 'iPhone 15 series cũ', badge: null },
            { name: 'iPhone 14 series cũ', badge: null },
            { name: 'iPhone 13 series cũ', badge: null },
            { name: 'iPhone 12 series cũ', badge: null },
            { name: 'iPhone 11 series cũ', badge: null },
            { name: 'Xem tất cả iPhone cũ', badge: null },
          ],
        },
        {
          title: 'Điện thoại Android cũ',
          items: [
            { name: 'Samsung cũ', badge: null },
            { name: 'Xiaomi cũ', badge: null },
            { name: 'OPPO cũ', badge: null },
            { name: 'Nokia cũ', badge: null },
            { name: 'realme cũ', badge: null },
            { name: 'vivo cũ', badge: null },
            { name: 'ASUS cũ', badge: null },
            { name: 'TCL cũ', badge: null },
            { name: 'Infinix cũ', badge: null },
          ],
        },
        {
          title: 'Chọn hãng laptop cũ',
          items: [
            { name: 'Laptop Dell cũ', badge: null },
            { name: 'Laptop ASUS cũ', badge: null },
            { name: 'Laptop Acer cũ', badge: null },
            { name: 'Laptop HP cũ', badge: null },
            { name: 'Laptop Surface cũ', badge: null },
          ],
        },
        {
          title: 'Sản phẩm nổi bật',
          items: [
            { name: 'iPhone 16 Pro Max - Cũ đẹp', badge: null },
            { name: 'iPhone 15 Pro Max cũ đẹp', badge: null },
            { name: 'iPhone 14 Pro Max cũ đẹp', badge: null },
            { name: 'iPhone 13 Pro Max cũ đẹp', badge: null },
            { name: 'Apple Watch Se 44mm 4G cũ đẹp', badge: null },
            { name: 'S23 Ultra cũ đẹp', badge: null },
            { name: 'S22 Ultra cũ đẹp', badge: null },
            { name: 'S24 Ultra cũ', badge: null },
          ],
        },
        {
          title: 'Sản phẩm Apple cũ',
          items: [
            { name: 'Apple Watch cũ', badge: null },
            { name: 'iPad cũ', badge: null },
          ],
        },
      ],
    },
    {
      name: ['Khuyến mãi'],
      src: 'https://cdn2.cellphones.com.vn/x/media/icons/menu/icon-cps-promotion.svg',
      tabs: [
        {
          title: 'Khuyến mãi',
          items: [
            { name: 'Hotsale cuối tuần', badge: null },
            { name: 'Ưu đãi thanh toán', badge: null },
            { name: 'Khách hàng doanh nghiệp B2B', badge: null },
            { name: 'Mua kèm gia dụng giảm 500K', badge: 'MỚI' },
          ],
        },
        {
          title: 'Thu cũ đổi mới giá hời',
          items: [
            { name: 'iPhone 16 Series trợ giá đến 3 triệu', badge: null },
            { name: 'S25 Series trợ giá 1 triệu', badge: null },
            { name: 'Xiaomi 15 trợ giá đến 3 triệu', badge: null },
            { name: 'Laptop trợ giá đến 4 triệu', badge: null },
          ],
        },
        {
          title: 'Ưu đãi thành viên',
          items: [{ name: 'Chính sách Smember 2025', badge: 'MỚI' }],
        },
        {
          title: 'Ưu đãi sinh viên',
          items: [
            { name: 'Chào năm học mới - Ưu đãi khủng', badge: null },
            { name: 'Nhập hội S-Student', badge: null },
            { name: 'Đăng ký S-Student', badge: 'HOT' },
            { name: 'Laptop giảm đến 500K', badge: null },
            { name: 'Điện thoại giảm đến 6%', badge: null },
            { name: 'Loa - tai nghe giảm thêm 5%', badge: null },
            { name: 'Hàng cũ giảm thêm 10%', badge: 'HOT' },
          ],
        },
      ],
    },
    {
      name: ['Tin công nghệ'],
      src: 'https://cdn.cellphones.com.vn/media/icons/menu/icon-cps-tech.svg',
      tabs: [
        {
          title: 'Chuyên mục',
          items: [
            { name: 'Tin công nghệ', badge: null },
            { name: 'Khám phá', badge: null },
            { name: 'S-Games', badge: null },
            { name: 'Tư vấn', badge: null },
            { name: 'Trên tay', badge: null },
            { name: 'Thị trường', badge: null },
            { name: 'Thủ thuật - Hỏi đáp', badge: null },
          ],
        },
      ],
    },
  ];
  goHome() {
    this.router.navigate(['/home'])
  }
  @ViewChild('searchContainer') searchContainer!: ElementRef;

  @HostListener('document:click', ['$event'])
  onClickOutside(event: Event) {
    if (this.searchContainer) {
      const clickedInside = this.searchContainer.nativeElement.contains(event.target);
      if (!clickedInside) {
        this.useSearch = false;
      }
    }
  }
  ngOnInit() {
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap((keyword) => {  
        if (!keyword.trim()) {
           return [];
        }
        return this.productService.searchProducts(keyword);
      })
    ).subscribe(results => {
      this.searchResults = results;
      this.isCalled = true;
      this.useSearch = true;
    });
  }
  onSearch() {
    this.useSearch = true;
    this.searchSubject.next(this.keyword);
  }
}