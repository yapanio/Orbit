using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Orbit.Core;
using Orbit.Models;
using Orbit.Models.Entities;
using Orbit.Services;

namespace Orbit.ViewModels
{
    /// <summary>
    /// ViewModel for MainWindow managing Windows Services list, filtering,
    /// execution controls (Start, Stop, Restart), statistics, and persistence.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private readonly WindowsServiceManager _serviceManager;
        private readonly FavoriteDbService _dbService;
        private readonly SystemMonitorService _monitorService;

        public int CurrentUserId { get; set; } = 1;

        private static readonly string[] CommonDevKeywords = new[]
        {
            "sql", "postgres", "mysql", "mariadb", "mongo", "redis",
            "docker", "w3svc", "apache", "nginx", "tomcat",
            "rabbitmq", "kafka", "elastic", "oracle"
        };

        private List<ServiceItemModel> _allServices = new();
        private ServiceItemModel? _selectedService;
        private string _searchText = string.Empty;
        private string _favoriteSearchText = string.Empty;
        private string _statusMessage = "Ready";
        private bool _isBusy;
        private float _cpu;
        private float _ram;
        private bool _isDisposed;

        private string _currentUserName = "Người dùng";

        public string CurrentUserName
        {
            get => _currentUserName;
            set
            {
                if (SetProperty(ref _currentUserName, value))
                {
                    OnPropertyChanged(nameof(CurrentUserDisplay));
                }
            }
        }

        public string CurrentUserDisplay => $"👤 {CurrentUserName}";

        public MainWindowViewModel(User? loggedInUser = null)
            : this(new WindowsServiceManager(), new FavoriteDbService(), new SystemMonitorService(), loggedInUser)
        {
        }

        public MainWindowViewModel(WindowsServiceManager serviceManager, FavoriteDbService dbService, User? loggedInUser = null)
            : this(serviceManager, dbService, new SystemMonitorService(), loggedInUser)
        {
        }

        public MainWindowViewModel(WindowsServiceManager serviceManager, FavoriteDbService dbService, SystemMonitorService monitorService, User? loggedInUser = null)
        {
            _serviceManager = serviceManager;
            _dbService = dbService;
            _monitorService = monitorService;

            if (loggedInUser != null)
            {
                CurrentUserId = loggedInUser.UserId;
                _currentUserName = loggedInUser.FullName;
            }
            else
            {
                CurrentUserId = _dbService.GetDefaultUserId();
                try
                {
                    using var db = new OrbitDbContext();
                    var user = db.Users.Find(CurrentUserId);
                    if (user != null)
                    {
                        _currentUserName = user.FullName;
                    }
                }
                catch { }
            }

            DisplayedServices = new ObservableCollection<ServiceItemModel>();
            FavoriteServices = new ObservableCollection<ServiceItemModel>();

            ToggleCommand = new RelayCommand(
                _ => ToggleSelectedService(),
                _ => !IsBusy && SelectedService != null
            );

            StartCommand = new RelayCommand(
                _ => StartSelectedService(),
                _ => !IsBusy && SelectedService != null && SelectedService.Status == "Stopped"
            );

            StopCommand = new RelayCommand(
                _ => StopSelectedService(),
                _ => !IsBusy && SelectedService != null && SelectedService.Status == "Running" && SelectedService.CanStop
            );

            RestartCommand = new RelayCommand(
                _ => RestartSelectedService(),
                _ => !IsBusy && SelectedService != null && SelectedService.Status == "Running" && SelectedService.CanStop
            );

            RefreshCommand = new RelayCommand(
                _ => RefreshServices(),
                _ => !IsBusy
            );

            ToggleTrackCommand = new RelayCommand(
                param => ToggleTrack(param as ServiceItemModel ?? SelectedService),
                _ => !IsBusy && (SelectedService != null)
            );

            ClearSearchCommand = new RelayCommand(
                _ => SearchText = string.Empty,
                _ => !string.IsNullOrEmpty(SearchText)
            );

            ClearFavoriteSearchCommand = new RelayCommand(
                _ => FavoriteSearchText = string.Empty,
                _ => !string.IsNullOrEmpty(FavoriteSearchText)
            );

            AutoDetectDevServicesCommand = new RelayCommand(
                _ => AutoDetectDevServices(),
                _ => !IsBusy
            );

            // Initial load
            RefreshServices();

            // Background system resource monitoring
            StartSystemMonitoring();
        }



        public ObservableCollection<ServiceItemModel> DisplayedServices { get; }
        public ObservableCollection<ServiceItemModel> FavoriteServices { get; }

        public ServiceItemModel? SelectedService
        {
            get => _selectedService;
            set
            {
                if (SetProperty(ref _selectedService, value))
                {
                    RelayCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilter();
                }
            }
        }

        public string FavoriteSearchText
        {
            get => _favoriteSearchText;
            set
            {
                if (SetProperty(ref _favoriteSearchText, value))
                {
                    ApplyFavoriteFilter();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    RelayCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public float Cpu
        {
            get => _cpu;
            set => SetProperty(ref _cpu, value);
        }

        public float Ram
        {
            get => _ram;
            set => SetProperty(ref _ram, value);
        }

        #region Statistics

        public int TotalServicesCount => _allServices.Count;
        public int RunningServicesCount => _allServices.Count(s => s.IsRunning);
        public int TrackedTotalCount => _allServices.Count(s => s.IsTracked);
        public int TrackedRunningCount => _allServices.Count(s => s.IsTracked && s.IsRunning);
        public int TrackedStoppedCount => _allServices.Count(s => s.IsTracked && !s.IsRunning);
        public bool HasFavoriteServices => TrackedTotalCount > 0;
        public int DisplayedCount => DisplayedServices.Count;
        public int FavoriteDisplayedCount => FavoriteServices.Count;

        private void UpdateStatistics()
        {
            OnPropertyChanged(nameof(TotalServicesCount));
            OnPropertyChanged(nameof(RunningServicesCount));
            OnPropertyChanged(nameof(TrackedTotalCount));
            OnPropertyChanged(nameof(TrackedRunningCount));
            OnPropertyChanged(nameof(TrackedStoppedCount));
            OnPropertyChanged(nameof(HasFavoriteServices));
            OnPropertyChanged(nameof(DisplayedCount));
            OnPropertyChanged(nameof(FavoriteDisplayedCount));
        }

        #endregion

        #region Commands

        public ICommand ToggleCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand RestartCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ToggleTrackCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand ClearFavoriteSearchCommand { get; }
        public ICommand AutoDetectDevServicesCommand { get; }

        #endregion

        #region Service Control Actions

        public async void ToggleSelectedService()
        {
            if (SelectedService == null) return;
            if (SelectedService.Status == "Stopped") StartSelectedService();
            else StopSelectedService();

            
        }
        public async void StartSelectedService()
        {
            if (SelectedService == null || SelectedService.Status != "Stopped") return;

            var target = SelectedService;
            var serviceName = target.ServiceName;
            var displayName = target.DisplayName;

            try
            {
                IsBusy = true;
                StatusMessage = $"Starting '{displayName}'...";
                target.Status = "Starting...";
                RelayCommand.RaiseCanExecuteChanged();

                await Task.Run(() => _serviceManager.StartService(serviceName, TimeSpan.FromSeconds(15)));

                target.Status = "Running";
                StatusMessage = $"Service '{displayName}' started successfully.";
            }
            catch (Exception ex)
            {
                target.Status = _serviceManager.GetServiceStatus(serviceName);
                StatusMessage = $"Error starting '{displayName}': {ex.Message}";
                DialogHelper.ShowError($"Không thể khởi chạy dịch vụ '{displayName}':\n\n{ex.Message}", "Lỗi khởi chạy dịch vụ");
            }
            finally
            {
                _serviceManager.RefreshService(target);
                IsBusy = false;
                UpdateStatistics();
                RelayCommand.RaiseCanExecuteChanged();
            }
        }

        public async void StopSelectedService()
        {
            if (SelectedService == null || SelectedService.Status != "Running" || !SelectedService.CanStop) return;

            var target = SelectedService;
            var serviceName = target.ServiceName;
            var displayName = target.DisplayName;

            try
            {
                IsBusy = true;
                StatusMessage = $"Stopping '{displayName}'...";
                target.Status = "Stopping...";
                RelayCommand.RaiseCanExecuteChanged();

                await Task.Run(() => _serviceManager.StopService(serviceName, TimeSpan.FromSeconds(15)));

                target.Status = "Stopped";
                StatusMessage = $"Service '{displayName}' stopped successfully.";
            }
            catch (Exception ex)
            {
                target.Status = _serviceManager.GetServiceStatus(serviceName);
                StatusMessage = $"Error stopping '{displayName}': {ex.Message}";
                DialogHelper.ShowError($"Không thể dừng dịch vụ '{displayName}':\n\n{ex.Message}", "Lỗi dừng dịch vụ");
            }
            finally
            {
                _serviceManager.RefreshService(target);
                IsBusy = false;
                UpdateStatistics();
                RelayCommand.RaiseCanExecuteChanged();
            }
        }

        public async void RestartSelectedService()
        {
            if (SelectedService == null || SelectedService.Status != "Running" || !SelectedService.CanStop) return;

            var target = SelectedService;
            var serviceName = target.ServiceName;
            var displayName = target.DisplayName;

            try
            {
                IsBusy = true;
                StatusMessage = $"Restarting '{displayName}'...";
                target.Status = "Stopping...";
                RelayCommand.RaiseCanExecuteChanged();

                await Task.Run(() => _serviceManager.RestartService(serviceName, TimeSpan.FromSeconds(15)));

                target.Status = "Running";
                StatusMessage = $"Service '{displayName}' restarted successfully.";
            }
            catch (Exception ex)
            {
                target.Status = _serviceManager.GetServiceStatus(serviceName);
                StatusMessage = $"Error restarting '{displayName}': {ex.Message}";
                DialogHelper.ShowError($"Không thể khởi động lại dịch vụ '{displayName}':\n\n{ex.Message}", "Lỗi khởi động lại dịch vụ");
            }
            finally
            {
                _serviceManager.RefreshService(target);
                IsBusy = false;
                UpdateStatistics();
                RelayCommand.RaiseCanExecuteChanged();
            }
        }

        public async void RefreshServices()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = "Scanning Windows Services...";

                var trackedNames = _dbService.LoadFavoriteServiceNames(CurrentUserId);
                var services = await Task.Run(() => _serviceManager.GetAllServices());

                foreach (var s in services)
                {
                    if (trackedNames.Contains(s.ServiceName))
                    {
                        s.IsTracked = true;
                    }
                }

                _allServices = services;
                ApplyFilter();
                UpdateFavoriteServices();
                StatusMessage = $"Loaded {_allServices.Count} services at {DateTime.Now:HH:mm:ss}.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to scan services: {ex.Message}";
                DialogHelper.ShowError($"Lỗi khi quét danh sách Windows Services:\n\n{ex.Message}", "Lỗi quét dịch vụ");
            }
            finally
            {
                IsBusy = false;
                UpdateStatistics();
                RelayCommand.RaiseCanExecuteChanged();
            }
        }

        public void ToggleTrack(ServiceItemModel? target)
        {
            if (target == null) return;

            target.IsTracked = !target.IsTracked;

            var trackedNames = _allServices.Where(s => s.IsTracked).Select(s => s.ServiceName);
            _dbService.SaveFavoriteServices(trackedNames, CurrentUserId);

            UpdateStatistics();
            UpdateFavoriteServices();

            StatusMessage = target.IsTracked
                ? $"Pinned '{target.DisplayName}' to tracked services."
                : $"Unpinned '{target.DisplayName}'.";
        }

        public void AutoDetectDevServices()
        {
            if (IsBusy) return;

            int newlyAdded = 0;
            foreach (var s in _allServices)
            {
                if (s.IsTracked) continue;

                bool isDevService = CommonDevKeywords.Any(kw =>
                    s.ServiceName.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    s.DisplayName.Contains(kw, StringComparison.OrdinalIgnoreCase));

                if (isDevService)
                {
                    s.IsTracked = true;
                    newlyAdded++;
                }
            }

            if (newlyAdded > 0)
            {
                var trackedNames = _allServices.Where(s => s.IsTracked).Select(s => s.ServiceName);
                _dbService.SaveFavoriteServices(trackedNames, CurrentUserId);

                UpdateStatistics();
                UpdateFavoriteServices();

                StatusMessage = $"Added {newlyAdded} dev environment service(s) to favorites.";
            }
            else
            {
                StatusMessage = "No new dev environment services were found on this machine.";
            }
        }

        #endregion

        #region Search and Filtering

        private void ApplyFilter()
        {
            var query = _allServices.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var term = SearchText.Trim();
                query = query.Where(s =>
                    s.ServiceName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    s.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            var filtered = query.ToList();
            //filtered = filtered.OrderBy(s => s.IsRunning).ToList();

            // Preserve selection if possible
            var prevSelectedName = SelectedService?.ServiceName;

            DisplayedServices.Clear();
            foreach (var item in filtered)
            {
                DisplayedServices.Add(item);
            }


            if (prevSelectedName != null)
            {
                SelectedService = DisplayedServices.FirstOrDefault(s => s.ServiceName.Equals(prevSelectedName, StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedService == null && DisplayedServices.Count > 0)
            {
                SelectedService = DisplayedServices[0];
            }

            UpdateStatistics();
        }

        private void UpdateFavoriteServices()
        {
            ApplyFavoriteFilter();
            OnPropertyChanged(nameof(TrackedTotalCount));
            OnPropertyChanged(nameof(TrackedRunningCount));
            OnPropertyChanged(nameof(TrackedStoppedCount));
            OnPropertyChanged(nameof(HasFavoriteServices));
        }

        private void ApplyFavoriteFilter()
        {
            var query = _allServices.Where(s => s.IsTracked);

            if (!string.IsNullOrWhiteSpace(FavoriteSearchText))
            {
                var term = FavoriteSearchText.Trim();
                query = query.Where(s =>
                    s.ServiceName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    s.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            var list = query.ToList();

            FavoriteServices.Clear();
            foreach (var item in list)
            {
                FavoriteServices.Add(item);
            }

            OnPropertyChanged(nameof(FavoriteDisplayedCount));
        }

        #endregion

        #region System Monitor

        private void StartSystemMonitoring()
        {
            Task.Run(async () =>
            {
                while (!_isDisposed)
                {
                    try
                    {
                        Cpu = _monitorService.GetCpuUsage();
                        Ram = _monitorService.GetRamUsage();
                    }
                    catch
                    {
                        // Ignore monitoring errors to maintain loop continuity
                    }

                    await Task.Delay(2000);
                }
            });
        }

        public void Dispose()
        {
            _isDisposed = true;
            _monitorService.Dispose();
        }

        #endregion
    }
}
