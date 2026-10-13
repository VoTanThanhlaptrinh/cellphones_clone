# DESIGN.md — Design system của Storefront CellphoneS Clone

> Tài liệu **bắt buộc** cho mọi thay đổi giao diện trong `cellphones_clone_ui/` (trang dành cho người mua, không áp dụng cho `admin_dashboard/`).
> Khi thêm hoặc sửa UI: dùng token và recipe ở đây, không tự đặt màu/kích thước mới. Muốn thêm token mới thì cập nhật file này trước.
> Lộ trình đưa code hiện tại về đúng chuẩn nằm ở [UI_IMPROVEMENT_PLAN.md](UI_IMPROVEMENT_PLAN.md).

---

## 1. Nguyên tắc

1. **Đỏ CellphoneS là màu thương hiệu duy nhất.** Chỉ dùng cho: hành động chính (Mua ngay, Thêm vào giỏ), giá bán, trạng thái đang chọn, header. Không dùng đỏ để trang trí tràn lan.
2. **Mobile-first.** Viết class cho màn hình nhỏ trước, sau đó mở rộng bằng `sm:`/`md:`/`lg:`. Mọi trang phải dùng được ở chiều rộng 360px.
3. **Dữ liệu thật hoặc không hiển thị.** Không đưa lên production các giá trị giả như "Giảm 17%", "xxxđ", "5 sao (1 đánh giá)" hay bảng thông số mẫu. Không có dữ liệu thì ẩn khối đó hoặc hiển thị empty state.
4. **Nhất quán hơn sáng tạo.** Cùng một loại phần tử (nút, giá, thẻ sản phẩm…) phải trông giống nhau ở mọi trang.
5. **Tiếng Việt, rõ ràng, truy cập được.** Mọi phần tử tương tác dùng được bằng bàn phím và có nhãn rõ nghĩa.

---

## 2. Design tokens

Token được khai báo trong `@theme` của `src/styles.css`. Tailwind v4 tự sinh class từ token (`bg-primary-500`, `text-danger`, `shadow-card`…).

```css
/* src/styles.css */
@import "tailwindcss";

@theme {
  /* Font */
  --font-sans: "Roboto", "Helvetica Neue", Arial, sans-serif;

  /* Màu thương hiệu – giữ đỏ CellphoneS */
  --color-primary: #d70018;
  --color-primary-50:  #fff1f2;
  --color-primary-100: #ffe0e3;
  --color-primary-200: #ffc6cc;
  --color-primary-300: #ff9aa5;
  --color-primary-400: #e45464;
  --color-primary-500: #d70018; /* màu thương hiệu */
  --color-primary-600: #c40016; /* hover */
  --color-primary-700: #a50012; /* active/pressed */
  --color-primary-800: #87000f;
  --color-primary-900: #6b000c;

  /* Màu ngữ nghĩa */
  --color-info: #3b82f6;     /* trả góp, yêu thích, link phụ */
  --color-success: #16a34a;  /* thành công, còn hàng */
  --color-warning: #f59e0b;  /* cảnh báo */
  --color-danger: #dc2626;   /* lỗi form, xoá – KHÔNG dùng thay màu thương hiệu */
  --color-rating: #fbbf24;   /* sao đánh giá */

  /* Bóng đổ */
  --shadow-card: 0 1px 2px rgb(0 0 0 / 0.06), 0 2px 6px rgb(0 0 0 / 0.08);
  --shadow-sticky: 0 -4px 16px rgb(0 0 0 / 0.08);
}
```

### 2.1 Màu

| Vai trò | Token / class | Ghi chú |
|---|---|---|
| Thương hiệu, CTA chính, giá bán | `primary-500` (`#d70018`) | Chữ trắng trên nền này đạt contrast ~5.4:1 |
| Hover của CTA | `primary-600` | |
| Active / pressed | `primary-700` | |
| Nền nhấn nhẹ (khối khuyến mãi, item đang chọn) | `primary-50` / `primary-100` | |
| Viền item đang chọn, focus ring | `primary-500` / `primary-300` | |
| Nền phụ trong header (nút "Danh mục", ô địa điểm) | `primary-400` hoặc `bg-white/15` | **Không** đặt chữ trắng cỡ nhỏ trên `primary-400` vì contrast chỉ ~3.7:1 |
| Thông tin phụ (trả góp, yêu thích) | `info` | |
| Lỗi, xoá | `danger` | |
| Thành công | `success` | |
| Sao đánh giá | `rating` | |

**Màu trung tính chỉ dùng thang `gray`**, không trộn với `neutral`, `slate` hay `zinc`.

| Vai trò | Class |
|---|---|
| Nền trang | `bg-gray-100` |
| Bề mặt (card, panel, modal) | `bg-white` |
| Chữ chính / tiêu đề | `text-gray-900` |
| Chữ phụ | `text-gray-600` |
| Chữ mờ, placeholder, disabled | `text-gray-400` |
| Giá gốc (gạch ngang) | `text-gray-500 line-through` |
| Viền mặc định / đường kẻ | `border-gray-200` |
| Viền input | `border-gray-300` |

### 2.2 Bảng chuyển đổi màu cũ sang token

Khi chạm vào file có màu cũ, thay theo bảng sau:

| Màu / class cũ | Thay bằng |
|---|---|
| `#da152b`, `#d7263a`, `#d70018`, `.main-color`, `.banner-bg-color`, `bg-[#d70018]` | `bg-primary-500` |
| `.main-text-color`, `text-red-600` (khi là màu thương hiệu) | `text-primary-500` |
| `#d02437`, `hover:bg-red-700`, `.button-bg:hover` | `hover:bg-primary-600` |
| `#e45464`, `.button-bg`, `.navbar-bg` | `bg-primary-400` (chỉ cho nền phụ, không dùng làm nền chữ nhỏ) hoặc `bg-white/15` |
| `bg-red-50`, `#fff5f0` | `bg-primary-50` |
| `text-red-500` / `ring-red-500` cho lỗi form | `text-danger` / `ring-danger` |
| `#3b82f6`, `.favorite-text`, `text-blue-600` | `text-info` |
| `#fbbf24`, `text-yellow-300/400` cho sao | `text-rating` |
| `neutral-*` | `gray-*` cùng cấp |
| `bg-orange-400` (badge giỏ hàng) | `bg-white text-primary-500` |

### 2.3 Bo góc, bóng, lớp (z-index)

| Thành phần | Bo góc |
|---|---|
| Button, input, badge vuông | `rounded-lg` |
| Card, panel, modal | `rounded-xl` |
| Chip lọc, avatar, badge số | `rounded-full` |

| Bóng | Dùng cho |
|---|---|
| `shadow-card` | Card sản phẩm, khối nội dung (thay cho `shadow-[0_2px_4px_rgba(0,0,0,0.2)]`) |
| `shadow-lg` | Dropdown, kết quả tìm kiếm |
| `shadow-sticky` | Thanh tổng tiền / thanh mua dính đáy trên mobile |

| Lớp | z-index |
|---|---|
| Thanh dính đáy (mobile CTA, tổng tiền) | `z-30` |
| Header dính | `z-40` |
| Dropdown, kết quả tìm kiếm, mega-menu | `z-50` |
| Modal / overlay | Do Angular Material quản lý, không tự đặt |

Không dùng các giá trị z-index tuỳ ý như `z-1000` hay `z-9999`.

---

## 3. Typography

- **Font duy nhất: Roboto**, nạp 3 weight 400/500/700 trong `src/index.html`:
  ```html
  <link href="https://fonts.googleapis.com/css2?family=Roboto:wght@400;500;700&display=swap" rel="stylesheet">
  ```
  Chỉ dùng `font-normal` (400), `font-medium` (500), `font-bold` (700). **Không** dùng `font-light`, `font-semibold`, `font-extrabold` vì các weight đó không được nạp nên trình duyệt sẽ tự giả lập.

| Vai trò | Class |
|---|---|
| Tiêu đề trang (H1) | `text-xl md:text-2xl font-bold text-gray-900` |
| Tiêu đề section (H2) | `text-lg md:text-xl font-bold text-gray-900` |
| Tiêu đề khối nhỏ (H3) | `text-base font-medium text-gray-900` |
| Body | `text-sm md:text-base text-gray-900` |
| Chữ phụ | `text-sm text-gray-600` |
| Caption, nhãn nhỏ | `text-xs text-gray-500` |
| Tên sản phẩm trong card | `text-sm font-medium text-gray-900 line-clamp-2` |

**Thẻ heading** phải theo đúng cấp bậc: mỗi trang có 1 `h1`, sau đó đến `h2`, `h3`. Không lồng `<p>` trong `<h2>`.

### 3.1 Hiển thị giá

| Ngữ cảnh | Giá bán | Giá gốc |
|---|---|---|
| Card / danh sách / giỏ | `text-base font-bold text-primary-500` | `text-xs text-gray-500 line-through` |
| Trang chi tiết | `text-2xl font-bold text-primary-500` | `text-base text-gray-500 line-through` |
| Tổng tiền | `text-lg font-bold text-primary-500` | – |

- Luôn format giá qua pipe dùng chung `vnd` (xem §6). Không viết hàm format riêng trong component, không nối chuỗi `price + 'đ'`.
- Chỉ hiện giá gốc khi `basePrice > salePrice`.

---

## 4. Layout & lưới

- **Container chuẩn**: khai báo trong `src/styles.css` và dùng cho mọi trang.
  ```css
  @utility container-page {
    width: 100%;
    max-width: 75rem;      /* 1200px – tương đương max-w-300 */
    margin-inline: auto;
    padding-inline: 0.75rem;
    @media (width >= 48rem) { padding-inline: 1rem; }
  }
  ```
  Không dùng `xl:w-[80vw]`, `lg:max-w-350`, `sm:w-sm md:w-md`, `w-150` hay các chiều rộng cố định khác cho khung trang.
- **Luồng giỏ hàng / thanh toán** dùng cột hẹp: `mx-auto w-full max-w-150 px-3`.
- **Breakpoint**: dùng mặc định của Tailwind (`sm` 640, `md` 768, `lg` 1024, `xl` 1280). Không dùng `vw` để tính chiều rộng card.
- **Spacing theo bội số 4px**: khoảng cách trong thẻ `gap-2`/`p-3` (mobile), `p-4` (desktop); giữa các section `mt-6 md:mt-8`.
- **Lưới sản phẩm**:
  ```html
  <div class="grid grid-cols-2 gap-2 sm:grid-cols-3 md:gap-3 lg:grid-cols-4 xl:grid-cols-5">
  ```
- **Cấu trúc trang**: nền `bg-gray-100`, nội dung đặt trong các khối `bg-white rounded-xl shadow-card p-3 md:p-4`.

---

## 5. Component recipes

Các class dùng chung được định nghĩa trong `@layer components` của `src/styles.css`. Component Angular dùng chung đặt trong `src/app/shared/`.

### 5.1 Button

```css
@layer components {
  .btn {
    @apply inline-flex h-10 items-center justify-center gap-2 rounded-lg px-4 text-sm font-medium
           transition-colors duration-150 select-none
           focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-300 focus-visible:ring-offset-2
           disabled:cursor-not-allowed disabled:opacity-50;
  }
  .btn-primary { @apply bg-primary-500 text-white hover:bg-primary-600 active:bg-primary-700; }
  .btn-outline { @apply border border-primary-500 bg-white text-primary-500 hover:bg-primary-50; }
  .btn-info    { @apply border border-info bg-white text-info hover:bg-blue-50; }
  .btn-ghost   { @apply bg-transparent text-gray-700 hover:bg-gray-100; }
  .btn-sm { @apply h-8 px-3 text-xs; }
  .btn-lg { @apply h-12 px-6 text-base; }
  .btn-block { @apply w-full; }
}
```

| Biến thể | Dùng cho |
|---|---|
| `btn btn-primary` | Hành động chính duy nhất trong một vùng: Mua ngay, Thanh toán, Đăng nhập, Gửi đánh giá |
| `btn btn-outline` | Hành động phụ: Thêm vào giỏ, Đăng ký, Theo dõi đơn hàng |
| `btn btn-info` | Trả góp 0% |
| `btn btn-ghost` | Hành động nhẹ: Huỷ, Xem thêm, icon button |

Quy tắc:
- Luôn dùng `<button type="button">` (hoặc `type="submit"` trong form). Nếu nút chuyển trang thì dùng `<a routerLink>` kèm class `btn`.
- Mỗi vùng màn hình chỉ có **một** `btn-primary`.
- Khi đang gọi API: thêm `[disabled]="loading"` và đổi nhãn (ví dụ "Đang xử lý…"). Không bấm được 2 lần.
- Icon button phải có `aria-label`.
- Không dùng hiệu ứng `hover:scale-*` hay gradient cho nút.

### 5.2 Product card (`app-product-card`)

```html
<a [routerLink]="['/product', p.id]"
   class="group relative flex h-full flex-col gap-2 rounded-xl bg-white p-3 shadow-card
          transition-shadow hover:shadow-lg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-300">
  @if (discountPercent(p) > 0) {
    <span class="absolute left-0 top-2 rounded-r-lg bg-primary-500 px-2 py-0.5 text-xs font-bold text-white">
      Giảm {{ discountPercent(p) }}%
    </span>
  }
  <img [ngSrc]="p.imgUrl" [alt]="p.productName" width="160" height="160"
       class="mx-auto aspect-square w-full max-w-40 object-contain transition-transform group-hover:scale-[1.02]" />
  <h3 class="text-sm font-medium text-gray-900 line-clamp-2 min-h-10">{{ p.productName }}</h3>
  <div class="flex flex-wrap items-baseline gap-x-2">
    <span class="text-base font-bold text-primary-500">{{ p.salePrice | vnd }}</span>
    @if (p.basePrice > p.salePrice) {
      <span class="text-xs text-gray-500 line-through">{{ p.basePrice | vnd }}</span>
    }
  </div>
  <div class="mt-auto flex items-center justify-between">
    <!-- rating chỉ hiện khi có dữ liệu -->
    <button type="button" class="btn btn-ghost btn-sm text-info" aria-label="Thêm vào yêu thích"
            [attr.aria-pressed]="isFavorite" (click)="$event.preventDefault(); $event.stopPropagation(); toggleFavorite()">
      …
    </button>
  </div>
</a>
```

Quy tắc:
- Cả thẻ là một `<a routerLink>`, **không** dùng `div (click)`. Như vậy người dùng mở được tab mới, dùng được bàn phím và SEO tốt hơn.
- Phần trăm giảm giá **tính từ dữ liệu**: `round((base − sale) / base × 100)`. Chỉ hiện khi lớn hơn 0.
- Badge "Trả góp 0%" hay ưu đãi S-Student chỉ hiện khi API trả về cờ tương ứng.
- Chiều rộng của card do lưới cha quyết định. Card không tự đặt `w-[..vw]`.
- Nút nằm bên trong thẻ phải chặn điều hướng (`preventDefault` + `stopPropagation`).

### 5.3 Badge & chip

| Loại | Class |
|---|---|
| Giảm giá | `rounded-r-lg bg-primary-500 px-2 py-0.5 text-xs font-bold text-white` |
| Trả góp | `rounded-lg bg-blue-50 px-2 py-0.5 text-xs font-medium text-info` |
| Trạng thái thành công | `rounded-full bg-green-100 px-3 py-1 text-xs font-medium text-success` |
| Badge số trên icon giỏ | `absolute -right-1.5 -top-1.5 grid min-w-4.5 h-4.5 place-items-center rounded-full bg-white px-1 text-[10px] font-bold text-primary-500` |
| Chip lọc (thương hiệu, nhu cầu) | `inline-flex h-8 items-center rounded-full border border-gray-300 bg-white px-3 text-sm text-gray-700 hover:border-primary-500 hover:text-primary-500` |
| Chip lọc đang chọn | thêm `border-primary-500 bg-primary-50 text-primary-500` |

Chip lọc là `<a routerLink [queryParams]>` hoặc `<button>`, không phải `div`.

### 5.4 Lựa chọn (phiên bản, màu sắc)

```html
<label class="relative block cursor-pointer">
  <input type="radio" class="peer sr-only" … />
  <span class="flex items-center gap-2 rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm
               peer-checked:border-primary-500 peer-checked:bg-primary-50
               peer-focus-visible:ring-2 peer-focus-visible:ring-primary-300">…</span>
  <span class="absolute -right-1.5 -top-1.5 hidden size-4 place-items-center rounded-full bg-primary-500 text-[10px] text-white peer-checked:grid">✓</span>
</label>
```

Không dùng `min-w-[12vw]`. Đặt các lựa chọn trong lưới `grid grid-cols-2 sm:grid-cols-3 gap-2`.

### 5.5 Rating

- Sao dùng `text-rating`, sao trống dùng `text-gray-300`. Kích thước 14px trong card, 20px trong trang chi tiết.
- Điểm và số lượt đánh giá lấy từ API review (`averageRating`, `reviews().length`). Không có review thì hiện "Chưa có đánh giá".
- Có text thay thế cho trình đọc màn hình: `<span class="sr-only">4,5 trên 5 sao</span>`.

### 5.6 Input (`app-input`)

- Mỗi instance phải có **id duy nhất** (ví dụ `inputId = \`input-${nextId++}\``). Không hard-code `id="input"`.
- Nhãn luôn liên kết với input qua `for`/`id`.
- Màu khi focus: `border-primary-500` hoặc `ring-primary-300` (không dùng xanh dương).
- Trạng thái lỗi: `border-danger` + `<p class="mt-1 text-xs text-danger">`. Hiện lỗi sau khi người dùng đã chạm vào trường (`touched`) hoặc khi submit.
- Form dùng Reactive Forms, và trường bắt buộc có dấu `*`.
- Input thường (không dùng floating label):
  `h-10 w-full rounded-lg border border-gray-300 bg-white px-3 text-sm text-gray-900 placeholder:text-gray-400 focus:border-primary-500 focus:outline-none focus:ring-2 focus:ring-primary-100`

### 5.7 Header

- Thanh header chính dùng `sticky top-0 z-40 bg-primary-500`. Thuộc tính sticky đặt **trên chính phần tử `<header>`** (hoặc wrapper ngoài cùng), không đặt trên một section con.
- Nút phụ trong header ("Danh mục", "Xem giá tại"): `rounded-lg bg-white/15 px-3 py-2 text-sm text-white hover:bg-white/25`.
- Ô tìm kiếm: `h-10 rounded-lg bg-white`. Mỗi kết quả là `<a routerLink>` có ảnh, tên và giá (format bằng `vnd`). Hỗ trợ phím ↑/↓/Enter/Esc. Panel kết quả dùng `z-50 shadow-lg rounded-xl`.
- Menu tài khoản mở bằng **click** (`<button aria-expanded>`), không mở chỉ bằng hover. Nhãn tiếng Việt: "Tài khoản của tôi", "Đăng xuất".

### 5.8 Breadcrumb

```html
<nav aria-label="Breadcrumb" class="flex items-center gap-1 text-xs text-gray-600">
  <a routerLink="/home" class="hover:text-primary-500">Trang chủ</a>
  <span aria-hidden="true">/</span>
  <span class="text-gray-900" aria-current="page">{{ name }}</span>
</nav>
```

### 5.9 Section header

```html
<div class="mb-3 flex items-center justify-between">
  <h2 class="text-lg md:text-xl font-bold text-gray-900">Điện thoại nổi bật</h2>
  <a routerLink="…" class="text-sm text-info hover:underline">Xem tất cả</a>
</div>
```

### 5.10 Thanh dính đáy (mobile CTA / tổng tiền)

```html
<div class="fixed inset-x-0 bottom-0 z-30 border-t border-gray-200 bg-white px-3 py-2 shadow-sticky
            pb-[calc(0.5rem+env(safe-area-inset-bottom))] md:static md:shadow-none">
  …
</div>
```

Khi dùng thanh này, phần nội dung trang phải có `pb-20 md:pb-0` để không bị thanh che mất.

### 5.11 Stepper thanh toán

Gồm 2 bước "1. Thông tin" và "2. Thanh toán". Bước hiện tại dùng `text-primary-500 border-b-2 border-primary-500`, các bước khác dùng `text-gray-400 border-b-2 border-gray-200`. Bước hiện tại có `aria-current="step"`. Chỉ cho bấm vào bước đã hoàn thành.

### 5.12 Empty state

```html
<div class="flex flex-col items-center gap-3 rounded-xl bg-white px-4 py-10 text-center">
  <svg class="size-12 text-gray-300" aria-hidden="true">…</svg>
  <p class="text-sm text-gray-600">Giỏ hàng của bạn đang trống.</p>
  <a routerLink="/home" class="btn btn-primary">Tiếp tục mua sắm</a>
</div>
```

Áp dụng cho: giỏ hàng trống, tìm kiếm không có kết quả, danh mục không có sản phẩm, chưa có đánh giá, lịch sử mua hàng trống.

### 5.13 Loading

| Tình huống | Cách hiển thị |
|---|---|
| Tải danh sách / lưới | Skeleton (`app-product-skeleton`, `animate-pulse bg-gray-200`), giữ đúng kích thước nội dung thật |
| Hành động của người dùng (submit, thanh toán) | Disable nút và đổi nhãn. Overlay toàn màn hình (`ngx-spinner`, màu `#d70018`) chỉ dùng cho thao tác chặn toàn trang như tạo đơn hay thanh toán |
| Ảnh | `loading="lazy"` cho ảnh dưới màn hình đầu tiên; ảnh LCP (banner, ảnh chính sản phẩm) dùng `priority` của `NgOptimizedImage` |

### 5.14 Thông báo (toast) & hộp thoại

- Toast **chỉ** gọi qua `NotifyService` (`src/app/services/notify.service.ts`), không gọi `ToastrService` trực tiếp. Nội dung tiếng Việt, ngắn gọn, nói rõ chuyện gì đã xảy ra và cần làm gì.
- Hộp thoại dùng `MatDialog`. Tiêu đề là `h2` có `mat-dialog-title`. Nút trong hộp thoại dùng recipe §5.1, hành động chính nằm bên phải.

### 5.15 Footer

- Desktop chia 4 cột (`lg:grid-cols-4`). Mobile hiển thị dạng accordion hoặc xếp dọc gọn. **Không ẩn footer trên mobile**, vì đó là nơi chứa hotline và chính sách.
- Không dùng class Bootstrap (`d-flex`, `justify-content-*`, `form-control-*`, `column-gap-*`).

---

## 6. Định dạng dữ liệu

| Dữ liệu | Quy tắc | Công cụ |
|---|---|---|
| Tiền | `29.990.000đ` (dấu chấm phân cách nghìn, hậu tố `đ` liền sau). Giá null hoặc 0 hiển thị `Liên hệ`. **Tổng tiền dùng `| vnd:false`** để 0 vẫn hiện `0đ` | Pipe `vnd` tại `src/app/shared/pipes/vnd.pipe.ts` |
| Ngày | `dd/MM/yyyy`. Có giờ thì dùng `dd/MM/yyyy HH:mm` | `DatePipe` (locale `vi-VN` đã đăng ký trong `app.config.ts`) |
| Số điện thoại | Che giữa khi hiển thị công khai: `079****84` | – |
| Rating | 1 chữ số thập phân, dấu phẩy: `4,5` | `DecimalPipe` `'1.1-1'` |

```ts
// src/app/shared/pipes/vnd.pipe.ts
@Pipe({ name: 'vnd' })
export class VndPipe implements PipeTransform {
  private static readonly formatter = new Intl.NumberFormat('vi-VN');

  transform(value: number | null | undefined, zeroAsEmpty = true, empty = 'Liên hệ'): string {
    if (value == null || Number.isNaN(value) || (zeroAsEmpty && value === 0)) return empty;
    return `${VndPipe.formatter.format(value)}đ`;
  }
}
```

---

## 7. Icon

- Dùng inline SVG, `fill="none" stroke="currentColor" stroke-width="1.5"`. Màu điều khiển bằng class `text-*`. **Không** hard-code `stroke="#D70018"` hay `fill="white"`.
- Kích thước: `size-4` (16px) trong text, `size-5` (20px) trong nút, `size-6` (24px) cho icon độc lập.
- Icon trang trí phải có `aria-hidden="true"`. Icon đứng một mình trong nút thì nút cần `aria-label`.
- `id` bên trong SVG (`clipPath`, `mask`, `linearGradient`) phải duy nhất trên trang. Tốt nhất là không dùng các phần tử này cho icon đơn giản.
- Icon lặp lại từ 2 nơi trở lên nên được tách thành component dùng chung trong `src/app/shared/icons/`.

---

## 8. Tương tác & khả năng truy cập (a11y)

- **Phần tử bấm được phải là `<button>` hoặc `<a>`.** Không gắn `(click)` lên `div`, `p`, `svg` hay `<a>` thiếu `href`/`routerLink`.
- **Link nội bộ dùng `routerLink`**, không dùng `href="/..."` (gây tải lại toàn trang). Không dùng `href="#"`. Chức năng chưa làm thì ẩn đi.
- Mọi phần tử có hover phải có focus tương ứng: `focus-visible:ring-2 focus-visible:ring-primary-300`.
- Ảnh nội dung có `alt` mô tả (tên sản phẩm, tên thương hiệu). Ảnh trang trí dùng `alt=""` kèm `aria-hidden="true"`.
- Contrast chữ tối thiểu 4.5:1 (chữ lớn từ 18px đậm trở lên: 3:1).
- Vùng chạm trên mobile tối thiểu 40×40px.
- Dropdown, menu và modal đóng được bằng `Esc` và dùng được bằng bàn phím.
- Hiệu ứng chạy liên tục (marquee) phải tắt khi người dùng bật `prefers-reduced-motion`.
- `<html lang="vi">`. Mỗi route có `title` riêng (thuộc tính `title` của Angular Router).

**Chuyển động**: `transition-colors` hoặc `transition-shadow` trong 150–200ms. Không dùng `duration-500` trở lên cho tương tác. Không phóng to nút khi hover.

---

## 9. Nội dung & ngôn từ

- 100% tiếng Việt trong UI dành cho khách. Không để lẫn nhãn tiếng Anh như "Profile", "Logout" hay "User".
- Nút dùng động từ, viết hoa chữ đầu: "Mua ngay", "Thêm vào giỏ", "Đăng nhập", "Xem thêm 12 sản phẩm". Không viết hoa toàn bộ (dùng "Mua ngay" thay vì "MUA NGAY").
- Thông báo lỗi nói rõ cách khắc phục, ví dụ "Số điện thoại phải gồm 10 chữ số" thay vì chỉ "Không hợp lệ".
- Không có placeholder hoặc lorem trong production: không "xxxđ", "Ưu đãi khi mua ...", "Đang cập nhật" lặp lại khắp nơi.
- Tên người dùng, số điện thoại, hạng thành viên luôn lấy từ `AuthService.currentUser()`. **Không hard-code.**

---

## 10. Thư viện UI

| Thư viện | Trạng thái | Dùng cho |
|---|---|---|
| Tailwind CSS v4 | **Chính** | Toàn bộ layout và style |
| Angular Material | Hạn chế | Chỉ `MatDialog`, `MatTabs`. Theme phải là đỏ thương hiệu (xem bên dưới) |
| Swiper (`swiper-container`) | Giữ | Banner, slider sản phẩm, gallery ảnh |
| ngx-toastr | Giữ | Thông qua `NotifyService` |
| ngx-spinner | Giữ | Overlay cho thao tác chặn toàn trang |
| daisyUI | **Chờ gỡ** | Đang không được bật (`@plugin`). Không dùng class daisyUI |
| flowbite (JS global) | **Chờ gỡ** | Không dùng `data-*` của flowbite |
| igniteui-angular | **Chờ gỡ** | Không được dùng trong `src` |
| Bootstrap class | **Cấm** | Dự án không có Bootstrap |

Không thêm thư viện UI mới khi chưa cập nhật bảng này.

Theme của Angular Material (`src/custom-theme.scss`) phải đặt primary về đỏ:
```scss
html {
  @include mat.theme((
    color: (theme-type: light, primary: mat.$red-palette),
    typography: Roboto,
    density: 0,
  ));
  @include mat.theme-overrides((primary: #d70018, on-primary: #ffffff));
}
```

---

## 11. Checklist trước khi merge thay đổi UI

- [ ] Chỉ dùng token trong file này. Không có mã hex mới, không dùng `neutral-*`, không dùng class không tồn tại.
- [ ] Đã kiểm tra ở các chiều rộng 375px, 768px và 1280px: không tràn ngang, không có chiều rộng cố định bằng `vw`/`px`.
- [ ] Giá hiển thị qua pipe `vnd`. Ngày hiển thị qua `DatePipe`.
- [ ] Không có dữ liệu hard-code hoặc placeholder (tên người dùng, % giảm, rating, tồn kho…).
- [ ] Mọi phần tử bấm được là `button`/`a` và dùng được bằng bàn phím. Có focus ring.
- [ ] Ảnh có `alt`. Icon trang trí có `aria-hidden`.
- [ ] Toàn bộ chữ là tiếng Việt và đúng giọng văn ở §9.
- [ ] Có trạng thái loading, rỗng và lỗi cho mọi khối gọi API.
- [ ] `npm run build` chạy thành công.
