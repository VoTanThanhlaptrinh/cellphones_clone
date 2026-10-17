# CellphoneS Clone - E-Commerce Platform

A full-stack e-commerce platform that clones the CellphoneS website, built for educational purposes.

## Project Overview

This is a full-stack e-commerce platform that clones the CellphoneS website, built for educational purposes. The application allows users to browse products (phones, tablets, laptops, accessories), search for items, manage shopping carts, and place orders. It includes user authentication with JWT tokens, role-based access control (Admin/User), and integration with Redis for caching product search results. The backend is deployed on Render, while the frontend is hosted on Vercel.

## Deployment

**Backend**: https://cellphones-clone.onrender.com (Render)

**Frontend**: https://cellphonesclonethanh.vercel.app/home (Vercel)

### Key Architecture Notes:

**🔧 Backend (`cellPhoneS_backend/`)**
- **Pattern**: Clean Architecture with Repository + Service layers
- **Auth**: JWT tokens stored in Redis, OAuth2 Google/Zalo
- **Search**: In-memory fuzzy search with Levenshtein distance
- **Storage**: Azure Blob Storage (Azurite for local)
- **Deployment**: Dockerized on Render

**🛒 Client Storefront (`cellphones_clone_ui/`)**
- **Framework**: Angular 20 with SSR (Server-Side Rendering)
- **State**: RxJS Signals + Observables
- **UI**: Tailwind CSS + Angular Material + Swiper.js
- **Deployment**: Vercel

**📊 Admin area (`/admin` in `cellphones_clone_ui/`)**
- **Framework**: the same Angular 20 app as the storefront (lazy-loaded, ADMIN role only)
- **UI**: shared Tailwind design system (no separate template)
- **Features**: catalog management (categories, brands, series, demands, storehouses) with tables, pagination and add/edit/delete dialogs
- **Purpose**: Catalog administration; products/orders are planned

> **Note**: The backend is hosted on a free tier service, which spins down after 15 minutes of inactivity. If you experience a delay upon the first visit, please wait about 30–60 seconds for the server to wake up.

### Key Features

- **Product Management**: Browse categories, view detailed product specifications, images, and stock availability
- **Search Functionality**: Fast product search using cached results stored in Redis
- **Shopping Cart**: Add products with color/variant selection and manage quantities
- **Order Processing**: Complete checkout with shipping fee calculation and payment method selection
- **User Authentication**: JWT-based authentication with support for OAuth2 (Google, Zalo)
- **Role-Based Authorization**: Separate Admin and User access levels using centralized middleware
- **Multi-role Support**: Student and Teacher registration with document verification
- **Localization**: Vietnamese language support using resource files

## Tech Stack

### 1. Core Backend
- **Framework**: ASP.NET Core 9.0 (C#)
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: ASP.NET Core Identity + JWT Bearer Tokens
- **Caching**: Redis (StackExchange.Redis) for product search optimization
- **Cloud Storage**: Azure Blob Storage (Azurite for local development)
- **API Documentation**: RESTful API with standardized response format
- **Authorization**: Custom middleware with route-based policies
- **Localization**: Multi-language support via resource files
- **Deployment**: Docker containerization on Render

### 2. Client Storefront
- **Framework**: Angular 20.0.5 with Standalone Components
- **Language**: TypeScript 5.x
- **UI Libraries**: 
  - Angular Material
  - Tailwind CSS with PostCSS
- **State Management**: RxJS Signals and Observables
- **HTTP Client**: Angular HttpClient for API communication
- **Carousel/Slider**: Swiper.js
- **Loading Indicators**: ngx-spinner
- **Routing**: Angular Router with lazy loading
- **Server-Side Rendering**: Angular Universal SSR support
- **Build Tool**: Angular CLI with esbuild
- **Deployment**: Vercel

### 3. Admin Dashboard
- **Framework**: Angular 19.0.6 with Standalone Components
- **Language**: TypeScript 5.6
- **UI Framework**: Tailwind CSS with custom PostCSS configuration
- **Build Tool**: Angular CLI
- **Purpose**: Administrative interface for product management, order processing, and user management
- **Deployment**: Separate deployment pipeline from client storefront

## Folder Structure

```
docnet_workspace/
│
├── .github/
│   └── workflows/                       # CI/CD GitHub Actions workflows
│
├── .vscode/
│   └── settings.json                    # Workspace settings
│
├── cellPhoneS_backend/                  # 🔧 ASP.NET Core 9.0 Backend API (Clean Architecture)
│   ├── src/
│   │   ├── Api/                        # Lớp API (Controllers, Middlewares, Program.cs config)
│   │   │   ├── Controllers/            # Endpoints (Product, Auth, Order, etc.)
│   │   │   └── Middlewares/            # CentralizedAuthMiddleware, RateLimiting
│   │   ├── Application/                # Lớp Use Cases (Business Logic)
│   │   │   ├── Services/               # Implementations & Contracts (ProductService, etc.)
│   │   │   └── DTOs/                   # Requests & Responses models
│   │   ├── Domain/                     # Lớp Core (Entities)
│   │   │   └── Models/                 # Product, User, Order, Cart, Review
│   │   ├── Infrastructure/             # Lớp Cơ Sở Hạ Tầng (Database, External Services)
│   │   │   ├── Data/                   # ApplicationDbContext
│   │   │   └── Repositories/           # Implementations & Contracts
│   │   └── Shared/                     # Lớp dùng chung (Constants, Helpers)
│   ├── Resources/
│   │   └── ShareResource.resx          # Localization (vi/en)
│   ├── Migrations/                     # EF Core database migrations
│   ├── Program.cs                      # App startup, middleware, CORS, Redis
│   ├── appsettings.json                # Configuration (ConnectionStrings, JWT, Redis)
│   ├── Dockerfile                      # Multi-stage Docker build
│   └── cellPhoneS_backend.csproj       # .NET project file
│
├── cellphones_clone_ui/                # 🛒 Angular 20 Client Storefront (SSR)
│   ├── src/
│   │   ├── app/
│   │   │   ├── home/                   # Homepage with category listings
│   │   │   ├── product-detail/         # Product detail pages
│   │   │   ├── category/               # Category browsing
│   │   │   ├── cart/                   # Shopping cart UI
│   │   │   ├── checkout/               # Checkout process
│   │   │   ├── payment-infor/          # Payment method selection
│   │   │   ├── member-dashboard/       # User account dashboard
│   │   │   ├── register/               # User/Student/Teacher registration
│   │   │   ├── login/                  # Login form
│   │   │   ├── header/                 # Navigation header
│   │   │   ├── header-member-dashboard/ # Member dashboard header
│   │   │   ├── back-to-top/            # Scroll to top button
│   │   │   ├── services/               # API services (CartService, ProductService, etc.)
│   │   │   ├── app.routes.ts           # Client-side routing
│   │   │   └── app.routes.server.ts    # SSR routing configuration
│   │   ├── server.ts                   # Angular Universal SSR entry point
│   │   └── main.server.ts              # Server bootstrap
│   ├── projects/
│   │   └── shared-utils/               # Shared utility library
│   ├── public/                         # Static assets
│   ├── angular.json                    # Angular CLI configuration
│   ├── package.json                    # Dependencies (Angular 20, Swiper, Tailwind, ngx-spinner)
│   ├── tsconfig.json                   # TypeScript configuration
│   └── README.md
│
├── docnet_workspace.sln                # Visual Studio solution file
├── README.md                           # Project overview & tech stack
└── .gitignore                          # Git ignore rules
```


