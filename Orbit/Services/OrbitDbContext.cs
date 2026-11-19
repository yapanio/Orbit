using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Orbit.Models.Entities;

namespace Orbit.Services;

public partial class OrbitDbContext : DbContext
{
    public OrbitDbContext()
    {
    }

    public OrbitDbContext(DbContextOptions<OrbitDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<FavoriteService> FavoriteServices { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=OrbitDb;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FavoriteService>(entity =>
        {
            entity.HasKey(e => e.FavoriteId);

            entity.HasIndex(e => new { e.UserId, e.ServiceName }, "UQ_User_Service").IsUnique();

            entity.Property(e => e.AddedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
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
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("User");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
