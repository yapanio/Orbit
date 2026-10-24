using System;
using System.IO;
using System.Threading.Tasks;
using Orbit.Services;
using Orbit.ViewModels;
using Xunit;

namespace Orbit.Tests
{
    public class SystemMonitorTests : IDisposable
    {
        private readonly string _testJsonPath;

        public SystemMonitorTests()
        {
            _testJsonPath = Path.Combine(Path.GetTempPath(), $"orbit_monitor_test_{Guid.NewGuid():N}.json");
        }

        public void Dispose()
        {
            if (File.Exists(_testJsonPath))
            {
                try { File.Delete(_testJsonPath); } catch { }
            }
        }

        private class FakeSystemMonitorService : SystemMonitorService
        {
            public float MockCpu { get; set; } = 35.5f;
            public float MockRam { get; set; } = 62.0f;

            public override float GetCpuUsage() => MockCpu;
            public override float GetRamUsage() => MockRam;
        }

        [Fact]
        public void SystemMonitorService_InstantiatesAndReturnsSafeMetrics()
        {
            using var monitor = new SystemMonitorService();

            var cpu = monitor.GetCpuUsage();
            var ram = monitor.GetRamUsage();

            Assert.InRange(cpu, 0f, 100f);
            Assert.InRange(ram, 0f, 100f);
        }

        [Fact]
        public async Task MainWindowViewModel_PollsMetricsFromMonitorService()
        {
            var fakeMonitor = new FakeSystemMonitorService
            {
                MockCpu = 45.0f,
                MockRam = 70.0f
            };

            var storage = new LocalStorageService(_testJsonPath);
            var manager = new WindowsServiceManager();

            using var vm = new MainWindowViewModel(manager, storage, fakeMonitor);

            // Wait a short moment for the background Task.Run to record values
            await Task.Delay(100);

            Assert.Equal(45.0f, vm.Cpu);
            Assert.Equal(70.0f, vm.Ram);
        }

        [Fact]
        public void MainWindowViewModel_CanSetCpuAndRamProperties()
        {
            var fakeMonitor = new FakeSystemMonitorService
            {
                MockCpu = 10.0f,
                MockRam = 20.0f
            };

            var storage = new LocalStorageService(_testJsonPath);
            var manager = new WindowsServiceManager();

            using var vm = new MainWindowViewModel(manager, storage, fakeMonitor);

            var cpuChanged = false;
            var ramChanged = false;

            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vm.Cpu)) cpuChanged = true;
                if (e.PropertyName == nameof(vm.Ram)) ramChanged = true;
            };

            vm.Cpu = 50.0f;
            vm.Ram = 80.0f;

            Assert.True(cpuChanged);
            Assert.True(ramChanged);
        }
    }
}
