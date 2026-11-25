# Các điểm cần cải thiện — CellphoneS Clone

> Khảo sát ngày 08/11/2026 bằng cách đọc code (`cellPhoneS_backend/` và `cellphones_clone_ui/`), **chưa chạy thử**.
> Các mục ghi "cần kiểm chứng" là suy luận từ code, nên tái hiện trước khi sửa.
>
> Không lặp lại những gì đã có trong:
> - `cellPhoneS_backend/docs/ADMIN_API_GUIDE.md` §0.2 (Phase 0): ghi sản phẩm qua `/api/products` chỉ cần đăng nhập, `/api/user/*` trả entity `User` (có `PasswordHash`) và cho xoá người khác, khoá tài khoản / blacklist JWT không có tác dụng, validator FluentValidation không được chạy.
> - `cellphones_clone_ui/UI_IMPROVEMENT_PLAN.md`: màu, font, class Tailwind sai, link `href`, a11y.

Mức ưu tiên: **P0** = sai tiền / sai kho / lỗ hổng, sửa ngay · **P1** = sai nghiệp vụ hoặc chậm thấy rõ · **P2** = thiết kế, dọn dẹp.

---

## Tóm tắt nhanh

| # | Ưu tiên | Vấn đề | Loại |
|---|---|---|---|
| L1 | P0 | Trừ kho kiểu "đọc rồi ghi đè" → bán vượt tồn kho khi đặt đồng thời | Logic |
| L2 | P0 | Đơn giao hàng trừ kho **trước** khi mở transaction; gọi GHTK lỗi thì kho mất mà không có đơn | Logic |
| L3 | P0 | Phí ship tính sai: trọng lượng bị nhân 500 hai lần; GHTK lỗi thì phí = 0 | Logic |
| L4 | P0 | API giỏ hàng (+/−/xoá) không kiểm tra chủ sở hữu (IDOR) | Logic / bảo mật |
| L5 | P0 | Huỷ được đơn đã thanh toán, hoàn tiền là stub luôn `true` | Logic |
| L6 | P1 | Đơn `Pending` giữ kho vĩnh viễn, không có job huỷ đơn quá hạn | Logic |
| L7 | P1 | Lọc theo brand/series trả về **mọi** sản phẩm của category | Logic |
| L8 | P1 | `Register` bỏ qua kết quả `CreateAsync` | Logic |
| L9 | P1 | `DeleteProduct` không `SaveChanges` | Logic |
| P1 | P1 | Trang chủ tính lại toàn bộ mỗi request, không cache | Hiệu năng |
| P2 | P1 | Polling thanh toán 3 giây/lần, mỗi lần backend gọi gateway | Hiệu năng |
| P3 | P1 | App Angular chặn bootstrap chờ `/auth/isLoggedIn` (Render ngủ → trắng trang 30–60 s) | Hiệu năng |
| P4 | P1 | Rate limit theo IP nhưng thiếu `ForwardedHeaders` → mọi người dùng chung 1 bucket | Hiệu năng / logic |

---

## 1. Logic nghiệp vụ

### L1 (P0) Bán vượt tồn kho do race condition

[StoreRepository.UpdateRangeAsync](cellPhoneS_backend/src/Stores/Infrastructure/Persistence/Repositories/Implementations/StoreRepository.cs#L66) ghi **giá trị tuyệt đối** `SET "Quantity" = {s.Quantity}` sau khi
[AllocateInventoryCoreAsync](cellPhoneS_backend/src/Orders/Application/Services/OrderServiceImpl.cs#L575) đọc tồn kho lên RAM và trừ ở C#.
Hai người cùng đặt 1 máy còn 1 chiếc: cả hai đọc `Quantity = 1`, cả hai ghi `0`, cả hai có đơn. Không có `CHECK ("Quantity" >= 0)` hay concurrency token để chặn.

**Cách sửa:** trừ kho nguyên tử trong DB và kiểm tra số dòng bị ảnh hưởng:

```csharp
var affected = await _context.Stores
    .Where(s => s.StoreHouseId == sh && s.ProductId == p && s.ColorId == c && s.Quantity >= take)
    .ExecuteUpdateAsync(u => u
        .SetProperty(s => s.Quantity, s => s.Quantity - take)
        .SetProperty(s => s.UpdateDate, DateTime.UtcNow));
if (affected == 0) throw new ConflictException("Sản phẩm vừa hết hàng, vui lòng thử lại.");
```

Thêm migration `CHECK ("Quantity" >= 0)` trên bảng `Stores` làm lưới an toàn cuối. Bỏ hẳn SQL ghép chuỗi trong `UpdateRangeAsync` và `RestoreInventoryAsync` (dùng `ExecuteUpdateAsync`, `UpdateDate` hiện được ghi bằng chuỗi không có múi giờ).

### L2 (P0) Trừ kho nằm ngoài transaction (đơn giao hàng)

Trong [CreateDeliveryOrder](cellPhoneS_backend/src/Orders/Application/Services/OrderServiceImpl.cs#L315): `AllocateInventoryCoreAsync` (đã ghi DB) → `CalculateDeliveryShippingFeeAsync` (gọi HTTP GHTK) → **sau đó** mới `BeginTransactionAsync`.
Nếu không đủ hàng ở sản phẩm thứ 2 thì không sao (ném lỗi trước khi ghi), nhưng nếu GHTK/lưu đơn lỗi thì kho đã bị trừ mà không có đơn, và rollback không hoàn lại được.
`CreatePickupOrder` thì đúng thứ tự (allocate trong transaction).

**Cách sửa:** tính phí ship (chỉ đọc + HTTP) **trước**, rồi mới mở transaction bao cả trừ kho và lưu đơn. Không gọi HTTP bên trong transaction.

### L3 (P0) Phí ship sai

1. [OrderServiceImpl.CalculateDeliveryShippingFeeAsync](cellPhoneS_backend/src/Orders/Application/Services/OrderServiceImpl.cs#L689) truyền `totalWeight = qty * 500` vào tham số tên `TotalQuantity` của `PackageToShip`;
   [ShippingFeeServiceImpl](cellPhoneS_backend/src/Shipping/Application/Services/ShippingFeeServiceImpl.cs#L38) lại tính `Weight = TotalQuantity * 500`. Kết quả: 1 máy = 250 000 g gửi lên GHTK.
2. `CallGHTKApi` nuốt mọi lỗi và trả `null` ([dòng 78, 84](cellPhoneS_backend/src/Shipping/Application/Services/ShippingFeeServiceImpl.cs#L78)), nên phí cộng `0` → GHTK sập là **miễn phí ship**.
3. Các kiện được gọi tuần tự; dùng `Task.WhenAll` để gọi song song.

**Cách sửa:** đổi `PackageToShip` thành `(PickProvince, PickDistrict, int WeightGram)`, chỉ nhân một chỗ; GHTK lỗi thì ném `BadGatewayException` (để khách thử lại) hoặc dùng bảng phí dự phòng có chủ đích, không bao giờ trả 0 im lặng. Đưa `500 g/máy` thành cấu hình hoặc trường cân nặng của sản phẩm.

### L4 (P0) IDOR trên giỏ hàng

[CartServiceImpl.MinusQuantity / PlusQuantity / RemoveFromCart](cellPhoneS_backend/src/Cart/Application/Services/CartServiceImpl.cs#L84) nhận `userId` nhưng không dùng: chỉ cần đoán `cartDetailId` là sửa/xoá được giỏ của người khác.
**Cách sửa:** truy vấn `Where(c => c.Id == id && c.CreateBy == userId)`; không thấy thì `NotFoundException`.

Cùng file:
- `PlusQuantity` so sánh `Quantity == totalQuantity` ([dòng 103](cellPhoneS_backend/src/Cart/Application/Services/CartServiceImpl.cs#L103)); khi kho giảm xuống dưới số trong giỏ thì điều kiện không bao giờ đúng → dùng `>=`.
- `AddToCart` không kiểm tra màu có thuộc sản phẩm không, không kiểm tra tồn kho, còn `Console.WriteLine` debug.
- `CartRepository.GetCartByUserIdIfNotThenCreate` có race tạo 2 giỏ cho 1 user (đã có quan hệ 1-1, nên request thứ hai sẽ nổ 500) → bắt `DbUpdateException` và đọc lại.

### L5 (P0) Huỷ đơn đã thanh toán mà không hoàn tiền

`Order.CanBeCancelled` cho phép huỷ mọi trạng thái trừ `Completed/Cancelled`, kể cả `Paid`. [DeleteOrder](cellPhoneS_backend/src/Orders/Application/Services/OrderServiceImpl.cs#L401) hoàn kho rồi gọi [ProcessRefundIfNecessaryAsync](cellPhoneS_backend/src/Orders/Application/Services/OrderServiceImpl.cs#L737) luôn trả `true`. Khách đã trả tiền tự huỷ được đơn, tiền không được hoàn và không ai được báo.
**Cách sửa:** trước khi có luồng hoàn tiền thật, người dùng chỉ được huỷ đơn `Pending`; huỷ đơn `Paid` chuyển sang admin (trạng thái `RefundRequested`). Đưa luật này vào `Order.CanBeCancelledBy(...)`.

Liên quan:
- `MarkOrderPaid` kiểm tra "đã có giao dịch Success chưa" bằng `AnyAsync` rồi `Add` → callback và polling đồng thời có thể ghi 2 dòng. Thêm unique index một phần `(OrderId) WHERE Status = 'Success'` trên `PaymentTransactions`.
- Đơn đã huỷ nhưng gateway báo `PAID` chỉ được `LogError`. Nên lưu thành trạng thái cần xử lý (vd. `PaidAfterCancel`) để admin thấy trên dashboard, không chỉ trong log.
- Các nhánh `catch (Exception ex)` ném `InternalServerException($"...{ex.Message}")` → lộ thông điệp nội bộ (SQL, stack) ra client. Ghi log `ex`, trả thông điệp chung.

### L6 (P1) Đơn chờ thanh toán giữ kho vĩnh viễn

Kho bị trừ ngay khi tạo đơn, nhưng không có gì huỷ đơn `Pending` bỏ dở. Job `sync-online-payments` chỉ hỏi lại gateway các đơn **đã từng** thử thanh toán trong 24 h. Đơn COD/đơn bỏ ngang làm "kẹt" hàng mãi.
**Cách sửa:** job Hangfire `expire-pending-orders` (vd. 15 phút/lần): đơn `Pending` quá N phút (lớn hơn thời hạn link PayOS) và gateway không báo `PAID/PENDING` → `Cancel` + hoàn kho (dùng lại logic của `DeleteOrder`). Thêm index `(Status, UpdateDate)` cho truy vấn này.

### L7 (P1) Lọc theo thương hiệu/dòng sản phẩm sai

`Product` không có `BrandId`/`SeriesId`. [GetProductsByBrandId / GetProductsBySeriesId](cellPhoneS_backend/src/Catalog/Infrastructure/Persistence/Repositories/Implementations/CategoryProductRepository.cs#L12) lọc `cp.Category.Brands.Any(b => b.Id == brandId)`, tức "sản phẩm thuộc category có brand này" → chọn Apple vẫn ra Samsung.
**Cách sửa (thiết kế dữ liệu):** thêm `Product.SeriesId` (nullable, FK) — brand suy ra qua `Series.BrandId`; hoặc `Product.BrandId` + `SeriesId`. Migration backfill từ tên sản phẩm/dữ liệu crawl.

### L8 (P1) Đăng ký bỏ qua lỗi tạo user

[AuthServiceImpl.Register](cellPhoneS_backend/src/Account/Application/Services/AuthServiceImpl.cs#L114) và luồng Google ([dòng 157](cellPhoneS_backend/src/Account/Application/Services/AuthServiceImpl.cs#L157)) không kiểm tra `IdentityResult`. Mật khẩu không đạt policy → `AddToRoleAsync` trên user chưa lưu → 500 khó hiểu (hoặc user tồn tại không có role). Kiểm tra `result.Succeeded` và trả `BadRequestException` với `result.Errors`. Tạo role `USER` một lần ở `AdminSeeder` thay vì mỗi lần đăng ký.

Cùng file:
- Login trả "Email not found" vs "Incorrect password" → lộ email nào đã đăng ký. Dùng một thông điệp chung.
- `CheckPasswordAsync` không đếm sai mật khẩu; dùng `SignInManager.CheckPasswordSignInAsync(user, pwd, lockoutOnFailure: true)`.
- Logout ([dòng 200](cellPhoneS_backend/src/Account/Application/Services/AuthServiceImpl.cs#L200)) chỉ xoá cookie, không xoá `rt:{token}` trong Redis → refresh token cũ vẫn dùng được 7 ngày nếu bị lộ.
- Phát hiện tái sử dụng refresh token chỉ xoá đúng key đó; `SessionId` có sẵn nhưng không dùng để thu hồi cả "họ" token. Lưu `session:{id}` → huỷ mọi token của phiên khi phát hiện reuse.
- Cookie option không thống nhất: Login/Google `Secure = false, SameSite = Strict`, Refresh `Secure = IsHttps, SameSite = Lax`, Logout `Lax`. Frontend (vercel.app) và backend (onrender.com) **khác site**, nên cookie `Strict`/`Lax` không được gửi kèm request `fetch` → refresh có thể hỏng ở production (*cần kiểm chứng*). Gom về một hàm `BuildRefreshCookieOptions()` với `SameSite = None; Secure = true` (production), và bật `UseForwardedHeaders` để `IsHttps` đúng sau proxy Render.

### L9 (P1) Các lỗi nhỏ khác

- [ProductServiceImpl.DeleteProduct](cellPhoneS_backend/src/Products/Application/Services/ProductServiceImpl.cs#L242) gọi `UpdateAsync` nhưng không `SaveChangesAsync` → không bao giờ xoá được.
- `BaseRepository.RemoveAsync` thực chất gọi `UpdateAsync` (không xoá gì), dễ gây hiểu nhầm. Đổi tên thành `SoftDelete` và tự gán `Status = "deleted"`, hoặc xoá thật.
- [CategoryRepository](cellPhoneS_backend/src/Catalog/Infrastructure/Persistence/Repositories/Implementations/CategoryRepository.cs#L75) dùng `FirstAsync` → slug/id không tồn tại ra 500 thay vì 404. Trang chủ/category không lọc sản phẩm `deleted`.
- Phân trang không nhất quán: `ProductRepository`/`CategoryProductRepository` dùng `Skip(page * size)` (0-based), `CategoryRepository`/admin dùng `(page - 1) * size` (1-based). [ProductRepository.GetAllAsync](cellPhoneS_backend/src/Products/Infrastructure/Persistence/Repositories/Implementations/ProductRepository.cs#L15) còn không có `OrderBy` → thứ tự trang không xác định. `page`/`size` từ URL không được giới hạn (`size = 100000` tải cả bảng).
- `GetCategoryDetail` sắp theo `CreateDate`, `GetCategoryDetailBySlug` theo `Id` → cùng category, hai endpoint ra thứ tự khác.
- Đăng nhập Google (*cần kiểm chứng*): không có cookie scheme cho `SignInScheme` (mặc định là JwtBearer, không sign-in được); redirect về `/api/auth/callbackGoogle` nhưng `RouteConfig` chỉ mở `/api/auth/callBack/google`, nên rơi vào catch-all 401; `OnRemoteFailure` hard-code `http://localhost:4200/login`.
- Endpoint trả `null!` hoặc `NotImplementedException`: `changePass`, `GET refreshToken`, `oauth2-zalo`, `studentRegister`, `teacherRegister`, `CreateProduct`, `UpdateProduct`, `SearchProducts`, `OrderService.GetOrders/GetOrderById`. Hoặc làm, hoặc xoá route để không lộ API "ma".

---

## 2. Hiệu năng

### P1 (P1) Cache trang chủ và danh mục

`GET /api/home` ([CategoryRepository.InitHomeByCategories](cellPhoneS_backend/src/Catalog/Infrastructure/Persistence/Repositories/Implementations/CategoryRepository.cs)) chạy mỗi request: mọi category × demand × brand × N sản phẩm, rồi `ProductCardEnricher` thêm 3 truy vấn (tồn kho, review, thông số). Dữ liệu này hầu như tĩnh.
**Cách sửa:** `AddOutputCache()` + `[OutputCache(Duration = 60, Tags = ["catalog"])]` cho `home`, `category/*`, `products/*` (GET); admin ghi catalog/sản phẩm/kho thì `EvictByTagAsync("catalog")`. Hoặc `IMemoryCache.GetOrCreateAsync` như `CacheService` đang làm cho kho.
Bật thêm `AddResponseCompression()` (Brotli/Gzip) — payload trang chủ JSON lớn, Render free tier băng thông chậm.

### P2 (P1) Polling thanh toán khuếch đại tải lên gateway

Trang `/payment/:orderId` poll **3 giây/lần** ([online-payment.component.ts:61](cellphones_clone_ui/src/app/store/pages/online-payment/online-payment.component.ts#L61)); mỗi lần backend `GetOnlinePayment` → `SyncOrderPaymentAsync` → `GET` gateway, và nếu `PAID` thì `MarkOrderPaid` tải lại đơn lần nữa. 10 khách đang thanh toán = 200 request/phút tới gateway.
**Cách sửa:**
- Backend: cache kết quả `GetStatus(orderId)` 3–5 s trong `IMemoryCache` (key theo orderId); trạng thái kết thúc (`PAID`, `CANCELLED`…) cache lâu hơn. Callback từ gateway thì xoá cache.
- Frontend: poll giãn dần (3 s → 5 s → 10 s), dừng khi tab ẩn (`document.visibilityState`), và dừng hẳn khi trạng thái kết thúc.
- Lâu dài: SSE/WebSocket (SignalR) đẩy trạng thái khi callback tới.
- `MarkOrderPaid` nhận `Order` đã nạp thay vì `orderId` để khỏi query lại; `SyncPendingOnlinePayments` gọi gateway tuần tự — dùng `Parallel.ForEachAsync` với `MaxDegreeOfParallelism = 4` (mỗi vòng một scope/DbContext riêng).

### P3 (P1) Frontend chặn khởi động chờ backend

`provideAppInitializer(() => authService.initializeAuth())` ([app.config.ts:36](cellphones_clone_ui/src/app/app.config.ts#L36)) chặn render toàn app tới khi `POST /auth/isLoggedIn` trả về. Backend Render free tier ngủ sau 15 phút → người dùng thấy **trang trắng 30–60 s**, kể cả khách chưa đăng nhập.
**Cách sửa:** không trả Observable từ initializer (gọi `initializeAuth().subscribe()` và để header hiển thị trạng thái "đang kiểm tra"), hoặc `race` với `timer(3000)`. Guard nào cần đăng nhập thì tự chờ `isLoggedIn$`.
Thêm: `isLoggedIn` hiện **xoay vòng refresh token** ở mỗi lần tải trang (tác dụng phụ trên một thao tác "kiểm tra"); tab thứ hai mở cùng lúc có thể bị tính là reuse → đăng xuất (*cần kiểm chứng*).

### P4 (P1) Rate limit sau proxy

`AuthLimit` phân vùng theo `Connection.RemoteIpAddress`, nhưng app không gọi `UseForwardedHeaders` → sau proxy Render mọi request có cùng IP proxy → **cả hệ thống chung 10 lần đăng nhập/phút**. `GlobalLimit` được khai báo nhưng không gắn vào endpoint nào. `RateLimitingMiddleware`/`RateLimitConfig` (275 dòng) là code chết.
**Cách sửa:** `builder.Services.Configure<ForwardedHeadersOptions>(o => { o.ForwardedHeaders = XForwardedFor | XForwardedProto; o.KnownNetworks.Clear(); o.KnownProxies.Clear(); })` + `app.UseForwardedHeaders()` đầu pipeline; dùng `GlobalLimiter` partition theo user id/IP; xoá thư mục `RateLimit` cũ.

### P5 (P2) Tìm kiếm

- [ProductSearchServiceImpl](cellPhoneS_backend/src/Products/Application/Services/ProductSearchServiceImpl.cs#L96) chạy `Fuzz.TokenSetRatio` trên **toàn bộ** sản phẩm mỗi lần gõ phím. Ổn với ~2 500 sản phẩm, nhưng: lọc sơ bộ bằng prefix/token index trước khi fuzzy, và frontend cần `debounceTime(300)` + `distinctUntilChanged` + `switchMap`.
- `mv_ProductSearch` là materialized view được tạo **ngoài migration** và không có chỗ nào `REFRESH` → sản phẩm mới/đã xoá/đổi giá không lên kết quả tìm kiếm cho tới khi ai đó refresh tay. Đưa `CREATE MATERIALIZED VIEW` vào migration, refresh `CONCURRENTLY` trong job Hangfire hoặc sau khi admin ghi sản phẩm, rồi gọi `RefreshCache()`.
- Thay thế bền hơn: Postgres `pg_trgm` + `unaccent` (index GIN) — bỏ được cache RAM và tự động đúng dữ liệu. `docker-compose.yml` đang dựng Elasticsearch nhưng code không dùng: hoặc dùng, hoặc bỏ cho đỡ nhầm.

### P6 (P2) Truy vấn và DB

- Không có `CancellationToken` từ controller xuống EF ở hầu hết endpoint → client huỷ request thì truy vấn vẫn chạy. Truyền `HttpContext.RequestAborted`.
- Truy vấn chỉ đọc nên dùng `AsNoTracking()` (giỏ hàng, danh mục, review…). Có thể bật mặc định `QueryTrackingBehavior.NoTracking` và chỉ `AsTracking()` ở nơi ghi.
- `JwtTokenServiceImpl.GenerateJwtToken` query số lượng giỏ hàng mỗi lần phát token (cả refresh) để nhét claim `amountCart` — vừa tốn query vừa sai ngay khi khách thêm vào giỏ. Bỏ claim, frontend gọi `GET /api/carts/amount`.
- Index nên thêm: `Orders (CreateBy, CreateDate DESC)` cho lịch sử đơn, `Orders (Status, UpdateDate)` cho job, `Products (Status)`, GIN trigram cho tìm kiếm admin (`ILIKE '%x%'` trong `AdminQueryServiceImpl`).
- `GetProductDetails` chạy 2 subquery giống hệt nhau trên `Stores` (màu và kho); gộp thành một rồi tách ở C#.
- `ReviewServiceImpl.GetAverageRatingAsync` tải mọi review lên RAM để tính trung bình → `AverageAsync` trong DB. Danh sách review không phân trang.
- `ImageService.GetAllImages` trả toàn bộ ảnh không phân trang.

### P7 (P2) Frontend

- Chỉ 1/53 component dùng `ChangeDetectionStrategy.OnPush`; app đã dùng signal nhiều, chuyển dần sang OnPush (hoặc zoneless `provideZonelessChangeDetection`) để giảm chu kỳ change detection — đặc biệt trang thanh toán có 2 `timer` chạy mỗi giây.
- ~15 thẻ `<img>` không có `loading="lazy"`/`NgOptimizedImage`; ảnh R2 nên có kích thước cố định (`width/height`) để tránh layout shift.
- Nhiều component admin `subscribe()` không có `takeUntilDestroyed` (dialog, images, commitments…). Rủi ro rò rỉ thấp vì là HTTP một lần, nhưng nên thống nhất.
- Budget `initial` 2 MB/4 MB là rộng; `hammerjs`, `@types/jquery`, `express` (SSR đang tắt) còn trong `dependencies`. Kiểm tra bundle bằng `ng build --stats-json` + esbuild analyzer.

---

## 3. Thiết kế

### D1 (P1) Ranh giới transaction và Unit of Work

Repository vừa tự `SaveChangesAsync` (vd. `CartRepository.CreateCartForUser`, `CartDetailRepository.RemoveCartItems`) vừa để service gọi `SaveChanges`; `OrderServiceImpl` dùng thẳng `ApplicationDbContext` song song với repository. Khó biết thao tác nào nằm trong transaction nào (gốc của L2).
**Đề xuất:** repository **không bao giờ** `SaveChanges`; service gọi `IUnitOfWork.SaveChangesAsync()` một lần ở cuối use case. Hoặc chấp nhận dùng `DbContext` trực tiếp trong service (EF đã là UoW/repository) và bỏ lớp repository mỏng — chọn một kiểu cho toàn dự án.

### D2 (P1) `OrderServiceImpl` quá lớn (758 dòng)

Gồm tạo đơn, phân bổ kho, phí ship, huỷ đơn, hoàn kho, thanh toán online, đồng bộ gateway, callback. Tách:
- `InventoryAllocator` (phân bổ + trừ/hoàn kho nguyên tử — L1),
- `OrderPaymentService` (StartOnlinePayment, Sync, Callback, MarkPaid),
- `OrderService` (tạo/huỷ/xem đơn).
`CreateDeliveryOrder` và `CreatePickupOrder` trùng gần hết → một hàm `CreateOrderAsync(userId, OrderPlacement)`.

### D3 (P1) Mô hình dữ liệu

- **Tiền dùng `double`** (`Product.BasePrice/SalePrice`, `OrderDetail.Price`) rồi `Math.Round` khi tính tổng. VND không có phần lẻ: dùng `long` (hoặc `decimal(18,0)`). Tránh sai số khi cộng nhiều dòng và khi so số tiền với gateway.
- **Thông số kỹ thuật dùng chung giá trị:** `SpecificationDetail` thuộc `Specification`, còn `ProductSpecification` chỉ là bảng nối. Nếu hai sản phẩm dùng chung một `Specification` thì dùng chung luôn giá trị (RAM, pin…). Giá trị phải gắn với sản phẩm: `ProductSpecificationValue(ProductId, SpecificationDetailId/Key, Value)`.
- Thiếu `BrandId/SeriesId` trên `Product` (L7).
- `Status` là chuỗi tự do (`"active"`, `"deleted"`; điều kiện `Status != "deleted"` lặp lại khắp nơi, có chỗ quên). Dùng enum + `HasConversion<string>()` và **global query filter** `HasQueryFilter(p => p.Status != Deleted)` để không quên lọc.
- Audit (`CreateBy/UpdateBy/CreateDate/UpdateDate`) lặp ở mọi entity và gán tay ở mọi service. Gom vào `AuditableEntity` + `SaveChangesInterceptor` tự điền.
- `Payment` / `PaymentTransaction`: CLAUDE.md nói "không lưu payment row" nhưng `DbSet<Payment>` vẫn còn. Xoá entity/bảng không dùng.

### D4 (P2) Tổ chức code

- Namespace không khớp thư mục (`cellphones_backend.*` vs `cellPhoneS_backend.*`), interface đặt tên lẫn lộn (`ProductService` vs `IReviewService`). Làm một đợt đổi tên cơ học (IDE refactor) để namespace = đường dẫn và mọi interface có tiền tố `I`.
- DI đăng ký tay, `ShippingFeeService` đăng ký **hai lần**. Có thể dùng Scrutor `Scan(...)` theo quy ước `I<Name>` → `<Name>Impl`.
- `ShippingFeeServiceImpl` nhận `HttpClient` mặc định (không timeout/base URL riêng) — đăng ký typed client như gateway/file-service. Cấu hình HTTP client rải trong `Program.cs`; gom vào `AddExternalClients()` và dùng `IOptions<T>` có `ValidateOnStart()` thay vì `_configuration["..."]!` (thiếu key sẽ nổ khi chạy thay vì khi khởi động).
- Thêm `AddStandardResilienceHandler()` (Polly: retry + timeout + circuit breaker) cho GHTK, provinces API, file-service. Không bật retry cho `POST checkout` của gateway ngoài cơ chế Idempotency-Key đã có.
- Dùng `ILogger` thay `Console.WriteLine` (AdminSeeder, Cart, Shipping, `Program.cs`).
- Lỗi không phải nghiệp vụ hiện để ASP.NET trả 500 trống; thêm `AddProblemDetails()` + `UseExceptionHandler()` để có body thống nhất và log có traceId.
- Phân quyền tập trung theo URL (`RouteConfig`) dễ quên khi thêm route (vd. callback Google). Kế hoạch trong `ADMIN_API_GUIDE.md` (permission attribute) giải quyết việc này; đến lúc đó nên có test liệt kê mọi endpoint và policy khớp với nó.

### D5 (P2) Frontend

- `errorInterceptor` giữ `isRefreshing`/`refreshTokenSubject` ở cấp module: khi refresh trả `null` hoặc lỗi, các request đang chờ ở nhánh `refreshTokenSubject.pipe(filter(t => t !== null), take(1))` **treo mãi** (không bao giờ có token mới). Phát một giá trị lỗi/hoàn tất cho hàng đợi khi refresh thất bại; chuyển state vào service.
- `refreshToken()` trong `AuthService` tự `logout()` + `navigate` bên trong `catchError`, còn interceptor lại `handleLogout()` lần nữa → hai nơi quyết định đăng xuất. Gom về một chỗ.
- Bỏ khối comment dài "suy nghĩ" trong `error.interceptor.ts`; thông điệp lỗi trộn Anh/Việt (`400 Bad Request: ...`).
- Kiểu `any` ở `register(data: any)`, `decoded: any`… → định nghĩa model.

---

## 4. Kiểm thử và vận hành

- Không có dự án test và không có CI (`.github/workflows/` rỗng). Tối thiểu nên có:
  - Unit test cho `Order` (luật trạng thái), phân bổ kho, tính phí ship (đã có bug L3 lẽ ra test bắt được).
  - Integration test (`WebApplicationFactory` + Testcontainers Postgres) cho: đặt hàng đồng thời không bán vượt kho (L1), IDOR giỏ hàng (L4), `RouteConfig` (mọi endpoint ghi đều cần quyền đúng).
  - CI: `dotnet build` + test, `npm run build` + `npm run check:boundaries` + `ng test --watch=false`.
- Health check thật (`AddHealthChecks().AddNpgSql().AddRedis()`) thay vì endpoint trả cứng; Render dùng nó để biết app sẵn sàng.
- Hangfire dashboard `/hangfire`: mặc định chỉ cho request local; nếu cần xem ở production thì thêm `IDashboardAuthorizationFilter` kiểm tra role ADMIN.
- Seed/warm-up trong `Program.cs` nuốt lỗi bằng `Console.WriteLine`; dùng `ILogger` và cân nhắc chạy warm-up nền (`IHostedService`) để không làm chậm khởi động trên Render.

---

## 5. Thứ tự đề xuất

1. **Tuần 1 (P0):** L1 + L2 (trừ kho nguyên tử, transaction đúng chỗ, CHECK constraint) → L3 (phí ship) → L4 (IDOR giỏ) → L5 (chặn huỷ đơn đã trả tiền, unique index giao dịch). Kèm Phase 0 trong `ADMIN_API_GUIDE.md`.
2. **Tuần 2 (P1 logic):** L6 job hết hạn đơn, L8 auth (Register, logout thu hồi token, cookie cross-site, ForwardedHeaders — P4), L9 lỗi nhỏ, L7 + migration `SeriesId`.
3. **Tuần 3 (P1 hiệu năng):** P1 output cache + nén, P2 giảm polling gateway, P3 bỏ chặn bootstrap.
4. **Sau đó (P2):** tách `OrderServiceImpl`, chuyển tiền sang `long`, sửa mô hình thông số, Unit of Work, test + CI, dọn namespace.
