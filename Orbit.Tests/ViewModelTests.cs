using System;
using System.Linq;
using Orbit.Models;
using Orbit.Services;
using Orbit.ViewModels;
using Xunit;

namespace Orbit.Tests
{
    public class ViewModelTests
    {
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
            var dbService = new FavoriteDbService();
            var manager = new WindowsServiceManager();
            var vm = new MainWindowViewModel(manager, dbService);

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
            var dbService = new FavoriteDbService();
            var manager = new WindowsServiceManager();
            var vm = new MainWindowViewModel(manager, dbService);

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
        public void MainWindowViewModel_PinningPersistsToDatabase()
        {
            var dbService = new FavoriteDbService();
            var manager = new WindowsServiceManager();
            var vm = new MainWindowViewModel(manager, dbService);

            var timeout = DateTime.UtcNow.AddSeconds(5);
            while (vm.IsBusy && DateTime.UtcNow < timeout)
            {
                System.Threading.Thread.Sleep(50);
            }

            var target = vm.DisplayedServices.First();
            bool initialTracked = target.IsTracked;

            try
            {
                // Toggle pin
                vm.ToggleTrack(target);
                Assert.NotEqual(initialTracked, target.IsTracked);

                // Verify persistence by querying fresh db service
                var reloadedDbService = new FavoriteDbService();
                var tracked = reloadedDbService.LoadFavoriteServiceNames(vm.CurrentUserId);

                if (target.IsTracked)
                {
                    Assert.Contains(target.ServiceName, tracked);
                }
                else
                {
                    Assert.DoesNotContain(target.ServiceName, tracked);
                }
            }
            finally
            {
                // Restore original state
                if (target.IsTracked != initialTracked)
                {
                    vm.ToggleTrack(target);
                }
            }
        }

        [Fact]
        public void MainWindowViewModel_CanExecuteRulesForServiceControls()
        {
            var dbService = new FavoriteDbService();
            var manager = new WindowsServiceManager();
            var vm = new MainWindowViewModel(manager, dbService);

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

        [Fact]
        public void MainWindowViewModel_FavoriteServicesCollectionAndSearchWork()
        {
            var dbService = new FavoriteDbService();
            var manager = new WindowsServiceManager();
            var vm = new MainWindowViewModel(manager, dbService);

            var timeout = DateTime.UtcNow.AddSeconds(5);
            while (vm.IsBusy && DateTime.UtcNow < timeout)
            {
                System.Threading.Thread.Sleep(50);
            }

            var service = vm.DisplayedServices.First();
            bool wasTracked = service.IsTracked;
            if (!service.IsTracked)
            {
                vm.ToggleTrack(service);
            }

            try
            {
                Assert.True(vm.HasFavoriteServices);
                Assert.Contains(vm.FavoriteServices, s => s.ServiceName == service.ServiceName);

                // Test search in favorites
                vm.FavoriteSearchText = service.ServiceName;
                Assert.Contains(vm.FavoriteServices, s => s.ServiceName == service.ServiceName);

                vm.FavoriteSearchText = "non_existent_favorite_search_query_999";
                Assert.Empty(vm.FavoriteServices);

                vm.ClearFavoriteSearchCommand.Execute(null);
                Assert.NotEmpty(vm.FavoriteServices);
            }
            finally
            {
                if (!wasTracked && service.IsTracked)
                {
                    vm.ToggleTrack(service);
                }
            }
        }

        [Fact]
        public void MainWindowViewModel_AutoDetectDevServicesWorks()
        {
            var dbService = new FavoriteDbService();
            var manager = new WindowsServiceManager();
            var vm = new MainWindowViewModel(manager, dbService);

            var timeout = DateTime.UtcNow.AddSeconds(5);
            while (vm.IsBusy && DateTime.UtcNow < timeout)
            {
                System.Threading.Thread.Sleep(50);
            }

            Assert.True(vm.AutoDetectDevServicesCommand.CanExecute(null));
            vm.AutoDetectDevServicesCommand.Execute(null);

            // Status message must indicate result
            Assert.False(string.IsNullOrWhiteSpace(vm.StatusMessage));
        }

        [Fact]
        public void MainWindowViewModel_CanInitializeWithAdminUser()
        {
            var auth = new AuthService();
            var admin = auth.Login("admin", "admin123");
            Assert.NotNull(admin);

            var dbService = new FavoriteDbService();
            var manager = new WindowsServiceManager();
            var vm = new MainWindowViewModel(manager, dbService, admin);

            Assert.Equal(1, vm.CurrentUserId);
            Assert.Equal("Administrator", vm.CurrentUserName);
            Assert.Equal("👤 Administrator", vm.CurrentUserDisplay);
        }

        [Fact]
        public void MainWindowViewModel_CanInitializeWithCamerUser()
        {
            var auth = new AuthService();
            var camer = auth.Login("camer", "123456");
            Assert.NotNull(camer);

            var dbService = new FavoriteDbService();
            var manager = new WindowsServiceManager();
            var vm = new MainWindowViewModel(manager, dbService, camer);

            Assert.Equal(2, vm.CurrentUserId);
            Assert.Equal("Developer Camer", vm.CurrentUserName);
            Assert.Equal("👤 Developer Camer", vm.CurrentUserDisplay);
        }
    }
}
