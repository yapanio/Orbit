using System;
using System.IO;
using System.Linq;
using Orbit.Models;
using Orbit.Services;
using Orbit.ViewModels;
using Xunit;

namespace Orbit.Tests
{
    public class ViewModelTests : IDisposable
    {
        private readonly string _testJsonPath;

        public ViewModelTests()
        {
            _testJsonPath = Path.Combine(Path.GetTempPath(), $"orbit_vm_test_{Guid.NewGuid():N}.json");
        }

        public void Dispose()
        {
            if (File.Exists(_testJsonPath))
            {
                try { File.Delete(_testJsonPath); } catch { }
            }
        }

        [Fact]
        public void WindowsServiceManager_CanQueryServicesOnLocalMachine()
        {
            var manager = new WindowsServiceManager();
            var services = manager.GetAllServices();

            Assert.NotNull(services);
            Assert.NotEmpty(services);

            // Every service should have a ServiceName
            Assert.All(services, s =>
            {
                Assert.False(string.IsNullOrWhiteSpace(s.ServiceName));
                Assert.False(string.IsNullOrWhiteSpace(s.Status));
            });
        }

        [Fact]
        public void MainWindowViewModel_InitializesAndLoadsServices()
        {
            var storage = new LocalStorageService(_testJsonPath);
            var manager = new WindowsServiceManager();
            var vm = new MainWindowViewModel(manager, storage);

            // Initial load executes asynchronously; wait briefly if necessary
            var timeout = DateTime.UtcNow.AddSeconds(5);
            while (vm.IsBusy && DateTime.UtcNow < timeout)
            {
                System.Threading.Thread.Sleep(50);
            }

            Assert.False(vm.IsBusy);
            Assert.NotEmpty(vm.DisplayedServices);
            Assert.True(vm.TotalServicesCount > 0);
            Assert.NotNull(vm.SelectedService);
        }

        [Fact]
        public void MainWindowViewModel_SearchFilteringWorks()
        {
            var storage = new LocalStorageService(_testJsonPath);
            var manager = new WindowsServiceManager();
            var vm = new MainWindowViewModel(manager, storage);

            var timeout = DateTime.UtcNow.AddSeconds(5);
            while (vm.IsBusy && DateTime.UtcNow < timeout)
            {
                System.Threading.Thread.Sleep(50);
            }

            var initialCount = vm.DisplayedServices.Count;
            Assert.True(initialCount > 0);

            // Filter with nonexistent query
            vm.SearchText = "xyz_non_existent_service_name_12345";
            Assert.Empty(vm.DisplayedServices);

            // Clear filter
            vm.SearchText = string.Empty;
            Assert.Equal(initialCount, vm.DisplayedServices.Count);
        }

        [Fact]
        public void MainWindowViewModel_PinningPersistsToDisk()
        {
            var storage = new LocalStorageService(_testJsonPath);
            var manager = new WindowsServiceManager();
            var vm = new MainWindowViewModel(manager, storage);

            var timeout = DateTime.UtcNow.AddSeconds(5);
            while (vm.IsBusy && DateTime.UtcNow < timeout)
            {
                System.Threading.Thread.Sleep(50);
            }

            var target = vm.DisplayedServices.First();
            bool initialTracked = target.IsTracked;

            vm.ToggleTrack(target);
            Assert.NotEqual(initialTracked, target.IsTracked);

            // Verify persistence by loading in fresh service
            var reloadedStorage = new LocalStorageService(_testJsonPath);
            var tracked = reloadedStorage.LoadTrackedServices();

            if (target.IsTracked)
            {
                Assert.Contains(target.ServiceName, tracked);
            }
            else
            {
                Assert.DoesNotContain(target.ServiceName, tracked);
            }
        }

        [Fact]
        public void MainWindowViewModel_CanExecuteRulesForServiceControls()
        {
            var storage = new LocalStorageService(_testJsonPath);
            var manager = new WindowsServiceManager();
            var vm = new MainWindowViewModel(manager, storage);

            var timeout = DateTime.UtcNow.AddSeconds(5);
            while (vm.IsBusy && DateTime.UtcNow < timeout)
            {
                System.Threading.Thread.Sleep(50);
            }

            var stoppedService = new ServiceItemModel
            {
                ServiceName = "test_stopped",
                DisplayName = "Test Stopped",
                Status = "Stopped",
                CanStop = true
            };

            var runningService = new ServiceItemModel
            {
                ServiceName = "test_running",
                DisplayName = "Test Running",
                Status = "Running",
                CanStop = true
            };

            var transitioningService = new ServiceItemModel
            {
                ServiceName = "test_trans",
                DisplayName = "Test Trans",
                Status = "Starting...",
                CanStop = true
            };

            // Case 1: Stopped service
            vm.SelectedService = stoppedService;
            Assert.True(vm.StartCommand.CanExecute(null));
            Assert.False(vm.StopCommand.CanExecute(null));
            Assert.False(vm.RestartCommand.CanExecute(null));

            // Case 2: Running service
            vm.SelectedService = runningService;
            Assert.False(vm.StartCommand.CanExecute(null));
            Assert.True(vm.StopCommand.CanExecute(null));
            Assert.True(vm.RestartCommand.CanExecute(null));

            // Case 3: Transitioning service (Anti-freeze / double-click protection)
            vm.SelectedService = transitioningService;
            Assert.False(vm.StartCommand.CanExecute(null));
            Assert.False(vm.StopCommand.CanExecute(null));
            Assert.False(vm.RestartCommand.CanExecute(null));
        }
    }
}
