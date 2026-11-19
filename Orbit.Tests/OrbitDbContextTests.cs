using System.Linq;
using Microsoft.EntityFrameworkCore;
using Orbit.Models.Entities;
using Orbit.Services;
using Xunit;

namespace Orbit.Tests;

public class OrbitDbContextTests
{
    [Fact]
    public void CanConnectAndQueryUsersFromDatabase()
    {
        using var context = new OrbitDbContext();
        
        // Assert database can be connected
        Assert.True(context.Database.CanConnect(), "Database connection failed. Ensure SQLite database is created.");

        // Assert seeded users exist
        var adminUser = context.Users.Include(u => u.FavoriteServices).FirstOrDefault(u => u.Username == "admin");
        var camerUser = context.Users.Include(u => u.FavoriteServices).FirstOrDefault(u => u.Username == "camer");

        Assert.NotNull(adminUser);
        Assert.Equal("Administrator", adminUser.FullName);

        Assert.NotNull(camerUser);
        Assert.Equal("Developer Camer", camerUser.FullName);
        Assert.NotEmpty(camerUser.FavoriteServices);
        Assert.Contains(camerUser.FavoriteServices, f => f.ServiceName == "MSSQLSERVER");
    }
}
