# CellphoneS Clone - E-Commerce Platform

A full-stack e-commerce platform that clones the CellphoneS website, built for educational purposes.

## Project Overview

This is a full-stack e-commerce platform that clones the CellphoneS website, built for educational purposes. The application allows users to browse products (phones, tablets, laptops, accessories), search for items, manage shopping carts, and place orders. It includes user authentication with JWT tokens, role-based access control (Admin/User), and integration with Redis for caching product search results. The backend is deployed on Render, while the frontend is hosted on Vercel.

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

**📊 Admin Dashboard (`admin_dashboard/`)**
- **Framework**: Angular 19 Standalone Components
- **UI**: TailAdmin template with Tailwind CSS v4
- **Features**: Dashboard, tables, forms, charts, invoice management
- **Purpose**: Product/Order/User administration

## Deployment

**Backend**: https://cellphones-clone.onrender.com (Render)

**Frontend**: https://cellphonesclonethanh.vercel.app/home (Vercel)

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
├── cellPhoneS_backend/                  # 🔧 ASP.NET Core 9.0 Backend API
│   ├── Auth/
│   │   ├── RouteConfig.cs              # Centralized authorization policies
│   │   └── CentralizedAuthMiddleware.cs # JWT & role-based access control
│   ├── Controllers/
│   │   ├── AuthController.cs           # Login, Register, OAuth2, JWT refresh
│   │   ├── User/
│   │   │   ├── ProductController.cs    # Product catalog endpoints
│   │   │   ├── ProductSearchController.cs # Fuzzy search with caching
│   │   │   ├── CartController.cs       # Shopping cart management
│   │   │   ├── OrderController.cs      # Order creation & checkout
│   │   │   ├── CategoryController.cs   # Category browsing
│   │   │   └── HomeController.cs       # Homepage data
│   │   └── Admin/
│   │       └── (Admin-only controllers) # Product/order/user management
│   ├── Services/
│   │   ├── Implement/
│   │   │   ├── ProductSearchServiceImpl.cs # In-memory fuzzy search
│   │   │   ├── JwtTokenServiceImpl.cs  # Token generation & refresh
│   │   │   ├── JwtBlacklistServiceImpl.cs # Revoked token tracking
│   │   │   ├── OrderServiceImpl.cs     # Order processing logic
│   │   │   ├── ShippingFeeServiceImpl.cs # GHTK shipping API integration
│   │   │   └── AzuriteServiceImpl.cs   # Azure Blob Storage
│   │   └── Interface/                  # Service contracts
│   ├── Repository/
│   │   ├── Implement/
│   │   │   ├── CartRepository.cs       # Cart data access
│   │   │   ├── CartDetailRepository.cs # Cart item operations
│   │   │   └── (Other repositories)
│   │   └── Interface/                  # Repository contracts
│   ├── Models/                         # Entity models (Product, Order, Cart, User, etc.)
│   ├── Data/
│   │   └── ApplicationDbContext.cs     # EF Core DbContext with materialized views
│   ├── DTOs/
│   │   ├── Requests/                   # API request models
│   │   └── Responses/
│   │       ├── ApiResponse.cs          # Standardized API response wrapper
│   │       ├── HomeViewModel.cs        # Homepage data structure
│   │       └── ShippingFeeResponse.cs  # Shipping calculation response
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
├── admin_dashboard/                    # 📊 Angular 19 Admin Dashboard
│   ├── src/
│   │   ├── app/
│   │   │   ├── pages/
│   │   │   │   ├── dashboard/
│   │   │   │   │   └── ecommerce/      # E-commerce dashboard
│   │   │   │   ├── profile/            # User profile page
│   │   │   │   ├── forms/
│   │   │   │   │   └── form-elements/  # Form input components
│   │   │   │   ├── tables/
│   │   │   │   │   └── basic-tables/   # Data table examples
│   │   │   │   ├── charts/             # Line & bar charts
│   │   │   │   ├── invoices/           # Invoice management
│   │   │   │   ├── ui-elements/        # UI component pages (Alerts, Avatars, Buttons, etc.)
│   │   │   │   ├── auth-pages/         # Sign in/up pages
│   │   │   │   ├── calender/           # Calendar page
│   │   │   │   ├── blank/              # Blank page template
│   │   │   │   └── other-page/
│   │   │   │       └── not-found/      # 404 error page
│   │   │   ├── shared/
│   │   │   │   ├── layout/
│   │   │   │   │   ├── app-layout/     # Main layout wrapper
│   │   │   │   │   ├── app-header/     # Top navigation bar
│   │   │   │   │   ├── app-sidebar/    # Collapsible sidebar menu
│   │   │   │   │   ├── backdrop/       # Mobile overlay
│   │   │   │   │   ├── auth-page-layout/ # Auth pages layout
│   │   │   │   │   └── generator-layout/ # AI generator layout
│   │   │   │   ├── components/
│   │   │   │   │   ├── common/         # Breadcrumbs, theme toggle, component cards
│   │   │   │   │   ├── header/         # Header dropdowns (user, notifications)
│   │   │   │   │   ├── cards/          # Card components (with/without images, icons)
│   │   │   │   │   ├── tables/
│   │   │   │   │   │   └── basic-tables/ # Table components (1-5)
│   │   │   │   │   ├── form/
│   │   │   │   │   │   └── form-elements/ # Input, select, checkbox, radio, toggle
│   │   │   │   │   ├── ui/             # Reusable UI elements (Avatar, Badge, Button, Dropdown)
│   │   │   │   │   ├── ui-example/     # UI pattern examples (FAQs, etc.)
│   │   │   │   │   ├── invoice/        # Invoice sidebar, main, list
│   │   │   │   │   ├── ecommerce/      # E-commerce components (billing, transactions)
│   │   │   │   │   ├── user-profile/   # Profile cards (meta, info, address)
│   │   │   │   │   ├── transactions/   # Order history
│   │   │   │   │   └── ai/             # AI sidebar history
│   │   │   │   ├── services/
│   │   │   │   │   └── sidebar.service.ts # Sidebar state management
│   │   │   │   └── pipes/              # Custom pipes (SafeHtmlPipe)
│   │   │   ├── app.routes.ts           # Admin routing configuration
│   │   │   └── app.config.ts           # Application configuration
│   │   ├── index.html                  # HTML entry point
│   │   └── main.ts                     # Bootstrap entry point
│   ├── public/
│   │   └── images/                     # Static images (logo, user avatars, error pages)
│   ├── .postcssrc.json                 # PostCSS configuration
│   ├── angular.json                    # Angular CLI configuration
│   ├── package.json                    # Dependencies (Angular 19, Tailwind CSS)
│   ├── tsconfig.json                   # TypeScript configuration
│   └── README.md                       # TailAdmin documentation
│
├── docnet_workspace.sln                # Visual Studio solution file
├── README.md                           # Project overview & tech stack
└── .gitignore                          # Git ignore rules
```


