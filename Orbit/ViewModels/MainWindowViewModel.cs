using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Orbit.Core;
using Orbit.Models;
using Orbit.Services;

namespace Orbit.ViewModels
{
    /// <summary>
    /// ViewModel for MainWindow managing Windows Services list, filtering,
    /// execution controls (Start, Stop, Restart), statistics, and persistence.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly WindowsServiceManager _serviceManager;
        private readonly LocalStorageService _storageService;

        private List<ServiceItemModel> _allServices = new();
        private ServiceItemModel? _selectedService;
        private string _searchText = string.Empty;
        private bool _showTrackedOnly;
        private string _statusMessage = "Ready";
        private bool _isBusy;

        public MainWindowViewModel()
            : this(new WindowsServiceManager(), new LocalStorageService())
        {
        }

        public MainWindowViewModel(WindowsServiceManager serviceManager, LocalStorageService storageService)
        {
            _serviceManager = serviceManager;
            _storageService = storageService;

            DisplayedServices = new ObservableCollection<ServiceItemModel>();

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

            // Initial load
            RefreshServices();
        }

        public ObservableCollection<ServiceItemModel> DisplayedServices { get; }

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

        public bool ShowTrackedOnly
        {
            get => _showTrackedOnly;
            set
            {
                if (SetProperty(ref _showTrackedOnly, value))
                {
                    ApplyFilter();
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

        #region Statistics

        public int TotalServicesCount => _allServices.Count;
        public int RunningServicesCount => _allServices.Count(s => s.IsRunning);
        public int TrackedTotalCount => _allServices.Count(s => s.IsTracked);
        public int TrackedRunningCount => _allServices.Count(s => s.IsTracked && s.IsRunning);
        public int DisplayedCount => DisplayedServices.Count;

        private void UpdateStatistics()
        {
            OnPropertyChanged(nameof(TotalServicesCount));
            OnPropertyChanged(nameof(RunningServicesCount));
            OnPropertyChanged(nameof(TrackedTotalCount));
            OnPropertyChanged(nameof(TrackedRunningCount));
            OnPropertyChanged(nameof(DisplayedCount));
        }

        #endregion

        #region Commands

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand RestartCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ToggleTrackCommand { get; }
        public ICommand ClearSearchCommand { get; }

        #endregion

        #region Service Control Actions

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

                var trackedNames = _storageService.LoadTrackedServices();
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
                StatusMessage = $"Loaded {_allServices.Count} services at {DateTime.Now:HH:mm:ss}.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to scan services: {ex.Message}";
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
            _storageService.SaveTrackedServices(trackedNames);

            UpdateStatistics();

            if (ShowTrackedOnly)
            {
                ApplyFilter();
            }

            StatusMessage = target.IsTracked
                ? $"Pinned '{target.DisplayName}' to tracked services."
                : $"Unpinned '{target.DisplayName}'.";
        }

        #endregion

        #region Search and Filtering

        private void ApplyFilter()
        {
            var query = _allServices.AsEnumerable();

            if (ShowTrackedOnly)
            {
                query = query.Where(s => s.IsTracked);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var term = SearchText.Trim();
                query = query.Where(s =>
                    s.ServiceName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    s.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            var filtered = query.ToList();

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

        #endregion
    }
}
