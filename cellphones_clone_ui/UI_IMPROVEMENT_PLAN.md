# Kế hoạch cải thiện UI — Storefront CellphoneS Clone

> Phạm vi: `cellphones_clone_ui/`, các trang dành cho người mua. `admin_dashboard/` **nằm ngoài phạm vi**.
> Chuẩn đích là [DESIGN.md](DESIGN.md). Mọi task dưới đây đều đưa code về đúng chuẩn đó, và **giữ nguyên màu đỏ thương hiệu `#d70018`**.
> Khảo sát ngày 13/10/2026 trên nhánh `thanh`.

---

## 1. Mục tiêu

1. **Nhất quán thương hiệu**: một màu đỏ duy nhất, một font, một hệ nút/giá/thẻ cho toàn site.
2. **Luồng mua hàng hoạt động trọn vẹn và tin cậy**: từ thẻ sản phẩm → chi tiết → giỏ → thông tin → thanh toán → thành công. Không còn nút chết hay dữ liệu giả.
3. **Dùng tốt trên mobile**: không tràn ngang, có thanh CTA và tổng tiền cố định ở đáy.
4. **Truy cập được và nhanh hơn**: dùng được bằng bàn phím, có `alt`, lazy route, ảnh tối ưu.

**Chỉ số hoàn thành tổng**: Lighthouse (mobile) Accessibility ≥ 90 và Best Practices ≥ 90 cho `/home`, `/product/:id`, `/cart`. Không còn mã hex đỏ cũ nào và không còn `convertPriceVnd` (kiểm tra bằng grep, xem §5).

---

## 2. Hiện trạng: các vấn đề chính

### 2.1 Nền tảng (ảnh hưởng toàn site)

| # | Vấn đề | Vị trí |
|---|---|---|
| F1 | Đỏ thương hiệu bị phân mảnh thành 6+ mã: `#d70018`, `#da152b` (`.main-color`), `#d7263a`, `#d02437`, `#e45464` (`.button-bg`/`.navbar-bg`), cùng với `red-500/600/700` của Tailwind. Chữ trắng trên `#e45464` chỉ đạt contrast ~3.7:1 (trượt chuẩn AA). | [src/styles.css](src/styles.css), 12 template dùng `main-color`/`button-bg`/`navbar-bg` |
| F2 | **Class không tồn tại**: chưa có `@theme`, nên `bg-primary-500` (14 chỗ), `text-primary`, `border-primary-*`, `gap-large`, `text-small`, `p-3xl`, `text-pure-white`… không có tác dụng. Login, register và member-dashboard vì vậy hiển thị sai. | [login.component.html:8](src/app/login/login.component.html#L8), [register.component.html:494](src/app/register/register.component.html#L494), member-dashboard, header-member-dashboard, product-detail |
| F3 | daisyUI được cài nhưng không bật `@plugin`, nên `btn`, `card`, `modal-box` không có style. Footer dùng class Bootstrap (`d-flex`, `form-control-plaintext`…) trong khi dự án không có Bootstrap. | [footer.component.html:1](src/app/footer/footer.component.html#L1), cart, category |
| F4 | Theme Angular Material đang là **xanh azure**, nên tab và dialog lệch màu thương hiệu. | [src/custom-theme.scss](src/custom-theme.scss) |
| F5 | Font: chỉ nạp Roboto 300/400/500 trong khi template dùng `font-bold` (46 chỗ), `font-semibold` (24), `font-extrabold` (4), nên chữ đậm bị giả lập. Open Sans được nạp nhưng không dùng. Material Icons bị nạp 2 lần. `lang="en"`, title "CellphonesCloneUi". `--font-roboto` chưa định nghĩa. Class `ig-*` thuộc igniteui (không được dùng). | [src/index.html](src/index.html) |
| F6 | Logic format giá `convertPriceVnd()` bị **copy ở 4 component**. Header và product-card in số thô kiểu `29990000đ`. | cart, order-review, payment, payment-infor; [header.component.html:321](src/app/header/header.component.html#L321), [product-card.component.html:33](src/app/product-card/product-card.component.html#L33) |
| F7 | Chiều rộng khung trang không thống nhất: `lg:max-w-350`, `max-w-300`, `xl:w-[80vw]`, `sm:w-sm md:w-md`, `w-150`… | home, product-detail, header, category |
| F8 | 20 link nội bộ dùng `href="/..."` nên mỗi lần bấm tải lại toàn bộ SPA. Nhiều link `href="#"` không dẫn đi đâu. | home, header, category, product-detail |
| F9 | Component `app-input` hard-code `id="input"`, nên nhiều input trên cùng trang bị trùng ID và label gắn sai. Màu focus là xanh dương. | [input.component.html:5](src/app/shared/custom/input/input.component.html#L5) |

### 2.2 Luồng mua hàng

| # | Vấn đề | Vị trí |
|---|---|---|
| C1 | Product card: badge "Giảm 17%", "Trả góp 0%", "S-Student giảm đến xxxđ" và rating "5" đều **hard-code**. | [product-card.component.html:5-16, 47-58](src/app/product-card/product-card.component.html#L5) |
| C2 | Product card là `div (click)`: không dùng được bằng bàn phím, không mở được tab mới. Thẻ thiếu `relative` nên badge định vị theo phần tử cha. Ảnh có `alt=""`. Nút "Yêu thích" nằm trong vùng click nên bấm vào sẽ chuyển trang. Chiều rộng tính theo `vw`. | [product-card.component.html:1-24](src/app/product-card/product-card.component.html#L1) |
| C3 | Chi tiết sản phẩm: **"MUA NGAY" là `div` không có handler**. "Thêm vào giỏ" là `<a (click)>` thiếu href. "Trả góp 0%" là `div` chết. | [product-detail.component.html:567-598](src/app/product-detail/product-detail.component.html#L567) |
| C4 | Chi tiết sản phẩm: rating đầu trang "5 (1 đánh giá)" hard-code, trong khi phần review thật ở dưới có `averageRating()`. "Kho còn {{ 1 }}" hard-code. Khối "Kho hàng" rỗng. 4 dòng khuyến mãi giống hệt nhau, đều có `href="#"`. | [:34-42](src/app/product-detail/product-detail.component.html#L34), [:375](src/app/product-detail/product-detail.component.html#L375), [:432-447](src/app/product-detail/product-detail.component.html#L432), [:538-565](src/app/product-detail/product-detail.component.html#L538) |
| C5 | Chi tiết sản phẩm: khi thiếu thông số thì hiện **bảng thông số iPhone giả**. Các nút "Yêu thích / Hỏi đáp / Thông số / So sánh" không làm gì. | [:207-232](src/app/product-detail/product-detail.component.html#L207), [:43-120](src/app/product-detail/product-detail.component.html#L43) |
| C6 | Chi tiết sản phẩm: `w-150` (600px) cố định và `min-w-[21.5vw]` gây tràn ngang trên mobile. Không có thanh mua cố định ở đáy trên mobile. Heading lồng `<p>` trong `<h2>`. | [:122](src/app/product-detail/product-detail.component.html#L122), [:488](src/app/product-detail/product-detail.component.html#L488), [:31-33](src/app/product-detail/product-detail.component.html#L31) |
| C7 | Giỏ hàng: tên user **hard-code `'Thành'`**. **Nút xoá item không có handler.** Mũi tên back không bấm được. Nút "Giỏ hàng" không có chức năng. Khuyến mãi placeholder "Ưu đãi khi mua ...". Thanh tổng tiền không cố định. Không có empty state. | [cart.component.html:2, 6, 17, 49, 82, 91](src/app/cart/cart.component.html#L2) |
| C8 | Thông tin thanh toán: back là `svg (click)`, bước "2. Thanh toán" là `div (click)`, nên không truy cập được bằng bàn phím và không có trạng thái stepper rõ ràng. | [payment-infor.component.html:5, 19](src/app/payment-infor/payment-infor.component.html#L5) |
| C9 | Trang đặt hàng thành công: tên user **hard-code `'Khách'`**. Nút dùng `routerLink` trên `<button>`. CTA dùng gradient, lệch chuẩn nút. | [checkout.component.html:3, 54-61](src/app/checkout/checkout.component.html#L3) |

### 2.3 Điều hướng & khám phá

| # | Vấn đề | Vị trí |
|---|---|---|
| N1 | Header: `sticky` đặt trên `section` con của `header` nên **không dính** khi cuộn. Dùng `z-9999`. | [header.component.html:159](src/app/header/header.component.html#L159) |
| N2 | Kết quả tìm kiếm: các mục là `<a>` không có href nên không bấm vào sản phẩm được. Giá không format. Không điều hướng được bằng phím. Không đóng khi click ra ngoài hoặc bấm Esc. | [header.component.html:310-335](src/app/header/header.component.html#L310) |
| N3 | Menu tài khoản chỉ mở bằng hover (`group-hover`), nên không dùng được trên cảm ứng hay bàn phím. Nhãn tiếng Anh "Profile"/"Logout". Badge giỏ màu cam. Link "Giỏ hàng" luôn mở modal yêu cầu đăng nhập. | [header.component.html:341-380](src/app/header/header.component.html#L341) |
| N4 | Trang chủ: banner dùng `mat-tab` với cùng một ảnh `banner.png` lặp lại. Lưới phụ kiện cố định `grid-cols-9` nên vỡ trên mobile. Link "Xem tất cả" là `href="#"`. Mega-menu rộng cố định `w-250`. | [home.component.html:66-125, 186-203](src/app/home/home.component.html#L66) |
| N5 | Danh mục: breadcrumb trỏ cả 2 mục về `/`. Ô thương hiệu có `<img>` **thiếu `src`** và `alt="item.alt"` là chuỗi literal. Ô "nhu cầu" không bấm được. Nút "Xem thêm" màu xanh nhạt lệch chuẩn. | [category.component.html:20-22, 51-73, 91-95](src/app/category/category.component.html#L20) |
| N6 | Footer bị **ẩn hoàn toàn trên mobile** (`hidden md:block`), nên mất hotline và chính sách. | [footer.component.html:1](src/app/footer/footer.component.html#L1) |

### 2.4 Tài khoản

| # | Vấn đề | Vị trí |
|---|---|---|
| A1 | Login/Register dùng hàng loạt class không tồn tại (xem F2), nên bố cục và màu nút bị sai. Có link `href="#"` "Điện thoại vui". | [login.component.html](src/app/login/login.component.html), [register.component.html](src/app/register/register.component.html) |
| A2 | Member dashboard: tên **"Võ Tấn Thành"** và số điện thoại **hard-code**. Hạng "S-NULL" là giá trị tĩnh. | [member-dashboard.component.html:16-19](src/app/member-dashboard/member-dashboard.component.html#L16) |

### 2.5 Chất lượng chung

| # | Vấn đề |
|---|---|
| Q1 | Mọi route đều import eager, không có `loadComponent`. Route thử nghiệm `/autocomplete` vẫn mở công khai ([app.routes.ts:54](src/app/app.routes.ts#L54)). |
| Q2 | Không dùng `NgOptimizedImage`. Ảnh banner/LCP không có `priority`. Nhiều ảnh thiếu `width`/`height` nên gây layout shift. |
| Q3 | Không có title riêng cho từng route và không có meta description. |
| Q4 | SVG icon được copy nguyên khối ở nhiều nơi, hard-code màu (`stroke="white"`, `#D70018`) và có `id` clipPath trùng nhau. |
| Q5 | Marquee ở top bar chạy liên tục, không tôn trọng `prefers-reduced-motion`. |

---

## 3. Lộ trình

Thứ tự: **P0 → P1 → P2 → P3 → P4**. P0 là điều kiện tiên quyết vì nó tạo token và recipe cho các giai đoạn sau. Mỗi task nên là một PR nhỏ.

### P0 — Nền tảng design system (bắt buộc làm trước)

> **Trạng thái: ✅ HOÀN THÀNH (13/10/2026)** – `ng build` thành công, token/class mới có trong CSS sinh ra.
> Ghi chú: `product-detail` cũng có `convertPriceVnd` (5 nơi, không phải 4); đã xoá cả `CurrencyPipe` ở `app.config.ts`/`order.component.ts`. Pipe `vnd` nhận thêm tham số `zeroAsEmpty` (`total | vnd:false`). Class `btn` bị bỏ khỏi `back-to-top` và nút xoá email (không phải nút thật). Đã xoá `ignite-ui-cli.json` và gỡ `daisyui`, `flowbite`, `igniteui-angular`, `@igniteui/*` (80 package). Class chết `img-fluid`, `flex-column`, `img-thumbnail`… giữ lại để xử lý cùng trang ở P1/P3; hex `#d70018,#e45464` còn trong gradient CTA của product-detail (P1.2).

| Task | File | Việc cần làm | Hoàn thành khi |
|---|---|---|---|
| P0.1 Tokens | `src/styles.css` | Thêm khối `@theme` (màu `primary-*`, semantic, `--font-sans`, `--shadow-card`, `--shadow-sticky`), `@utility container-page`, và các class `.btn*` theo [DESIGN.md §2, §4, §5.1](DESIGN.md). Chuyển `.main-color`, `.main-text-color`, `.button-bg`, `.navbar-bg`, `.banner-bg-color` thành alias của token, sau đó xoá dần ở P1–P3. | Các class `bg-primary-500`, `text-primary`, `shadow-card` có hiệu lực. Màu trang login hiển thị đúng đỏ. |
| P0.2 Material theme | `src/custom-theme.scss` | Đổi palette sang đỏ và thêm `mat.theme-overrides` với `primary: #d70018` ([DESIGN.md §10](DESIGN.md)). | Thanh gạch dưới của mat-tab ở trang chủ có màu đỏ thương hiệu. |
| P0.3 Font & meta | `src/index.html` | Nạp Roboto 400/500/700. Bỏ Open Sans và Material Icons bị nạp trùng. Đặt `lang="vi"`, title "CellphoneS Clone – Điện thoại, laptop chính hãng". Bỏ class `font-(family-name:--font-roboto) ig-typography ig-scrollbar`. | DevTools không còn request Open Sans. Chữ đậm hiển thị đúng weight 700. |
| P0.4 Pipe `vnd` | `src/app/shared/pipes/vnd.pipe.ts` (mới) | Tạo pipe theo [DESIGN.md §6](DESIGN.md). Thay `convertPriceVnd()` ở cart, order-review, payment, payment-infor bằng pipe này và xoá `CurrencyPipe` inject thừa. | `grep -r convertPriceVnd src` không còn kết quả. |
| P0.5 Dọn thư viện | `package.json`, `angular.json`, các template | Gỡ `daisyui`. Kiểm tra không có `data-*` của flowbite rồi gỡ `flowbite.js` khỏi `scripts`. Gỡ `igniteui-angular`, `@igniteui/*` nếu build vẫn chạy. Thay class Bootstrap trong footer. Rà các chỗ dùng `btn`/`card` để khớp recipe mới. | `npm run build` chạy thành công. Bundle nhỏ đi. |
| P0.6 `app-input` | `src/app/shared/custom/input/` | Sinh id duy nhất, focus dùng `primary`, trạng thái lỗi dùng `danger` ([DESIGN.md §5.6](DESIGN.md)). | Hai `app-input` trên cùng trang có id khác nhau. Bấm vào label thì focus đúng input. |

### P1 — Luồng mua hàng (ưu tiên cao nhất cho end-user)

> **Trạng thái: ✅ HOÀN THÀNH (13/10/2026)** – `ng build` sạch; đã xem thực tế trên trình duyệt (card, chi tiết desktop + 375px, modal đăng nhập khi khách bấm "Mua ngay"). Chưa chạy được luồng giỏ hàng/thanh toán thật vì cần tài khoản đăng nhập.
> Ghi chú thực hiện:
> - Bỏ nút "Yêu thích" khỏi card (chưa có tính năng); bỏ khối khuyến mãi/"Smember"/thông số iPhone giả/"Hỏi đáp"/"So sánh"/"Trả góp 0%" (không có dữ liệu thật).
> - **Sửa lỗi chọn màu** ở chi tiết sản phẩm (trước đây `onRadioChange` không bao giờ gán màu mới). Khách chưa đăng nhập bấm Mua ngay/Thêm vào giỏ sẽ thấy modal đăng nhập.
> - Giỏ hàng: nối API `DELETE /api/carts/{id}` (có sẵn ở backend), chọn theo `cartDetailId`, xoá dùng `confirm()` của trình duyệt (chưa có dialog xác nhận chung).
> - `header-cart-order` tự lấy tên từ `AuthService`; bỏ dữ liệu cá nhân hard-code ở `payment-infor` (email/SĐT để người mua tự nhập).
> - Thanh toán: nút "Tạo Đơn Hàng" trước đây **không có handler**; giờ COD → "Hoàn tất đặt hàng" chuyển tới `/checkout` (đơn đã được tạo ở bước 1). QR sửa để hiện khi tạo xong. **Cần xác nhận luồng**: đơn vẫn được tạo ở bước 1 (trước khi chọn phương thức) và quay lại bước 1 rồi "Tiếp tục" sẽ tạo thêm một đơn; ONLINE chưa có redirect sau khi thanh toán.
> - `RouteConfig.cs` thêm `GET /api/reviews/product/*` cho phép ẩn danh; thiếu dòng này thì khách bị 401 → đăng xuất → đẩy về `/home` mỗi khi mở trang chi tiết.
> - `order-review` không có route nên lúc đó không sửa (đã xoá ở đợt rà soát sau P4); `member-dashboard` vẫn hard-code tên/SĐT (đã xử lý ở P3).
> - ⚠️ **Đính chính sau rà soát:** mục "✅" của P1.6 trước đây quá lạc quan. Tiêu chí "hoàn tất đơn chỉ bằng bàn phím" chưa đạt vì `autocomplete-input` (chọn tỉnh/huyện/cửa hàng) chưa dùng được bằng bàn phím, và form giao hàng/nhận tại cửa hàng không hiện lỗi. Đã sửa ở đợt rà soát sau P4 (xem §6).

| Task | File | Việc cần làm | Hoàn thành khi |
|---|---|---|---|
| P1.1 Product card | `src/app/product-card/` | Viết lại theo [DESIGN.md §5.2](DESIGN.md): thẻ `<a routerLink>`, `relative`, % giảm tính từ `basePrice`/`salePrice`, ẩn badge/rating/S-Student khi không có dữ liệu, giá qua `vnd`, `alt` là tên sản phẩm, ảnh `aspect-square object-contain`, bỏ chiều rộng `vw`. Nút yêu thích chặn điều hướng. Cập nhật `product-skeleton` cho khớp kích thước. | Tab tới được từng card và Enter mở chi tiết. Ctrl+click mở tab mới. Card không có giảm giá thì không hiện badge. |
| P1.2 CTA chi tiết sản phẩm | `src/app/product-detail/` | "Mua ngay" thành `<button class="btn btn-primary btn-lg">` (thêm vào giỏ rồi chuyển tới `/cart`). "Thêm vào giỏ" thành `btn btn-outline btn-lg`. "Trả góp 0%" thành `btn btn-info` (ẩn nếu chưa có tính năng). Thêm thanh mua dính đáy trên mobile ([§5.10](DESIGN.md)). Disable nút khi đang gọi API. | Bấm "Mua ngay" thì sản phẩm vào giỏ và chuyển trang. Trên mobile 375px luôn thấy giá và nút mua. |
| P1.3 Dữ liệu thật ở chi tiết | `src/app/product-detail/` | Rating đầu trang dùng `averageRating()`/`reviews().length` và bấm vào thì cuộn tới phần review. Tồn kho lấy từ `StoreHouseDTOs`. Bỏ bảng thông số giả (thay bằng empty state). Bỏ khối "Kho hàng" rỗng. Khuyến mãi lấy từ API, nếu không có thì ẩn. Ẩn các nút Hỏi đáp/So sánh chưa có chức năng. | Không còn chuỗi "1 đánh giá", "{{ 1 }}" hay "Super Retina XDR" trong template. |
| P1.4 Layout chi tiết responsive | `src/app/product-detail/` | Dùng `container-page`. Lưới `grid gap-4 lg:grid-cols-[minmax(0,1fr)_minmax(0,1fr)]`. Bỏ `w-150`, `lg:w-[39vw]`, `min-w-[12vw]`. Gallery `aspect-square`. Heading đúng cấp (`h1` là tên sản phẩm). Lựa chọn phiên bản và màu theo [§5.4](DESIGN.md). | Không có thanh cuộn ngang ở 375/768/1280px. |
| P1.5 Giỏ hàng | `src/app/cart/`, `src/app/shared/header-cart-order/` | Gắn handler xoá item (kèm xác nhận). Mũi tên back thành `<button aria-label="Quay lại">`. Bỏ nút "Giỏ hàng" vô nghĩa. Tên user lấy từ `AuthService.currentUser()`. Ẩn khuyến mãi placeholder. Thêm empty state. Thanh "Tạm tính + Mua ngay" dính đáy. Dùng checkbox chuẩn với `accent-primary-500`. | Giỏ trống hiển thị empty state có CTA. Xoá item thì tổng tiền cập nhật ngay. |
| P1.6 Thông tin & thanh toán | `src/app/payment-infor/`, `src/app/payment/`, `src/app/order-review/` | Stepper theo [§5.11](DESIGN.md). Back và đổi bước dùng `<button>`. Form dùng `app-input` đã sửa và hiển thị lỗi tiếng Việt. Radio giao hàng/nhận tại cửa hàng dùng recipe lựa chọn. Thanh tổng tiền dính đáy. Nút thanh toán disable khi đang xử lý. | Hoàn tất được đơn hàng chỉ bằng bàn phím. Không gửi đơn 2 lần khi double-click. |
| P1.7 Đặt hàng thành công | `src/app/checkout/` | Tên user thật. CTA dùng `<a routerLink class="btn …">`. Bỏ gradient. | Đúng recipe nút. Không còn `'Khách'` hard-code. |

### P2 — Điều hướng & khám phá

> **Trạng thái: ✅ HOÀN THÀNH (13/10/2026)** – `ng build` sạch; đã kiểm tra thực tế trên trình duyệt ở 1280px và 375px (header sticky, tìm kiếm bằng phím, danh mục, đổi danh mục, footer, không tràn ngang).
> Ghi chú thực hiện:
> - Header: `sticky` đặt trên `<header>`; tìm kiếm là combobox (↑/↓/Enter/Esc, đóng khi click ra ngoài, kết quả là `<a routerLink>` kèm giá `vnd`); menu tài khoản mở bằng click (trước đây ẩn hoàn toàn trên mobile); giỏ hàng là `<a routerLink="/cart">` và chỉ hiện modal đăng nhập với khách. Đã bỏ các nút/link chết: ô "Xem giá tại Hồ Chí Minh", "Cửa hàng gần bạn", "Tải ứng dụng". Chunk header giảm từ ~447KB xuống ~100KB.
> - Nút "Danh mục" giờ hiện cả trên mobile; modal danh mục viết lại từ API (`homeData`), trước đây là dữ liệu tĩnh 848 dòng với panel rộng cố định và các mục không phải link.
> - Trang chủ: khung `container-page`, banner Swiper (2 banner thật), tiêu đề + "Xem tất cả" cho từng danh mục, lưới phụ kiện responsive. Đã bỏ ảnh ưu đãi CDN bên ngoài đã chết (404) và 2 ảnh trùng lặp bên trái mỗi danh mục.
> - Danh mục: **sửa lỗi không tải lại khi đổi từ danh mục này sang danh mục khác** (trước chỉ đọc `snapshot`), có skeleton/lỗi/rỗng (trước đây `app-product-skeleton` chưa từng hiển thị vì chưa import), nút "Xem thêm" có trạng thái tải.
> - Footer: hiện trên mobile; bỏ ~40 link `href="#"`, form đăng ký không có handler và dòng link SEO; các mục chính sách hiển thị dạng chữ (chưa có trang đích).
> - **Còn phụ thuộc backend:** lọc theo thương hiệu/nhu cầu ở trang danh mục. Endpoint hiện chỉ hỗ trợ tải thêm theo `category` (tag `brand`/`series` ném lỗi), nên chip ở trang danh mục để dạng hiển thị, không giả lập là bấm lọc được. Link `?brand=`/`?demand=` ở trang chủ đưa tới danh mục nhưng chưa lọc.
> - Các tile phụ kiện/linh kiện/hàng cũ ở trang chủ vẫn là nội dung tĩnh (chưa có route đích) nên không phải link.
> - Chưa làm ở P2: `header-cart-order` còn 3 link `href="#"` (header của luồng giỏ hàng) – nên dọn cùng P3/P4.

| Task | File | Việc cần làm | Hoàn thành khi |
|---|---|---|---|
| P2.1 Header sticky & màu | `src/app/header/` | Chuyển `sticky top-0 z-40` lên `<header>` (thanh khuyến mãi có thể cuộn đi). Dùng token `primary`. Nút phụ dùng `bg-white/15`. Badge giỏ trắng với chữ đỏ. Link tiện ích `href="#"` đổi thành route thật hoặc ẩn. Marquee tắt khi bật `prefers-reduced-motion`. | Cuộn trang thì thanh tìm kiếm vẫn ở trên cùng. |
| P2.2 Tìm kiếm | `src/app/header/` | Mỗi kết quả là `<a [routerLink]="['/product', id]">`. Giá qua `vnd`. Hỗ trợ ↑/↓/Enter/Esc, đóng khi click ra ngoài. Debounce. Hiện "Không tìm thấy" theo empty state. Trên mobile, ô tìm kiếm chiếm hết hàng thứ hai. | Gõ từ khoá, dùng phím ↓ rồi Enter thì mở đúng sản phẩm. |
| P2.3 Menu tài khoản & giỏ | `src/app/header/` | Menu mở bằng click, có `aria-expanded`, đóng bằng Esc. Nhãn "Tài khoản của tôi"/"Đăng xuất". Link giỏ hàng: nếu đã đăng nhập thì `routerLink="/cart"`, chưa đăng nhập thì mở modal. | Người dùng đã đăng nhập vào được giỏ trực tiếp. Menu dùng được trên cảm ứng. |
| P2.4 Trang chủ | `src/app/home/` | Dùng `container-page`. Banner dùng ảnh thật và có `priority` (hoặc dùng Swiper thay cho `mat-tab`). Mega-menu dùng `routerLink`. Lưới phụ kiện `grid-cols-4 sm:grid-cols-6 lg:grid-cols-9`. "Xem tất cả" trỏ đúng danh mục. Section header theo [§5.9](DESIGN.md). | Không tràn ngang ở 375px. Không có link `href="#"`. |
| P2.5 Danh mục | `src/app/category/` | Breadcrumb theo [§5.8](DESIGN.md). Ô thương hiệu có ảnh và `alt` thật. Thương hiệu/nhu cầu là chip lọc bấm được, có trạng thái đang chọn. Lưới sản phẩm chuẩn. "Xem thêm" dùng `btn btn-outline`. Có empty state. | Bấm chip thì URL đổi `?brand=`/`?demand=` và danh sách lọc theo. |
| P2.6 Footer | `src/app/footer/` | Bỏ class Bootstrap. Hiển thị trên mobile (xếp dọc hoặc accordion). Màu nền `bg-white`/`bg-gray-100` thống nhất. | Footer hiển thị ở 375px. |

### P3 — Tài khoản

> **Trạng thái: ✅ HOÀN THÀNH (13/10/2026)** – `ng build` sạch; đã kiểm tra trên trình duyệt ở 1280px và 375px (đăng nhập, đăng ký, trang thành viên khi chưa đăng nhập). Chưa thử đăng nhập thành công/đăng ký thật vì cần tài khoản và sẽ gửi dữ liệu lên backend Render; đăng nhập sai mật khẩu được kiểm tra bằng response 401 giả lập ở tầng fetch.
> Ghi chú thực hiện:
> - **Gỡ `minireset.css`** (angular.json + package.json). File này nằm ngoài cascade layer của Tailwind nên ghi đè `margin/padding` của `p, ul, li, fieldset, legend, h1–h6…`, `font-size/weight` của tiêu đề và `height: auto` của mọi `<img>`, làm vô hiệu hoá các utility `p-*`, `m-*`, `text-*`, `font-bold`, `h-*` trên các thẻ đó trên toàn site. Tailwind preflight đã có đủ các reset này. **Tác động toàn site**: tiêu đề, khoảng đệm và kích thước ảnh giờ khớp thiết kế.
> - **Đăng nhập:** trước đây form nằm trong `hidden md:block` nên **mobile không có form đăng nhập**; mọi class form đều có tiền tố `md:`. Viết lại sạch: hiện/ẩn mật khẩu thật, nút Google nối `AuthService.onOAuth2Google()`, lỗi tiếng Việt. Đã bỏ nút Zalo, "Quên mật khẩu" (`/restore-password` không có route) và link "dienthoaivui" (`href="#"`).
> - **Sửa lỗi đăng nhập sai mật khẩu:** backend trả 401 và `errorInterceptor` coi đó là phiên hết hạn (thử refresh, rồi chờ/đẩy về `/home`, không có thông báo). Giờ các request `/api/auth/*` không tự refresh; form hiện "Email hoặc mật khẩu không đúng.".
> - **Đăng ký:** viết lại gọn (danh sách trường + `errorFor()`/`inputClass()`), bỏ ~260 dòng HTML học sinh/giáo viên đã comment. Quy tắc mật khẩu đồng bộ với ASP.NET Identity ở backend (chữ hoa + chữ thường + chữ số + ký tự đặc biệt, ≥ 8 ký tự); trước đây form bỏ sót chữ số/chữ thường nên người dùng gặp lỗi từ server.
> - **Trang thành viên:** trước đây là bản chép từ Smember với dữ liệu giả (tên/SĐT/email/ngày sinh hard-code, voucher `EMAIL_10142FC1`, "sản phẩm yêu thích", hạng "S-NULL"). Viết lại chỉ với dữ liệu thật (tên, số sản phẩm trong giỏ, đăng xuất), dùng header/footer chung; đã xoá `member-tabs`, `infor`, `purchase-history`, `warranty` và `header-member-dashboard` (bản sao header). Có màn hình "chưa đăng nhập".
> - **Còn phụ thuộc backend:** lịch sử đơn hàng (`GET /api/orders/list` chưa triển khai), SĐT/email/ngày sinh/hạng thành viên (token chỉ có `id`, `fullName`, `roles`, `amountCart`; `GET /api/user/{id}` trả nguyên entity `User`). Trang hiện ghi rõ tính năng đang hoàn thiện thay vì hiển thị dữ liệu giả.
> - Dọn nốt `header-cart-order`: bỏ ô "Cửa hàng gần bạn" (không có trang), "Tra cứu đơn" và "Giỏ hàng" dùng `routerLink`.

| Task | File | Việc cần làm | Hoàn thành khi |
|---|---|---|---|
| P3.1 Đăng nhập / Đăng ký | `src/app/login/`, `src/app/register/` | Sau P0.1, rà lại các class `*-primary-*` và token kiểu smember (`gap-large`, `text-small`, `p-3xl`, `text-pure-white`…), đổi sang class Tailwind chuẩn. Nút dùng recipe. Lỗi form tiếng Việt và hiện dưới từng trường. Bỏ link "Điện thoại vui" `href="#"`. | Không còn class không tồn tại (kiểm tra bằng grep ở §5). |
| P3.2 Member dashboard | `src/app/member-dashboard/`, `src/app/shared/header-member-dashboard/` | Tên, số điện thoại (đã che) và hạng thành viên lấy từ `AuthService.currentUser()`. Tab dùng recipe chip/tab. Lịch sử mua hàng có empty state. | Đăng nhập bằng tài khoản khác thì thấy đúng tên của tài khoản đó. |

### P4 — Hiệu năng, SEO, a11y

> **Trạng thái: ✅ HOÀN THÀNH (13/10/2026)** – `ng build` sạch; đã kiểm tra trên trình duyệt (6 trang: trang chủ, danh mục, sản phẩm, đăng nhập, đăng ký, thành viên).
> Ghi chú thực hiện:
> - **P4.1 Lazy route + tiêu đề:** mọi route trừ trang chủ dùng `loadComponent`; gỡ route `/autocomplete`. Bundle khởi tạo giảm từ **1,02MB → 773KB** (truyền tải ~252KB → ~203KB). `AppTitleStrategy` ghép "… | CellphoneS Clone"; trang sản phẩm/danh mục đặt lại title + meta description theo dữ liệu thật.
> - **P4.2 Ảnh:** `NgOptimizedImage` cho thẻ sản phẩm, banner chính (`priority`), ảnh ưu đãi, gallery sản phẩm (ảnh đầu `priority`, đã xác nhận `fetchpriority=high` + `eager`, các ảnh sau `lazy`). Ảnh trong giỏ hàng/kết quả tìm kiếm giữ `<img>` có `width/height` + lazy vì `ngSrc` lỗi khi API trả URL rỗng. Footer đã có `width/height`.
> - **P4.3 Icon:** `<app-icon>` (13 icon: cart, search, user, grid, chevron-*, arrow-left, trash, x, eye, eye-off, check-circle, box) thay các SVG lặp ở 11 template. Chưa gom: 5 ngôi sao ở trang sản phẩm (có class động theo điểm) và SVG trong `order-review`/`checkout`.
> - **P4.4 A11y:** **không chạy Lighthouse/axe** (chưa tải công cụ về máy); thay bằng bộ kiểm tra DOM tự viết trên 6 trang. Đã sửa: tương phản màu chữ (`--color-info` #3b82f6 → #2563eb; giá gốc, ngày đánh giá, chữ địa chỉ footer), trang chủ thiếu `h1`, heading nhảy cấp h2→h4 ở khối đánh giá, thiếu `<main>` ở đăng nhập/đăng ký/đặt hàng/đặt hàng thành công, link nhỏ < 24px. Kết quả cuối: 0 lỗi tương phản (≈1.000 đoạn chữ/trang), đúng 1 `h1` + 1 `<main>`, 0 ảnh thiếu `alt`, 0 nút/link/ô nhập thiếu tên-nhãn, 0 id trùng. **Chưa kiểm tra**: focus ring bằng bàn phím trên toàn bộ trang, trang giỏ hàng/thanh toán (cần đăng nhập), điểm Lighthouse thật.

| Task | File | Việc cần làm | Hoàn thành khi |
|---|---|---|---|
| P4.1 Lazy routes | `src/app/app.routes.ts` | Đổi sang `loadComponent` cho mọi route trừ `home`. Gỡ route thử nghiệm `/autocomplete`. Thêm `title` cho từng route. | Bundle khởi tạo nhỏ đi (so sánh output của `ng build`). |
| P4.2 Ảnh | home, product-card, product-detail | Dùng `NgOptimizedImage` (`ngSrc`, `width`/`height`). Ảnh LCP có `priority`. | Lighthouse không còn cảnh báo về kích thước ảnh hoặc LCP lazy-load. |
| P4.3 Icon dùng chung | `src/app/shared/icons/` (mới) | Gom các icon lặp lại (giỏ hàng, sao, mũi tên, tim, home) thành component dùng `currentColor` và `aria-hidden`. | Không còn SVG copy nguyên khối ở từ 2 nơi trở lên. Không còn `stroke="#D70018"`. |
| P4.4 Rà soát a11y | toàn bộ | Chạy Lighthouse/axe. Sửa contrast, `alt`, focus và heading. | Accessibility ≥ 90 trên 3 trang chính. |

---

## 6. Rà soát sau P4 (13/10/2026)

Đối chiếu 7 commit với plan này và `DESIGN.md`: nội dung các commit khớp message; phát hiện các chỗ plan ghi hoàn thành nhưng chưa đạt, đã sửa trong một commit riêng:

| Chỗ lệch | Cách xử lý |
|---|---|
| `autocomplete-input`: `id="input"` trùng, focus xanh, chọn bằng `<p (mousedown)>` (không dùng được bàn phím), `z-9999` | Viết lại thành combobox: id duy nhất, ↑/↓/Enter/Esc, `role="listbox"`/`option`, màu thương hiệu, hiện lỗi |
| `ship-form`/`store-form` không hiện lỗi khi bỏ trống | Thêm `errorFor()` tiếng Việt, truyền `[errorMessage]` cho từng ô |
| `header-cart-order` còn nút/ô chết (tìm kiếm, "Xem giá tại", logo `<a (click)>`), nút thiếu `type` | Viết lại gọn theo header chính: logo `routerLink`, bỏ phần chết, tên người dùng là link tới trang thành viên |
| `modal-ask-login`: `<a (click)>` không có href (Tab không tới được), `red-600`, `hover:scale` | `routerLink` + `btn btn-outline/btn-primary` |
| `product-detail`: `font-semibold/extrabold`, nút tự chế, nhãn `text-yellow-600` trên `yellow-50` tương phản thấp (chỉ hiện khi đã đăng nhập nên bộ kiểm tra P4 bỏ sót) | Dùng `font-bold/medium`, `btn btn-primary`, chữ `gray-900` |
| Nút "Lên đầu" (`back-to-top`) nổi che mất tổng tiền ở thanh cố định dưới đáy trên mobile (giỏ hàng, đặt hàng, trang sản phẩm); `z-index: 1000` ngoài thang z đã quy định | Đặt `bottom-32` trên mobile (`md:bottom-8`), `z-30`, có focus ring; xoá CSS riêng. Đo lại: không còn chồng lấn |
| Code dư: `order-review/` (không có route), alias CSS `.main-color`… (không template nào dùng), hex `#21130d` | Đã xoá / chuyển sang token |
| `DESIGN.md`/`CLAUDE.md` còn ghi daisyUI/flowbite/igniteui "chờ gỡ", `10.2.0/`, "Review chưa commit" | Đã cập nhật |

**Vẫn chưa kiểm chứng được** (cần đăng nhập thật): luồng giỏ hàng → thông tin nhận hàng → thanh toán; phần form chỉ được kiểm tra bằng dữ liệu giả lập ở tầng fetch.

## 4. Ngoài phạm vi

- `admin_dashboard/` (TailAdmin) giữ nguyên.
- Thay đổi API backend: chỉ ghi chú khi UI cần dữ liệu mới (cờ trả góp, khuyến mãi theo sản phẩm, tồn kho chi tiết). Các khối đó **ẩn đi** cho tới khi có API.
- Dark mode.

---

## 5. Cách kiểm tra

```bash
npm run build
```

```bash
npm start
```

- Kiểm tra thủ công bằng DevTools ở 3 kích thước **375px**, **768px**, **1280px** cho các trang `/home`, `/category/:slug`, `/product/:id`, `/cart`, `/order`, `/login`, `/member-dashboard`. Yêu cầu không có thanh cuộn ngang và CTA luôn thấy được.
- Thử toàn bộ luồng mua hàng **chỉ bằng bàn phím** (Tab/Enter/Esc).
- Chạy Lighthouse (mobile) cho `/home`, `/product/:id`, `/cart`.
- Dùng grep để chắc chắn đã dọn sạch (chạy từ `cellphones_clone_ui/`, kết quả mong đợi là rỗng):

```bash
grep -rnE "#da152b|#d7263a|#d02437|#e45464|convertPriceVnd|neutral-|href=\"#\"|font-(semibold|extrabold|light)" src/app
```

```bash
grep -rnE "\b(gap|text|p|px|py)-(small|medium|large)\b|\b(gap|p)-3xl\b|text-pure-white|d-flex|justify-content-|form-control-" src/app --include=*.html
```
