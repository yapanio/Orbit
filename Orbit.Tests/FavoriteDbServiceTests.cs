using System.Collections.Generic;
using System.Linq;
using Orbit.Services;
using Xunit;

namespace Orbit.Tests
{
    public class FavoriteDbServiceTests
    {
        [Fact]
        public void LoadFavoriteServiceNames_ReturnsFavoritesForCamer()
        {
            var dbService = new FavoriteDbService();
            // User camer has UserId = 2
            var favorites = dbService.LoadFavoriteServiceNames(2);

            Assert.NotNull(favorites);
            Assert.NotEmpty(favorites);
            Assert.Contains("MSSQLSERVER", favorites);
        }

        [Fact]
        public void SaveFavoriteServices_CanAddAndRemoveFavorites()
        {
            var dbService = new FavoriteDbService();
            const int testUserId = 1; // Admin user
            const string testServiceName = "test_custom_service_xyz";

            var initial = dbService.LoadFavoriteServiceNames(testUserId);

            try
            {
                // Add test service
                var updated = new HashSet<string>(initial) { testServiceName };
                dbService.SaveFavoriteServices(updated, testUserId);

                var reloaded = dbService.LoadFavoriteServiceNames(testUserId);
                Assert.Contains(testServiceName, reloaded);

                // Remove test service
                dbService.SaveFavoriteServices(initial, testUserId);
                var afterRemoval = dbService.LoadFavoriteServiceNames(testUserId);
                Assert.DoesNotContain(testServiceName, afterRemoval);
            }
            finally
            {
                // Guarantee cleanup
                dbService.SaveFavoriteServices(initial, testUserId);
            }
        }

        [Fact]
        public void GetDefaultUserId_ReturnsPositiveId()
        {
            var dbService = new FavoriteDbService();
            var userId = dbService.GetDefaultUserId();

            Assert.True(userId > 0);
        }
    }
}
