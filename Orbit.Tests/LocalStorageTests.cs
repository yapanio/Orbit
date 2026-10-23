using System;
using System.IO;
using System.Linq;
using Orbit.Services;
using Xunit;

namespace Orbit.Tests
{
    public class LocalStorageTests : IDisposable
    {
        private readonly string _testFilePath;

        public LocalStorageTests()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), $"orbit_test_{Guid.NewGuid():N}.json");
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
            {
                try { File.Delete(_testFilePath); } catch { }
            }
        }

        [Fact]
        public void LoadTrackedServices_WhenFileDoesNotExist_ReturnsEmptySet()
        {
            var service = new LocalStorageService(_testFilePath);
            var result = service.LoadTrackedServices();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void SaveAndLoadTrackedServices_RoundtripSuccess()
        {
            var service = new LocalStorageService(_testFilePath);
            var servicesToTrack = new[] { "mssqlserver", "docker", "redis" };

            service.SaveTrackedServices(servicesToTrack);

            Assert.True(File.Exists(_testFilePath));

            var loaded = service.LoadTrackedServices();
            Assert.Equal(3, loaded.Count);
            Assert.Contains("mssqlserver", loaded);
            Assert.Contains("docker", loaded);
            Assert.Contains("redis", loaded);
        }

        [Fact]
        public void SaveTrackedServices_CaseInsensitiveDeduplication()
        {
            var service = new LocalStorageService(_testFilePath);
            var duplicateList = new[] { "Docker", "docker", "DOCKER", "mssql" };

            service.SaveTrackedServices(duplicateList);

            var loaded = service.LoadTrackedServices();
            Assert.Equal(2, loaded.Count);
            Assert.Contains("docker", loaded);
            Assert.Contains("mssql", loaded);
        }
    }
}
