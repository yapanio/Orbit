# Kế Hoạch Triển Khai Chi Tiết: Local Development Environment Manager (Phase 1)

Tài liệu này cụ thể hóa lộ trình xây dựng ứng dụng quản lý Windows Services dành cho lập trình viên, được tối ưu hóa dựa trên nền tảng **C#, .NET 8, WPF và MVVM thuần** (sử dụng `ViewModelBase`, `RelayCommand` chuẩn và xử lý bất đồng bộ qua `Task.Run`).

## 1. Kiến Trúc & Cấu Trúc Thư Mục (Solution Structure)

Dự án được tổ chức trong 1 Project WPF duy nhất, phân tách rõ ràng các tầng theo mô hình MVVM:

```text
LocalDevManager/
├── App.xaml / App.xaml.cs              # Khởi chạy ứng dụng
├── app.manifest                        # Cấu hình quyền Administrator (UAC)
├── Core/                               # Nền tảng MVVM dùng chung
│   ├── ViewModelBase.cs                # Cài đặt INotifyPropertyChanged & SetProperty<T>
│   └── RelayCommand.cs                 # Cài đặt ICommand với CommandManager.RequerySuggested
├── Models/                             # Các class POCO lưu trữ dữ liệu
│   ├── ServiceItemModel.cs             # Thông tin hiển thị của 1 Windows Service
│   └── TrackedConfigModel.cs           # Cấu trúc lưu trữ danh sách Service được ghim
├── Services/                           # Tầng giao tiếp Hệ điều hành & Lưu trữ (Infrastructure)
│   ├── WindowsServiceManager.cs        # Gọi ServiceController (Load, Start, Stop, Restart)
│   ├── LocalStorageService.cs          # Lưu/Đọc danh sách Tracked Services (JSON / SQLite)
│   └── SystemMonitorService.cs         # Đọc thông số % CPU và % RAM qua PerformanceCounter
├── Converters/                         # Chuyển đổi dữ liệu hiển thị (IValueConverter)
│   └── StatusToColorConverter.cs       # Đổi trạng thái (Running/Stopped/Pending) sang màu sắc
├── ViewModels/                         # Logic điều khiển giao diện
│   └── MainWindowViewModel.cs          # Chứa ObservableCollection, SelectedItem, Commands, Search, Metrics
└── Views/                              # Giao diện XAML
    └── MainWindow.xaml                 # Bố cục Dashboard, DataGrid và Panel điều khiển
```

## 2. Các Quy Tắc Kỹ Thuật Đã Chốt (Technical Decisions)

1. **MVVM Thuần & Phân tách trách nhiệm (Separation of Concerns):**
   * Sử dụng `ViewModelBase` với phương thức `SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)` để tự động kích hoạt `PropertyChanged`.
   * Sử dụng `RelayCommand` truyền thống kết hợp `CommandManager.RequerySuggested` để tự động bật/tắt nút bấm (`CanExecute`).
   * **ViewModel không gọi trực tiếp API hệ điều hành:** Mọi thao tác với `ServiceController` hay `PerformanceCounter` đều phải được đóng gói trong các class nằm ở thư mục `Services/`.

2. **Cơ chế Bất đồng bộ & Chống đơ UI (Asynchronous UI):**
   * Giữ nguyên cấu trúc `RelayCommand` nhận `Action<object>`.
   * Sử dụng phương thức `async void` trong ViewModel cho các lệnh Start/Stop/Restart.
   * Bọc các tác vụ nặng của `ServiceController` (`Start()`, `Stop()`, `WaitForStatus()`) bên trong `await Task.Run(...)`.
   * **Quy tắc bắt buộc:** Mọi hàm `async void` phải nằm trong khối `try { ... } catch (Exception ex) { ... }` để bắt lỗi phân quyền hoặc timeout, tuyệt đối không để văng ngoại lệ làm sập ứng dụng.

3. **Chặn lỗi Double-Click khi Service đang chuyển trạng thái:**
   * Ngay trước khi gọi `await Task.Run(...)`, cập nhật trạng thái của `SelectedService.Status` thành `"Starting..."` hoặc `"Stopping..."`.
   * Điều kiện `canExecute` của các nút bấm kiểm tra `Status == "Stopped"` hoặc `Status == "Running"` sẽ lập tức trả về `false`, tự động làm mờ nút bấm trong suốt thời gian chờ.

4. **Quản lý Tài nguyên Hệ điều hành (`IDisposable`):**
   * Không giữ đối tượng `ServiceController` trực tiếp trên giao diện.
   * Khi đọc danh sách hoặc điều khiển Service, luôn sử dụng khối `using` (ví dụ: `using var sc = new ServiceController(serviceName);`) để giải phóng bộ nhớ hệ điều hành ngay sau khi hoàn tất.

## 3. Lộ Trình Thực Hiện Chi Tiết (4 Cột Mốc / Milestones)

### Cột mốc 1: Dựng Khung MVVM & Đọc Danh Sách Windows Services

**Mục tiêu:** Mở ứng dụng lên thấy được toàn bộ danh sách Windows Services trong máy hiển thị trên bảng.

* [ ] **Bước 1.1:** Khởi tạo project WPF (.NET 8) và cài đặt các gói NuGet:
  * `System.ServiceProcess.ServiceController`
  * `System.Diagnostics.PerformanceCounter`
* [ ] **Bước 1.2:** Tạo `Core/ViewModelBase.cs` và `Core/RelayCommand.cs`.
* [ ] **Bước 1.3:** Tạo class `ServiceItemModel` (kế thừa `ViewModelBase` để khi thuộc tính `Status` hoặc `IsTracked` đổi thì dòng trên bảng tự cập nhật):
  * `string ServiceName { get; set; }` (Tên định danh hệ thống, VD: `mssqlserver`)
  * `string DisplayName { get; set; }` (Tên hiển thị đầy đủ)
  * `string Status { get; set; }` (`Running`, `Stopped`, `Starting...`, `Stopping...`)
  * `bool CanStop { get; set; }`
  * `bool IsTracked { get; set; }` (Đánh dấu service hay dùng)
* [ ] **Bước 1.4:** Viết hàm `GetAllServices()` trong `WindowsServiceManager.cs`:
  * Gọi `ServiceController.GetServices()`.
  * Duyệt qua mảng, map dữ liệu sang `List<ServiceItemModel>` và gọi `.Dispose()` từng phần tử `ServiceController`.
* [ ] **Bước 1.5:** Thiết kế `MainWindow.xaml` với `<DataGrid AutoGenerateColumns="False" ItemsSource="{Binding DisplayedServices}" SelectedItem="{Binding SelectedService}">` gồm các cột: Tracked, Service Name, Display Name, Status.

### Cột mốc 2: Điều Khiển Service (Start / Stop / Restart) & Quyền Admin

**Mục tiêu:** Chọn 1 dòng trên bảng, bấm nút để bật/tắt/khởi động lại service mượt mà và xử lý lỗi rõ ràng.

* [ ] **Bước 2.1:** Cấu hình quyền Administrator:
  * Thêm file `app.manifest` vào project.
  * Đổi thẻ `<requestedExecutionLevel level="requireAdministrator" uiAccess="false" />` để ứng dụng có quyền điều khiển Windows SCM.
* [ ] **Bước 2.2:** Thiết kế khu vực Detail Panel ở cột bên phải màn hình (`Grid.Column="1"`):
  * Hiển thị thông tin chi tiết của `SelectedService` qua `{Binding SelectedService.DisplayName}`, `{Binding SelectedService.Status}`.
  * Đặt 4 nút: **Start**, **Stop**, **Restart**, **Refresh**.
* [ ] **Bước 2.3:** Khai báo các `RelayCommand` trong `MainWindowViewModel`:
  * `StartCommand`: `canExecute => SelectedService != null && SelectedService.Status == "Stopped"`
  * `StopCommand`: `canExecute => SelectedService != null && SelectedService.Status == "Running" && SelectedService.CanStop`
  * `RestartCommand`: `canExecute => SelectedService != null && SelectedService.Status == "Running" && SelectedService.CanStop`
* [ ] **Bước 2.4:** Viết các hàm `async void StartSelectedService()`, `StopSelectedService()`, `RestartSelectedService()` sử dụng `await Task.Run(...)` với thời gian chờ tối đa (Timeout) là `TimeSpan.FromSeconds(15)`.
* [ ] **Bước 2.5:** Thêm thuộc tính `string StatusMessage` dưới đáy màn hình (StatusBar) để hiển thị tiến trình hoặc thông báo lỗi thân thiện khi rơi vào khối `catch`.

### Cột mốc 3: Tìm Kiếm, Bộ Lọc & Lưu Trữ Danh Sách Theo Dõi (Local Persistence)

**Mục tiêu:** Lọc nhanh service cần tìm và ghim lại các service phục vụ công việc lập trình (SQL, Postgres, Apache, Docker...) để dùng lâu dài.

* [ ] **Bước 3.1:** Chức năng Tìm kiếm & Lọc:
  * Giữ một danh sách gốc `private List<ServiceItemModel> _allServices`.
  * Tạo thuộc tính `string SearchText` (với `UpdateSourceTrigger=PropertyChanged` trên XAML) và `bool ShowTrackedOnly`.
  * Mỗi khi `SearchText` hoặc `ShowTrackedOnly` thay đổi, chạy hàm `ApplyFilter()` dùng LINQ `.Where(...)` để cập nhật lại `ObservableCollection<ServiceItemModel> DisplayedServices`.
* [ ] **Bước 3.2:** Chức năng Ghim (Track / Untrack Service):
  * Thêm nút `ToggleTrackCommand` cho phép đánh dấu hoặc bỏ đánh dấu `SelectedService.IsTracked`.
* [ ] **Bước 3.3:** Lưu trữ xuống đĩa cứng (`LocalStorageService`):
  * **Phương án 1 (Nhanh gọn):** Dùng `System.Text.Json` lưu danh sách các `ServiceName` được ghim vào file `tracked_services.json` trong thư mục chạy app.
  * **Phương án 2 (Chuẩn đồ án môn học):** Dùng `Microsoft.EntityFrameworkCore.Sqlite` tạo bảng `TrackedServices` lưu `ServiceName` và `AddedDate`.
  * Khi mở app, đối chiếu danh sách từ Windows SCM với danh sách đã lưu để bật cờ `IsTracked = true` cho các service tương ứng.

### Cột mốc 4: Dashboard Thống Kê, Theo Dõi CPU/RAM & Hoàn Thiện Giao Diện

**Mục tiêu:** Hoàn thiện giao diện trực quan, tích hợp bộ đo tài nguyên máy tính thời gian thực để đưa vào CV và báo cáo.

* [ ] **Bước 4.1:** Tạo Dashboard thống kê Service phía trên bảng (bằng `Border` và `StackPanel`):
  * **Total Services:** Tổng số service đang hiển thị.
  * **Running:** Số lượng service đang chạy.
  * **Tracked Running:** Số lượng service trong danh sách theo dõi đang chạy.
* [x] **Bước 4.2:** Tích hợp theo dõi thông số máy (`SystemMonitorService.cs`) theo chuẩn MVVM:
  * Tạo class `SystemMonitorService` chứa 2 đối tượng `PerformanceCounter`:
    * CPU: `new("Processor", "% Processor Time", "_Total")`
    * RAM: `new("Memory", "Available MBytes")` (tính % RAM vật lý `(Total - Available) / Total * 100` khớp với Task Manager)
  * Cung cấp 2 hàm `GetCpuUsage()` và `GetRamUsage()`.
  * Trong `MainWindowViewModel`, khai báo 2 thuộc tính `float Cpu` và `float Ram`, chạy vòng lặp ngầm `Task.Run` với `await Task.Delay(2000)` để cập nhật số liệu mỗi 2 giây và hiển thị lên thanh `ProgressBar` trên Dashboard.
* [ ] **Bước 4.3:** Áp dụng `StatusToColorConverter` (triển khai `IValueConverter`):
  * `"Running"` $\rightarrow$ Màu xanh lá (`#2ECC71`)
  * `"Stopped"` $\rightarrow$ Màu xám hoặc đỏ nhạt (`#E74C3C`)
  * `"Starting..."` / `"Stopping..."` $\rightarrow$ Màu cam/vàng (`#F39C12`)
* [ ] **Bước 4.4:** Áp dụng `<Style>` và `<Trigger>` (với thẻ `<Setter>`) cho các nút bấm Start/Stop để có hiệu ứng đổi màu khi di chuột (`IsMouseOver`) và khi nút bị vô hiệu hóa (`IsEnabled="False"`).

## 4. Tiêu Chí Nghiệm Thu (Checklist Hoàn Thành Phase 1)

* [ ] Code-behind (`MainWindow.xaml.cs`) chỉ chứa `InitializeComponent()` và gán `DataContext = new MainWindowViewModel()`.
* [x] ViewModel hoàn toàn tách biệt với API hệ thống (mọi lệnh gọi `ServiceController` và `PerformanceCounter` đều nằm trong tầng `Services`).
* [ ] Bấm Start/Stop không làm đơ cửa sổ (vẫn kéo di chuyển cửa sổ hoặc gõ vào ô tìm kiếm bình thường trong lúc chờ service bật).
* [ ] Nút Start tự động mờ đi khi service đang chạy hoặc đang trong quá trình khởi động (`Starting...`).
* [x] Thông số % CPU và % RAM trên Dashboard tự động nhảy số mỗi 2 giây mượt mà.
* [ ] Tắt ứng dụng mở lại vẫn giữ nguyên danh sách các service đã ghim (Tracked Services).
* [ ] Hiển thị thông báo rõ ràng khi một service không cho phép dừng hoặc bị quá thời gian phản hồi (Timeout).
