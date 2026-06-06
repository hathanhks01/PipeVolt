# PipeVolt

Backend API cho hệ thống quản lý bán hàng điện nước: kho — sản phẩm, đơn hàng, giỏ hàng, thanh toán, bảo hành, chat hỗ trợ và chatbot AI. Dự án xây dựng theo kiến trúc 3 lớp trên **ASP.NET Core 8** và **SQL Server**.

## Mục lục

- [Tính năng](#tính-năng)
- [Kiến trúc](#kiến-trúc)
- [Công nghệ](#công-nghệ)
- [Cấu trúc thư mục](#cấu-trúc-thư-mục)
- [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
- [Cài đặt và chạy local](#cài-đặt-và-chạy-local)
- [Cấu hình](#cấu-hình)
- [Database & Migration](#database--migration)
- [Chạy bằng Docker](#chạy-bằng-docker)
- [API & tài liệu](#api--tài-liệu)
- [Xác thực & phân quyền](#xác-thực--phân-quyền)
- [SignalR Chat](#signalr-chat)
- [Thanh toán SePay](#thanh-toán-sepay)
- [Tích hợp frontend](#tích-hợp-frontend)
- [Scripts tiện ích](#scripts-tiện-ích)
- [Lưu ý bảo mật](#lưu-ý-bảo-mật)

## Tính năng

| Nhóm | Mô tả |
|------|--------|
| **Xác thực** | Đăng ký, đăng nhập JWT, đăng nhập Google OAuth |
| **Sản phẩm** | CRUD sản phẩm, danh mục, thương hiệu; upload ảnh tĩnh (`wwwroot/images`) |
| **Kho & mua hàng** | Kho, tồn kho, nhà cung cấp, đơn mua (`PurchaseOrder`) |
| **Bán hàng** | Giỏ hàng, checkout, đơn bán (`SalesOrder`), chi tiết đơn, hóa đơn |
| **Khách hàng & nhân sự** | Quản lý khách, nhân viên, tài khoản (phân quyền Admin / Employee / Customer) |
| **Bảo hành** | Theo dõi trạng thái bảo hành sản phẩm |
| **Báo cáo** | API báo cáo thống kê |
| **Chat** | SignalR realtime (`/chathub`) giữa khách — nhân viên — admin |
| **Chatbot AI** | Tích hợp Google Gemini (cấu hình qua `OpenAI:ApiKey` trong `appsettings`) |
| **Thanh toán** | Webhook SePay xác nhận chuyển khoản và cập nhật đơn hàng |

## Kiến trúc

```
┌─────────────────────────────────────────┐
│           PipeVolt_Api                  │  Controllers, Swagger, SignalR Hub,
│         (Presentation)                  │  JWT/CORS, Static files, DI setup
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│           PipeVolt_BLL                  │  Business logic, Services, AutoMapper
│         (Business Layer)                │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│           PipeVolt_DAL                  │  EF Core, Models, Repositories,
│      (Data Access Layer)                │  DTOs, Migrations, Unit of Work
└─────────────────┬───────────────────────┘
                  │
            SQL Server
```

**Luồng phụ thuộc:** `PipeVolt_Api` → `PipeVolt_BLL` → `PipeVolt_DAL`

## Công nghệ

- [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- ASP.NET Core Web API
- Entity Framework Core 9 + SQL Server
- JWT Bearer + Google Authentication
- SignalR
- Swagger (Swashbuckle)
- AutoMapper
- BCrypt (mật khẩu)
- Docker (ASP.NET 8 runtime image)

## Cấu trúc thư mục

```
PipeVolt/
├── PipeVolt_Api/          # Web API, Controllers, cấu hình, wwwroot, Dockerfile
│   └── PipeVolt_Api.sln   # Solution Visual Studio
├── PipeVolt_BLL/          # Services, IServices, ChatHub
├── PipeVolt_DAL/          # Models, Repositories, DTOs, Migrations
├── node.js                # Script gom mã nguồn ra all_code_for_review.txt
└── README.md              # Tài liệu này
```

## Yêu cầu hệ thống

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) trở lên
- SQL Server (LocalDB, Express hoặc instance đầy đủ)
- [EF Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) (đã khai báo trong `PipeVolt_Api/.config/dotnet-tools.json`)
- Tùy chọn: Docker Desktop (chạy container API)

Frontend (React/Vite) **không nằm trong repo này**; API CORS cho phép `http://localhost:5173` và `http://localhost:3000`.

## Cài đặt và chạy local

### 1. Clone repository

```bash
git clone <url-repo>
cd PipeVolt
```

### 2. Khôi phục solution

```bash
cd PipeVolt_Api
dotnet restore PipeVolt_Api.sln
```

### 3. Cấu hình connection string

Chỉnh `PipeVolt_Api/appsettings.json` (hoặc dùng [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)) — xem mục [Cấu hình](#cấu-hình).

### 4. Áp dụng migration

```bash
cd PipeVolt_Api
dotnet tool restore
dotnet ef database update --project ../PipeVolt_DAL --startup-project .
```

### 5. Chạy API

```bash
dotnet run --project PipeVolt_Api.csproj
```

Mặc định (profile `http` / `https` trong `launchSettings.json`):

| Mục | Giá trị |
|-----|---------|
| URL | `http://localhost:3030` |
| Swagger UI | `http://localhost:3030/swagger` |
| SignalR Hub | `http://localhost:3030/chathub` |

## Cấu hình

File chính: `PipeVolt_Api/appsettings.json`. Không commit secret thật lên Git — dùng User Secrets hoặc biến môi trường.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=<host>;Database=PipeVolt;User Id=<user>;Password=<password>;TrustServerCertificate=True;"
  },
  "JWT": {
    "Key": "<chuỗi-bí-mật-đủ-dài>",
    "Issuer": "PipeVoltAPI",
    "Audience": "PipeVoltClient",
    "ExpireMinutes": 60
  },
  "Google": {
    "ClientId": "<google-client-id>",
    "ClientSecret": "<google-client-secret>"
  },
  "OpenAI": {
    "ApiKey": "<google-gemini-api-key>"
  }
}
```

> **Lưu ý:** Mục `OpenAI:ApiKey` trong code được dùng làm API key cho **Google Gemini** (`generativelanguage.googleapis.com`), không phải OpenAI.

Cấu hình Docker: `PipeVolt_Api/appsettings.Docker.json` (connection tới host `db`).

### User Secrets (khuyến nghị khi dev)

```bash
cd PipeVolt_Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=PipeVolt;..."
dotnet user-secrets set "JWT:Key" "your-secret-key-at-least-32-chars"
```

## Database & Migration

- DbContext: `PipeVolt_DAL/Models/PipeVoltDbContext.cs`
- Migrations: `PipeVolt_DAL/Migrations/`

**Tạo migration mới** (sau khi đổi model):

```bash
cd PipeVolt_Api
dotnet ef migrations add <TenMigration> --project ../PipeVolt_DAL --startup-project .
```

**Cập nhật database:**

```bash
dotnet ef database update --project ../PipeVolt_DAL --startup-project .
```

Khi chạy container với `ASPNETCORE_ENVIRONMENT=Docker`, ứng dụng tự gọi `Database.Migrate()` lúc khởi động.

## Chạy bằng Docker

Build từ **thư mục gốc** repo (Dockerfile tham chiếu cả 3 project):

```bash
# Từ thư mục PipeVolt (root)
docker build -f PipeVolt_Api/Dockerfile -t pipevolt-api .
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Docker pipevolt-api
```

Container lắng nghe port **8080**. Cần SQL Server reachable (ví dụ service tên `db` như trong `appsettings.Docker.json`).

## API & tài liệu

Base route: `api/[controller]`

| Controller | Chức năng chính |
|------------|-----------------|
| `Auth` | Login, Register, Logout, GoogleLogin |
| `Products`, `ProductCategories`, `Brands` | Sản phẩm & phân loại |
| `Cart`, `CartItem`, `Checkout` | Giỏ hàng & thanh toán |
| `SalesOrders`, `OrderDetails` | Đơn bán |
| `PurchaseOrders`, `PurchaseOrderDetails` | Đơn mua |
| `Inventories`, `Warehouses`, `Suppliers` | Kho & NCC |
| `Customers`, `Employees`, `UserAccount` | Người dùng |
| `Warranties` | Bảo hành |
| `Reports` | Báo cáo |
| `Chat`, `Chatbot` | Chat & AI |
| `Sepay` | Webhook thanh toán SePay |

Mô tả chi tiết từng endpoint: mở **Swagger** sau khi chạy API.

## Xác thực & phân quyền

- **JWT:** Gửi header `Authorization: Bearer <token>` (token trả về từ `POST /api/Auth/Login` hoặc `Register` / `GoogleLogin`).
- **Session:** Login cũng lưu token vào session server (`JwtToken`) — phù hợp flow cookie/session nếu cần.
- **Vai trò** (`userType` claim): `0` Admin, `1` Employee, `2` Customer.

| Policy | Quyền |
|--------|--------|
| `RequireAdmin` | Chỉ Admin |
| `RequireEmployee` | Chỉ Employee |
| `RequireCustomer` | Chỉ Customer |
| `RequireAdminOrEmployee` | Admin hoặc Employee |

Ví dụ: `EmployeesController` yêu cầu `RequireAdmin`.

## SignalR Chat

- Hub: `/chathub`
- JWT cho SignalR: truyền `access_token` trên query string khi negotiate (đã cấu hình trong `JwtBearerEvents`).

Ví dụ kết nối (JavaScript):

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:3030/chathub", {
    accessTokenFactory: () => jwtToken
  })
  .build();
```

## Thanh toán SePay

Webhook: `POST /api/Sepay/payment/access`

- Chỉ xử lý giao dịch `transferType = "in"`.
- Mã đơn lấy từ `code` hoặc parse `ORD-...` trong `content`.
- Cập nhật trạng thái thanh toán / đơn hàng tương ứng trong database.

Cấu hình URL webhook trên cổng SePay trỏ tới endpoint public của API (ngrok / deploy) khi test local.

## Tích hợp frontend

CORS policy `AllowReactApp`:

- `http://localhost:5173` (Vite)
- `http://localhost:3000` (Create React App)

Bật `AllowCredentials()` — frontend cần gửi credentials nếu dùng cookie/session.

Ảnh sản phẩm/danh mục phục vụ qua static files, ví dụ: `/images/products/<file>`.

## Scripts tiện ích

`node.js` ở root — gom toàn bộ file `.cs`, `.js`, `.json`, `.css` (trừ `bin`, `obj`, `Migrations`, …) vào `all_code_for_review.txt` để review hoặc gửi AI.

```bash
node node.js
```

## Lưu ý bảo mật

- **Không** đẩy `appsettings.json` chứa mật khẩu DB, JWT key, Google/Gemini key lên repository công khai.
- Đổi toàn bộ secret mặc định trước khi deploy production.
- Bật HTTPS và hạn chế Swagger trên môi trường production nếu cần.
- Xem xét bật xác thực webhook SePay (IP whitelist / signature) nếu SePay hỗ trợ.

## License

Chưa khai báo license — bổ sung khi publish 
