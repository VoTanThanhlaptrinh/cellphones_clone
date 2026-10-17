# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository layout

Monorepo for a CellphoneS e-commerce clone (backend deployed on Render, storefront on Vercel):

- `cellPhoneS_backend/` – ASP.NET Core 9 API (PostgreSQL/EF Core, Redis, Azure Blob).
- `cellphones_clone_ui/` – Angular 20 app with two areas: the customer storefront (`src/app/store/`) and the admin area at `/admin` (`src/app/admin/`), sharing one auth, HTTP and design system (standalone components, Tailwind/Material). The former TailAdmin-based `admin_dashboard/` was merged into it and removed; see `cellphones_clone_ui/ADMIN_MERGE_PLAN.md`.
- `analyze_layers.js` at the repo root is untracked and unrelated to the app (a stray script). Ignore it.
- Use `cellPhoneS_backend/cellPhoneS_backend.sln`; the root `docnet_workspace.sln` is stale (it points at a nonexistent `cellphones_backend\` folder).

## Commands

Backend (run from `cellPhoneS_backend/`):

- `dotnet run` / `dotnet build` / `dotnet watch run` (local API is expected at `http://localhost:5169`).
- `dotnet ef migrations add <Name>` / `dotnet ef database update` (migrations live in `Migrations/`).
- `docker-compose.yml` only starts Elasticsearch + Kibana. Postgres, Redis and Azurite must be provided separately.
- There is no test project for the app and no CI workflow (`.github/workflows/` is empty).

Frontend (run from `cellphones_clone_ui/`):

- `npm start` (ng serve, port 4200), `npm run build`, `npm test` (Karma/Jasmine; single spec: `npx ng test --include='**/foo.spec.ts'`).
- `npm run check:boundaries` fails when an import crosses the folder rules below (run it before committing UI changes).
- Storefront SSR server: `npm run serve:ssr:cellphones-clone-ui`, but `angular.json` currently sets `"ssr": false`.

## Backend architecture (`cellPhoneS_backend/`)

- Folders follow Clean Architecture (`src/Api | Application | Domain | Infrastructure | Shared`), but **namespaces do not match folders**. Types live in legacy namespaces such as `cellphones_backend.Models/Data/Repositories/DTOs`, `cellphones_backend.Services.Implement` and `cellPhoneS_backend.Services/Auth/Controllers`. Copy the namespaces and usings of neighbouring files rather than deriving them from the path.
- **Code is organised by module, each with the same layered layout**: `src/<Module>/{Api/Controllers,Application/{DTOs,Services,Validation},Domain/Entities,Infrastructure/Persistence}` for modules `Account, Admin, Cart, Catalog, Media, Orders, Payments, Products, Shipping, Stores` (service interface and `Impl` side by side; controllers split into `Admin/` and `User/` where a module has both). Cross-cutting code stays in the original layer folders: `Api` (`BaseController`, `HealthController`, filters), `Application` (`Common/Exceptions`, DI, `DTOs/Common`), `Infrastructure` (`ApplicationDbContext`, auth middleware, rate limiting, `IRepository`/`BaseRepository`, repository DI) and `Shared` (utilities and `Constants`). Put a new file in its module. String constants (order status/type, gateway payment status, providers, gateway error codes, config keys) live in `src/Shared/Constants` (namespace `cellPhoneS_backend.Constants`); state-transition rules live on the entities (`Order.MarkPaid/Cancel/CanBeCancelled/GetTotalAmount`, `Order.StartPaymentAttempt/NextPaymentAttempt`), not in services.
- DI registration is manual, in two places: `AddCustomServices()` (`src/Application/DependencyInjection/ServiceCollectionExtensions.cs`) and `AddRepositories()` (`src/Infrastructure/DependencyInjection/RepositoryServiceCollectionExtensions.cs`). A new service or repository must be added to the right one. Service interface naming is mixed (`AuthService`, `ProductService` vs `IReviewService`, `IEmailService`).
- Request flow: Controller (`src/<Module>/Api/Controllers`, extends `BaseController`) → service → repository → `ApplicationDbContext` (Identity-based). Services return plain data (empty list when nothing matches) and signal business failures by throwing `BadRequestException`/`NotFoundException`/`ConflictException`/`UnauthorizedException`/`ForbiddenException`/`InternalServerException` (`src/Application/Common/Exceptions.cs`). Controllers own the success status and message (`Ok(new ApiResponse<T>(message, data))`); `ApiExceptionFilter` (`src/Api/Filters`, registered in `AddControllers`) turns the exceptions into the matching HTTP status with an `ApiResponse` body. DTOs are in `src/<Module>/Application/DTOs` (requests and responses side by side), validators (FluentValidation) in `src/<Module>/Application/Validation`.
- **Authorization is centralized, not attribute-based.** `CentralizedAuthMiddleware` evaluates the ordered list in `src/Infrastructure/Auth/RouteConfig.cs` (first match wins; roles are `ADMIN` and `USER`; `/api/*` is the authenticated catch-all). A new public endpoint must be added there or it returns 401, and a new role-restricted route must be added before the catch-all.
- Auth tokens: JWT access token is sent in the `Authorization` header; the refresh token is an httpOnly cookie whose value is stored in Redis. `JwtBlacklistService` stores revoked JTIs in Redis. Google OAuth is configured in `Program.cs`, and its failure handler redirects to a hard-coded `http://localhost:4200/login`.
- Rate limiting: `Program.cs` uses the built-in `AddRateLimiter` policies (`GlobalLimit`, `AuthLimit`). `src/Infrastructure/RateLimit/RateLimitingMiddleware` and `RateLimitConfig` exist but are **not registered** in the pipeline.
- Startup (`Program.cs`) warms `ProductSearchService` (in-memory fuzzy search with FuzzySharp over `IMemoryCache`) and `IVietnamLocationService`; failures are logged and swallowed. Hangfire runs on the Postgres connection (dashboard at `/hangfire`).
- Localization: resx files in `Resources/`, default culture `vi`, overridable with `?culture=en`.
- External integrations: Azure Blob (Azurite locally), GHTK shipping fees, MailKit email, Vietnam administrative-division API, and the self-built payment gateway (`D:\IT\payment-module`, app `CELLPHONES`, providers PAYOS and VNPAY; its guide is `doc/API_INTEGRATION_GUIDE.md` there).
- Online payments go only through the gateway (`IPaymentGatewayClient`, typed `HttpClient` registered in `Program.cs`; there is no direct PayOS SDK or webhook any more). The gateway is the single source of truth for payment status: CellphoneS stores no payment row, only `Order.PaymentAttempt`, which `OrderServiceImpl.StartOnlinePayment` saves **before** calling the gateway so retries reuse the same `Idempotency-Key` `cellphones-order-{orderId}-attempt-{n}` (the attempt is bumped only after a terminal FAILED/EXPIRED/CANCELLED payment or a 4xx rejection). The amount is computed on the server. Besides polling, the gateway can push: `POST /api/payments/callback` (anonymous in `RouteConfig`, authenticated by `X-Payment-Signature` = `sha256=` + HMAC-SHA256(`PaymentGateway:CallbackSecret`, `"<X-Payment-Timestamp>.<raw body>"`), timestamp within 5 min; see `PaymentCallbackSignature`). It is only a hint: `OrderService.HandlePaymentCallback` re-asks the gateway (`GET /status`) and then runs the same idempotent `MarkOrderPaid` path, returning 5xx if the gateway is unreachable so the gateway retries. The gateway's `callbackUrl` for `CELLPHONES` must be `https://<backend>/api/payments/callback` with the same secret. Polling paths: `GET /api/payments/order/{orderId}` (the storefront URL is `/payment/:orderId`, so a reload keeps the transaction), `GET /api/orders/{id}/status` and the Hangfire job `sync-online-payments` (every 5 min) ask the gateway and call `MarkOrderPaid` on `PAID` (idempotent), as a fallback if a callback is lost. The gateway's `returnUrl` for `CELLPHONES` must be `https://<frontend>/payment/success` and its `cancelUrl` `https://<frontend>/payment/failed`; both pages ignore the URL, read the order id from `sessionStorage` (`pendingPaymentOrderId`), re-ask the backend and redirect to the other page / `/payment/:orderId` when the status does not match (VNPay uses one return URL for both outcomes). The old `/payment/return` still works as a redirect-only page. The `/payment/:orderId` page renders the gateway's `paymentInfo` (PayOS QR string, account, transfer content) itself instead of embedding the provider page, counts down to `expiresAt`, and 5 seconds after expiry goes to `/home`; VNPay has no `paymentInfo` and is a full-page redirect. `PaymentGateway:TestAmountOverride` (user-secrets) forces every online payment to that amount for testing; remove it for real prices.
- CORS only allows `http://localhost:4200` and `https://cellphonesclonethanh.vercel.app`.
- Product Reviews feature (committed): code is wrapped in `// === BEGIN/END NEW FEATURE: PRODUCT REVIEWS ===` markers and shipped with the `AddReview` migration. `GET /api/reviews/product/*` is allowed anonymously in `RouteConfig.cs`; `POST /api/reviews` requires login.

## Configuration

`appsettings*.json` is gitignored. Required keys: `ConnectionStrings:db` (Postgres), `ConnectionStrings:Redis` (code forces `Ssl = true` except for localhost, so a local non-TLS Redis works in Development), `ConnectionStrings:StorageAccount`, `Jwt:Key|Issuer|Audience`, `Authentication:Google:ClientId|ClientSecret`, `PaymentGateway:BaseUrl|ApiKey` (the gateway's base URL and the `CELLPHONES` API key; backend only, never in the frontend). `UserSecretsId` is set on the project, so `dotnet user-secrets` also works.

## Frontend notes (`cellphones_clone_ui/`)

- **Any storefront UI change must follow `cellphones_clone_ui/DESIGN.md`** (brand red `#d70018` tokens, typography, component recipes, a11y and copy rules). The ordered cleanup backlog is `cellphones_clone_ui/UI_IMPROVEMENT_PLAN.md`; do P0 (tokens in `src/styles.css`, `vnd` pipe) before UI work that depends on it.

- **Folder rules** (`src/app/`, path aliases `@core`, `@shared`, `@store`, `@admin`; enforced by `scripts/check-boundaries.mjs`):
  - `core/` – auth (`auth.service`, `admin.guard`, `safeReturnUrl`), `http/` interceptors, `models/` (`ApiResponse`, `PagedResult`, …), `services/` (notify, loading, title strategy). No UI. Imports only `core`.
  - `shared/` – presentational building blocks used by both areas: `form/` (input, autocomplete-input, select), `ui/` (icon, badge, alert, table, pagination, confirm-dialog), `pipes/`. Imports only `shared` and `core/models`; never injects a service.
  - `store/` – the storefront: `layout/` (store-shell, header, footer…), `pages/`, `components/`, `services/`, `store.routes.ts`. Never imports `admin`.
  - `admin/` – the admin area, lazy-loaded at `/admin` behind `adminGuard` (ADMIN role from the JWT; the backend still enforces `/api/admin/*`). Never imports `store`. `layout/` is the shell, `catalog/` is a config-driven CRUD page (add a resource in `catalog/catalog-resources.ts` and it gets a route, menu entry, table, form and delete dialog).
- Header/footer come from the `StoreShellComponent` layout route, not from a URL list: pages that need them go under it in `store.routes.ts`; minimal pages (login, cart, order, checkout) stay outside.
- `src/environments/environment.ts` points `apiUrl` at the deployed Render backend. The development build replaces it with `environment.development.ts` (`http://localhost:5169/api`), so use the development configuration to hit a local API.
- `proxy.conf.json` proxies `/api/provinces` to provinces.open-api.vn. `vercel.json` rewrites every path to `index.html`.
