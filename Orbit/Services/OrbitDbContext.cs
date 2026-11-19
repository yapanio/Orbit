using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Orbit.Models.Entities;

namespace Orbit.Services;

public partial class OrbitDbContext : DbContext
{
    private static bool _isInitialized = false;
    private static readonly object _initLock = new();

    /// <summary>
    /// Đường dẫn file database SQLite (mặc định lưu tại thư mục thực thi ứng dụng).
    /// </summary>
    public static string DbPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "orbit.db");

    public OrbitDbContext()
    {
        EnsureDatabaseCreated();
    }

    public OrbitDbContext(DbContextOptions<OrbitDbContext> options)
        : base(options)
    {
        EnsureDatabaseCreated();
    }

    private OrbitDbContext(bool skipInit)
    {
    }

    /// <summary>
    /// Tự động khởi tạo database SQLite và seed dữ liệu mặc định nếu chưa tồn tại.
    /// </summary>
    public static void EnsureDatabaseCreated()
    {
        if (!_isInitialized)
        {
            lock (_initLock)
            {
                if (!_isInitialized)
                {
                    using var context = new OrbitDbContext(skipInit: true);
                    context.Database.EnsureCreated();
                    _isInitialized = true;
                }
            }
        }
    }

    public virtual DbSet<FavoriteService> FavoriteServices { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FavoriteService>(entity =>
        {
            entity.HasKey(e => e.FavoriteId);

            entity.HasIndex(e => new { e.UserId, e.ServiceName }, "UQ_User_Service").IsUnique();

            entity.Property(e => e.AddedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ServiceName).HasMaxLength(200);

            entity.HasOne(d => d.User).WithMany(p => p.FavoriteServices)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_FavoriteServices_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.HasIndex(e => e.Username, "UQ_Users_Username").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("User");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        // Seed dữ liệu mẫu mặc định cho SQLite
        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 1,
                Username = "admin",
                Password = "admin123",
                FullName = "Administrator",
                Role = "Admin",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                UserId = 2,
                Username = "camer",
                Password = "123456",
                FullName = "Developer Camer",
                Role = "User",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<FavoriteService>().HasData(
            new FavoriteService
            {
                FavoriteId = 1,
                UserId = 2,
                ServiceName = "MSSQLSERVER",
                AddedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new FavoriteService
            {
                FavoriteId = 2,
                UserId = 2,
                ServiceName = "wuauserv",
                AddedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new FavoriteService
            {
                FavoriteId = 3,
                UserId = 1,
                ServiceName = "Spooler",
                AddedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
