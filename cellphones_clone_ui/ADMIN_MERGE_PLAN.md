# Kế hoạch: sáp nhập admin_dashboard vào cellphones_clone_ui

> **Trạng thái: đã thực hiện xong 10 bước.** Phần dưới là kế hoạch gốc; các điểm làm khác kế hoạch ghi ở đây.
>
> - **Bước 5 (UI kit):** chỉ port những thành phần có chỗ dùng: `select`, `badge`, `alert`, `table`, `pagination`, `confirm-dialog`, và mở rộng `app-input` (`min`/`max`/`step`). Không port textarea, checkbox, radio, switch, multi-select, date/time-picker, phone-input, dropdown, avatar, breadcrumb vì chưa trang nào cần; lấy lại từ lịch sử git của `admin_dashboard/` khi cần, và viết thành ControlValueAccessor như kế hoạch.
> - **Bảng:** một `app-table` bọc `<table>` thật thay cho 5 wrapper `app-table-*` của TailAdmin (wrapper đặt custom element giữa `table`/`tr` làm hỏng cấu trúc bảng).
> - **Bước 7-8:** thay vì một trang mỗi tài nguyên, dùng một `CatalogPage` + `CatalogFormDialog` + `AdminCatalogService` điều khiển bằng `admin/catalog/catalog-resources.ts`. Có 5 tài nguyên: categories, brands, series, demands, storehouses. Cam kết, thông số và hình ảnh gắn với từng sản phẩm nên làm cùng trang quản lý sản phẩm; dashboard chỉ có lối tắt (chưa có số liệu vì API thống kê còn là TODO).
> - **Bước 3:** `back-to-top` chỉ còn nằm trong `StoreShellComponent` nên không còn xuất hiện ở login, cart, order, checkout.
> - **Bước 9:** chưa thêm `flatpickr` (chưa có date-picker nào được dùng). `DESIGN.md` có thêm §12, `CLAUDE.md` và `README.md` cập nhật.
> - Đã xoá thêm 4 component trống không ai dùng (`category-accessory`, `category-audio`, `category-household`, `information`).
> - Chưa kiểm chứng giao diện `/admin` trên trình duyệt với tài khoản ADMIN thật (cần đăng nhập); các trang đã build và qua `check:boundaries`.

## Bối cảnh

- `admin_dashboard/` là template TailAdmin chưa sửa gì. Nó **không có** HttpClient, environment, interceptor, guard, model hay auth. Cả 19 route đều là trang demo dùng dữ liệu cứng. Template có 139 component, khoảng 30 cái không được dùng, và nhiều thư viện không import ở đâu.
- `cellphones_clone_ui/` đã có sẵn hạ tầng thật: auth (JWT trong bộ nhớ, refresh qua cookie, roles decode từ JWT), 3 interceptor, `NotifyService`, spinner toàn trang, token Tailwind v4 và `DESIGN.md`. Chưa có guard nào.
- Backend đã có CRUD admin ở `/api/admin/{brands,categories,demands,series,storehouses,commitments,images,specifications}` (POST/PUT/DELETE). Bản GET danh sách phân trang là các route public (`GET /api/brands?pageIndex&pageSize`…). `RouteConfig` đã chặn `/api/admin/*` cho role ADMIN.

Mục tiêu: một app Angular duy nhất. Khu `/admin` được lazy-load và chỉ ADMIN vào được. Admin dùng chung auth, HTTP và design system với storefront, không có code trùng lặp. Tận dụng những component tốt của cả hai bên, rồi xoá `admin_dashboard/`.

Các quyết định đã chốt:
1. Thư mục chia thành `core / shared / store / admin`.
2. Một design system chung (token storefront, không dark mode).
3. Chỉ lấy layout và UI kit của TailAdmin, bỏ các trang demo.
4. Xoá `admin_dashboard/` ở bước cuối.

## Cấu trúc thư mục đích (`cellphones_clone_ui/src/app`)

```
core/                 # dùng toàn app, không chứa UI
  auth/               # auth.service.ts, admin.guard.ts (mới)
  http/               # auth/error/loading interceptor, loading.context.ts
  models/             # api.model.ts (ApiResponse, PagedResult), user, product, order, cart, store, review, catalog…
  services/           # notify, loading, app-title.strategy
shared/               # thuần trình bày, chỉ được import từ core/models
  ui/                 # icon, badge, table/*, pagination, dropdown, alert, avatar, breadcrumb, card, skeleton
  form/               # input, textarea, select, multi-select, autocomplete, checkbox, radio, switch, date-picker, label
  pipes/              # vnd, (safe-html bị bỏ)
store/                # toàn bộ storefront hiện tại
  layout/             # store-shell (header, footer, back-to-top), header-cart-order
  pages/              # home, category, product-detail, cart, order/*, checkout, login, register, member-dashboard
  components/         # product-card, modal-ask-login, modal-categories, order-step-header, order-bottom-bar…
  services/           # cart, category, order, payment, product, review, store, checkout
  store.routes.ts
admin/
  layout/             # admin-shell, admin-sidebar, admin-header, backdrop, sidebar.state.ts (signals)
  pages/              # dashboard, brands, categories, demands, series, storehouses, commitments, specifications
  services/           # admin-catalog.service.ts (CRUD generic theo resource)
  admin.routes.ts
```

**Quy tắc phụ thuộc** (đây là cách đáp ứng yêu cầu "tránh ràng buộc"):
- `store` và `admin` không bao giờ import lẫn nhau.
- Cả `store` lẫn `admin` chỉ được dùng `core` và `shared`.
- `shared` chỉ được import `core/models`, không được inject service.

Cách giữ quy tắc:
- Thêm alias trong `tsconfig.json`: `@core/*`, `@shared/*`, `@store/*`, `@admin/*`.
- Thêm script `scripts/check-boundaries.mjs` (Node thuần, không thêm dependency) để quét các lệnh import sai hướng, chạy bằng `npm run check:boundaries`.

## Gộp component: giữ cái nào, bỏ cái nào

| Nhu cầu | Giữ | Xử lý |
|---|---|---|
| Ô nhập | `app-input` của store (CVA) | Bổ sung `hint`, `min`, `max`, `step` từ `app-input-field` của TailAdmin. Bỏ `app-input-field`. |
| Textarea, checkbox, radio, switch, select, multi-select | Template của TailAdmin | Viết lại thành **ControlValueAccessor** để dùng được với `formControlName`, đổi sang token storefront, chuyển vào `shared/form`. Bỏ `radio-sm` (trùng). Gộp kiểu `Option` (đang khai báo 2 lần) vào `shared/form/option.ts`. |
| Combobox tìm kiếm | `app-autocomplete-input` của store | Chuyển vào `shared/form`. |
| Chọn ngày | `app-date-picker` của TailAdmin (flatpickr) | Viết thành CVA. Chỉ trang admin import nó, nên flatpickr nằm trong chunk admin. |
| Nút | Class `.btn*` trong `styles.css` (theo DESIGN.md §5.1) | Bỏ `app-button`. |
| Icon | `app-icon` của store | Thêm các icon mà sidebar cần (dashboard, folder, tag, store, list, menu, logout…). Bỏ cách lưu SVG trong chuỗi rồi render bằng `safeHtml`, nên `safe-html.pipe` bị xoá (không còn `bypassSecurityTrustHtml`). |
| Modal | `MatDialog` (đã dùng, DESIGN.md §5.14) | Bỏ `app-modal` và `ModalService` để chỉ còn một hệ modal. |
| Bảng | `app-table*` của TailAdmin (5 wrapper) | Chuyển vào `shared/ui/table` và đổi token. |
| Badge, alert, avatar, dropdown, page-breadcrumb, component-card | TailAdmin | Chuyển vào `shared/ui` và đổi token. Màn lịch sử đơn của storefront chuyển sang dùng `app-badge` thay cho class tự viết trong `member-dashboard.component.ts`. |
| Phân trang | Chưa bên nào có | Viết mới `shared/ui/pagination` dựa trên `PagedResult`. |
| Toast, spinner | `NotifyService`, `ngx-spinner`, `SKIP_GLOBAL_SPINNER` | Admin dùng lại nguyên các thứ này. |
| Theme và sidebar state | `ThemeService` và `ModalService` bị bỏ | `SidebarService` (BehaviorSubject) chuyển thành `admin/layout/sidebar.state.ts` dùng signals. |

**Đổi token khi port** (TailAdmin → storefront):
- `brand-*` → `primary-*`
- `font-outfit` → bỏ (dùng Roboto)
- `text-theme-*` / `text-title-*` → thang chữ trong DESIGN.md §3
- `shadow-theme-*` → `shadow-card` / `shadow-lg`
- `error-*` → `danger`, `success-*` → `success`
- Xoá mọi biến thể `dark:`

**Tuyệt đối không** copy khối `@theme` của TailAdmin vào `src/styles.css`. Khối đó ghi đè `gray-*` và reset breakpoint, sẽ làm vỡ storefront.

## Các bước thực hiện (mỗi bước là một commit, build phải pass)

1. **Dọn `core` và alias.**
   - Tạo `core/models/api.model.ts` chứa `ApiResponse` và `PagedResult`.
   - Xoá bản `ApiResponse` khai báo trùng trong `services/auth.service.ts`. Chuyển `PagedResult` ra khỏi `order.model.ts`.
   - Chuyển auth service và các interceptor vào `core/auth` và `core/http`.
   - Thêm `paths` vào `tsconfig.json` và script kiểm tra ranh giới import.
2. **Chuyển storefront vào `store/`** bằng `git mv` (giữ lịch sử file), rồi sửa import theo alias.
   - Gom `services/` vào `store/services`, riêng `notify` chuyển vào `core/services`.
   - Gom `shared/custom`, `shared/icons`, `shared/pipes` vào `shared/form`, `shared/ui`, `shared/pipes`.
   - Không đổi hành vi gì ở bước này.
3. **Dùng layout route thay cho danh sách ẩn header.**
   - Tạo `store/layout/store-shell` chứa header, footer và back-to-top.
   - `app.routes.ts` có ba nhóm:
     - `''` → store-shell với các con `home`, `product/:id`, `category/:slug`, `member-dashboard`.
     - Các trang tối giản không có shell: `login`, `register`, `cart`, `order/*`, `checkout`.
     - `admin` → `loadChildren`.
     - Cuối cùng vẫn là `'**'`.
   - `app.html` chỉ còn spinner và `router-outlet`. Xoá `urlForHideHeadFooter` cùng `shouldHideHeadFooter` trong `app.ts`.
4. **Guard và luồng đăng nhập.**
   - Thêm `core/auth/admin.guard.ts` kiểu `canMatch`, kiểm tra `currentUser()?.roles.includes('ADMIN')`. Đọc đồng bộ được vì `provideAppInitializer` đã chờ `initializeAuth()` xong.
   - Chưa đăng nhập thì chuyển tới `/login?returnUrl=/admin…`. Đăng nhập rồi nhưng không phải ADMIN thì về `/home`.
   - `login.component.ts:50` điều hướng tới `returnUrl` nếu có, chỉ nhận đường dẫn nội bộ bắt đầu bằng `/`.
   - Menu tài khoản ở header storefront hiện thêm mục "Trang quản trị" khi user là ADMIN.
   - Không port trang đăng nhập/đăng ký của TailAdmin.
5. **Port UI kit vào `shared/`** theo bảng trên. Mỗi component phải đáp ứng: CVA cho các control form, có id duy nhất, label gắn với input, dùng được bằng bàn phím, nhãn tiếng Việt.
6. **Khung admin.**
   - Port `app-layout`, `app-sidebar`, `app-header`, `backdrop` thành `admin/layout/*`.
   - Menu sidebar là mảng `{ label, icon: IconName, path }` dùng `app-icon`.
   - Bỏ widget "Purchase Plan", ô search demo, dropdown thông báo demo và các `console.log`.
   - Header admin có tên user (lấy từ `currentUser`), link "Về cửa hàng" và nút Đăng xuất (gọi `AuthService.logout()`).
7. **Trang admin đầu tiên: Thương hiệu (brands). Đây là mẫu cho các trang khác.**
   - `admin/services/admin-catalog.service.ts` viết theo dạng generic với tham số `resource`:
     - `list` gọi `GET /api/{resource}`.
     - `create` gọi `POST /api/admin/{resource}`, `update` gọi `PUT /api/admin/{resource}/{id}`, `remove` gọi `DELETE /api/admin/{resource}/{id}`.
   - Trang gồm bảng, phân trang, dialog thêm/sửa (`MatDialog` cùng form reactive) và xác nhận xoá.
   - Có đủ các trạng thái: đang tải, rỗng, lỗi.
8. **Nhân mẫu cho các tài nguyên còn lại:** categories, demands, series, storehouses, commitments, specifications.
   - Images gắn với sản phẩm/màu, nên xử lý khi có trang sản phẩm.
   - Trang dashboard ban đầu chỉ có các ô thống kê lấy từ API đang có. Chưa port biểu đồ: các endpoint thống kê `orders/today|month|quarter|year` vẫn là TODO, nên chưa thêm `apexcharts`.
9. **Dependency và tài liệu.**
   - Thêm `flatpickr`. Chỉ thêm `apexcharts`/`ng-apexcharts` khi đã có API thống kê.
   - Không mang sang: fullcalendar, amcharts, prismjs, ng-otp-input, ngx-drag-drop, popper. Nếu cần dropdown định vị thì dùng `@angular/cdk/overlay`, đã có sẵn.
   - Sửa `DESIGN.md`: bỏ câu "không áp dụng cho admin", thêm mục "Admin" (layout sidebar, bảng, phân trang, form) và cập nhật bảng thư viện ở §10.
   - Cập nhật `CLAUDE.md` (layout repo, quy tắc thư mục, guard). Ghi chú trong `UI_IMPROVEMENT_PLAN.md`.
10. **Xoá `admin_dashboard/`** khi các bước trên đã build pass. Cập nhật `CLAUDE.md`.

## File chính bị ảnh hưởng

- `cellphones_clone_ui/tsconfig.json`: thêm `paths`.
- `cellphones_clone_ui/package.json`: thêm `flatpickr` và script `check:boundaries`.
- `src/app/app.routes.ts`, `app.ts`, `app.html`, `app.config.ts`: layout route, `loadChildren` cho admin, đường dẫn interceptor mới.
- `src/app/services/auth.service.ts` → `core/auth/`. Thêm mới `core/auth/admin.guard.ts`.
- `src/app/login/login.component.ts`: hỗ trợ `returnUrl`.
- `src/app/header/header.component.*`: mục "Trang quản trị".
- Thêm mới: `src/app/admin/**`, `src/app/shared/form/*`, `src/app/shared/ui/*`.
- `DESIGN.md`, `CLAUDE.md`.
- Backend không cần đổi gì: CORS đã cho phép `localhost:4200` và domain Vercel.

## Kiểm tra

Sau mỗi bước:
- `npm run build` pass.
- `npm run check:boundaries` không báo lỗi.
- Dung lượng bundle initial không tăng (admin phải nằm trong lazy chunk; xem output build hoặc `source-map-explorer`).

Thử trên trình duyệt (`npm start`, backend `dotnet run`):
1. Storefront không đổi: home, danh mục, sản phẩm, giỏ, các bước `/order/*`, checkout, tài khoản. Header/footer hiện và ẩn đúng như trước. Thử ở 375px và 1280px.
2. Chưa đăng nhập vào `/admin` thì bị chuyển tới `/login?returnUrl=/admin`. Đăng nhập tài khoản ADMIN thì quay lại `/admin`. Tài khoản USER thì bị về `/home`. Không thấy request `/api/admin/*` nào trả 403.
3. Trang Thương hiệu: thêm, sửa, xoá, phân trang. Toast và spinner hoạt động. Tải lại trang vẫn đăng nhập (refresh cookie).
4. Sidebar admin thu gọn/mở được, bản mobile dùng backdrop, đóng được bằng Esc.
5. Bundle storefront không chứa flatpickr hay code admin (kiểm tra bằng `source-map-explorer dist/**/main*.js`).
