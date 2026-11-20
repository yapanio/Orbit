# Orbit - Windows Service Manager & System Monitor

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 8.0" />
  <img src="https://img.shields.io/badge/WPF-Windows%20Desktop-0078D7?logo=windows&logoColor=white" alt="WPF Desktop" />
  <img src="https://img.shields.io/badge/Entity%20Framework%20Core-8.0-5C2D91?logo=nuget&logoColor=white" alt="EF Core 8" />
  <img src="https://img.shields.io/badge/Database-SQLite-003B57?logo=sqlite&logoColor=white" alt="SQLite" />
  <img src="https://img.shields.io/badge/Tests-xUnit%20(29%20Passed)-22c55e?logo=checkmarx&logoColor=white" alt="xUnit Tests" />
  <img src="https://img.shields.io/badge/License-MIT-blue.svg" alt="License" />
</p>

**Orbit** là ứng dụng Windows Desktop hiện đại, gọn nhẹ và hiệu năng cao giúp theo dõi, kiểm soát toàn diện các dịch vụ Windows Services và giám sát tài nguyên phần cứng (CPU & RAM) theo thời gian thực. Được xây dựng trên nền tảng **WPF (.NET 8.0)**, áp dụng mô hình kiến trúc **MVVM** chuẩn mực cùng hệ thống CSDL cục bộ **SQLite (Entity Framework Core)** không cần cài đặt.

---

## 🌟 Tính Năng Nổi Bật (Key Features)

### 1. ⚙️ Quản lý Windows Service Chuyên Sâu
- **Liệt kê thời gian thực**: Quét và hiển thị toàn bộ dịch vụ Windows Service trên máy với độ trễ tối thiểu thông qua `ServiceController`.
- **Thao tác nhanh**: Khởi động (**Start**), Dừng (**Stop**), Khởi động lại (**Restart**) dịch vụ an toàn, tích hợp xử lý bất đồng bộ (non-blocking UI).
- **Bộ lọc đa năng**: Lọc dịch vụ theo trạng thái (*Đang chạy - Running*, *Đã dừng - Stopped*, *Tất cả - All*).
- **Tìm kiếm tức thì (Instant Search)**: Tìm kiếm theo tên dịch vụ (`ServiceName`) hoặc tên hiển thị (`DisplayName`) ngay khi gõ phím.

### 2. ⭐ Ghim & Dịch vụ Yêu Thích (Favorites / Pinning)
- Ghim các dịch vụ quan trọng (SQL Server, Docker, MySQL, Redis, v.v.) vào mục **Yêu thích** bằng 1 click chuột (biểu tượng ★).
- **Tự động nhận diện dịch vụ Dev (⚡ Auto-Detect Dev Services)**: Tính năng thông minh tự động quét và ghim nhanh các dịch vụ lập trình phổ biến đã cài trên máy.
- Dữ liệu yêu thích được lưu trữ độc lập theo từng tài khoản người dùng vào CSDL SQLite.

### 3. 📊 Giám Sát Tài Nguyên Hệ Thống (System Hardware Monitor)
- **CPU Usage**: Theo dõi tải CPU theo thời gian thực bằng `PerformanceCounter`, cập nhật liên tục với biểu đồ thanh tiến trình.
- **RAM Usage**: Đo lường dung lượng RAM đang sử dụng và tổng dung lượng bộ nhớ hệ thống (Used / Total GB, tỷ lệ %).
- Cơ chế cập nhật chạy nền tối ưu, không gây hao tổn tài nguyên máy.

### 4. 🔐 Xác Thực Người Dùng & Quản Lý Phiên (Authentication)
- Màn hình đăng nhập hiện đại (`LoginWindow`) trước khi vào Dashboard chính.
- Phân quyền tài khoản cơ bản (`Admin` / `User`), hiển thị thông tin người dùng đang đăng nhập và hỗ trợ Đăng xuất (`Logout`).

### 5. 💾 Cơ Sở Dữ Liệu SQLite Tự Động (Zero-Configuration)
- Sử dụng **Entity Framework Core 8** với cơ sở dữ liệu **SQLite** (`orbit.db`).
- **Tự động khởi tạo (Auto-migration & Seeding)**: Ứng dụng tự động tạo file database và nạp sẵn dữ liệu mẫu khi chạy lần đầu tiên, không yêu cầu cài đặt SQL Server hay cấu hình kết nối phức tạp.

### 6. 🛡️ Tích Hợp UAC Administrator
- Khai báo quyền `requireAdministrator` trong `app.manifest`, đảm bảo ứng dụng có đủ thẩm quyền làm việc với Windows Service Control Manager (SCM).

---

## 🛠️ Công Nghệ & Kiến Trúc (Tech Stack & Architecture)

- **Ngôn ngữ**: C# 12
- **Framework**: .NET 8.0 (`net8.0-windows`)
- **Giao diện**: WPF (Windows Presentation Foundation), XAML hiện đại
- **Mô hình kiến trúc**: MVVM (Model - View - ViewModel)
- **CSDL & ORM**: SQLite (`Microsoft.EntityFrameworkCore.Sqlite` 8.0)
- **System APIs**:
  - `System.ServiceProcess.ServiceController` (Quản lý Service)
  - `System.Diagnostics.PerformanceCounter` (Đo CPU / RAM)
- **Kiểm thử tự động**: xUnit 2.9, Coverlet, Microsoft.NET.Test.Sdk (100% 29/29 tests pass)

---

## 📁 Cấu Trúc Dự Án (Project Structure)

```text
Orbit/
├── Orbit/                                # Dự án ứng dụng WPF chính
│   ├── app.manifest                      # Cấu hình quyền UAC Administrator
│   ├── App.xaml / App.xaml.cs            # Entry point & xử lý Exception toàn cục
│   ├── Core/
│   │   ├── DialogHelper.cs               # Tiện ích hiển thị thông báo lỗi / xác nhận
│   │   └── RelayCommand.cs               # Triển khai ICommand chuẩn MVVM
│   ├── Models/
│   │   ├── Entities/                     # Thực thể EF Core (User, FavoriteService)
│   │   └── ServiceItemModel.cs           # Model đóng gói dữ liệu của từng Windows Service
│   ├── Services/
│   │   ├── AuthService.cs                # Service xác thực đăng nhập
│   │   ├── FavoriteDbService.cs          # Quản lý CSDL dịch vụ yêu thích
│   │   ├── OrbitDbContext.cs             # DbContext SQLite & Auto-Seed
│   │   ├── SystemMonitor.cs              # Service đo lường CPU & RAM
│   │   └── WindowsServiceManager.cs      # Service tương tác trực tiếp với Windows SCM
│   ├── ViewModels/
│   │   └── MainWindowViewModel.cs        # ViewModel trung tâm điều khiển Dashboard
│   └── Views/
│       ├── LoginWindow.xaml              # Giao diện đăng nhập
│       └── MainWindow.xaml               # Giao diện Dashboard chính (Tabs, Stats, DataGrid)
│
├── Orbit.Tests/                          # Dự án kiểm thử tự động (Unit Tests)
│   ├── AuthServiceTests.cs               # Test xác thực đăng nhập
│   ├── DialogHelperTests.cs              # Test helper thông báo
│   ├── FavoriteDbServiceTests.cs         # Test lưu trữ yêu thích vào SQLite
│   ├── OrbitDbContextTests.cs            # Test kết nối & schema CSDL
│   ├── SystemMonitorTests.cs             # Test giám sát phần cứng
│   └── ViewModelTests.cs                 # Test luồng xử lý của ViewModel
│
├── .gitignore                            # Cấu hình bỏ qua file build & SQLite runtime
└── README.md                             # Tài liệu dự án
```

---

## 🚀 Cài Đặt & Chạy Ứng Dụng (Getting Started)

### 1. Yêu cầu hệ thống (Prerequisites)
- Hệ điều hành: **Windows 10 hoặc Windows 11** (64-bit).
- Đã cài đặt [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
- Visual Studio 2022 (với workload *.NET Desktop Development*) hoặc Visual Studio Code / JetBrains Rider.

### 2. Tải mã nguồn (Clone repository)
```bash
git clone https://github.com/<your-username>/Orbit.git
cd Orbit
```

### 3. Build & Chạy ứng dụng
Mở Command Prompt hoặc PowerShell với quyền **Administrator** (để có quyền can thiệp vào các dịch vụ Windows):
```bash
dotnet build
dotnet run --project Orbit/Orbit.csproj
```

### 4. Tài khoản đăng nhập mẫu (Default Credentials)

Khi ứng dụng khởi động, màn hình đăng nhập sẽ xuất hiện. Bạn có thể sử dụng các tài khoản được tạo sẵn sau:

| Tên đăng nhập (Username) | Mật khẩu (Password) | Vai trò (Role) | Mô tả |
| :--- | :--- | :--- | :--- |
| **`admin`** | `admin123` | **Admin** | Quản trị viên hệ thống |
| **`camer`** | `123456` | **User** | Nhà phát triển (có sẵn dịch vụ yêu thích) |

*(Dữ liệu được tự động tạo và lưu trữ cục bộ trong file `orbit.db` tại thư mục chạy app).*

---

## 🧪 Chạy Kiểm Thử (Running Tests)

Để kiểm tra toàn bộ 29 ca kiểm thử tự động của dự án:
```bash
dotnet test
```

**Kết quả mẫu:**
```text
Passed!  - Failed: 0, Passed: 29, Skipped: 0, Total: 29, Duration: 1 s - Orbit.Tests.dll
```

---

## 📄 Bản Quyền (License)

Dự án được phát hành theo giấy phép [MIT License](LICENSE). Tự do sử dụng cho mục đích học tập và phát triển cá nhân.
