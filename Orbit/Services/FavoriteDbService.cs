using System;
using System.Collections.Generic;
using System.Linq;
using Orbit.Core;
using Orbit.Models.Entities;

namespace Orbit.Services
{
    /// <summary>
    /// Quản lý dữ liệu dịch vụ yêu thích (Favorite Services) lưu trong SQLite bằng EF Core.
    /// </summary>
    public class FavoriteDbService
    {
        private readonly Func<OrbitDbContext> _contextFactory;

        public FavoriteDbService(Func<OrbitDbContext>? contextFactory = null)
        {
            _contextFactory = contextFactory ?? (() => new OrbitDbContext());
        }

        /// <summary>
        /// Lấy danh sách tên các dịch vụ yêu thích của một user từ database.
        /// </summary>
        public HashSet<string> LoadFavoriteServiceNames(int userId)
        {
            try
            {
                using var db = _contextFactory();
                return db.FavoriteServices
                    .Where(f => f.UserId == userId)
                    .Select(f => f.ServiceName)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"Lỗi khi tải danh sách yêu thích từ cơ sở dữ liệu:\n\n{ex.Message}", "Lỗi cơ sở dữ liệu");
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Đồng bộ danh sách tên dịch vụ yêu thích của một user vào database.
        /// </summary>
        public void SaveFavoriteServices(IEnumerable<string> serviceNames, int userId)
        {
            try
            {
                using var db = _contextFactory();
                var current = db.FavoriteServices.Where(f => f.UserId == userId).ToList();
                var targetNames = serviceNames.Distinct(StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);

                // Xóa các dịch vụ bỏ yêu thích
                var toRemove = current.Where(c => !targetNames.Contains(c.ServiceName)).ToList();
                if (toRemove.Count > 0)
                {
                    db.FavoriteServices.RemoveRange(toRemove);
                }

                // Thêm các dịch vụ mới yêu thích
                var existingNames = current.Select(c => c.ServiceName).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var toAdd = targetNames
                    .Where(name => !existingNames.Contains(name))
                    .Select(name => new FavoriteService
                    {
                        UserId = userId,
                        ServiceName = name,
                        AddedDate = DateTime.Now
                    })
                    .ToList();

                if (toAdd.Count > 0)
                {
                    db.FavoriteServices.AddRange(toAdd);
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"Lỗi khi lưu danh sách yêu thích vào cơ sở dữ liệu:\n\n{ex.Message}", "Lỗi cơ sở dữ liệu");
            }
        }

        /// <summary>
        /// Tự động lấy UserId phù hợp (ưu tiên theo tài khoản Windows đăng nhập hoặc user đầu tiên).
        /// </summary>
        public int GetDefaultUserId()
        {
            try
            {
                using var db = _contextFactory();
                var winUser = Environment.UserName;
                var user = db.Users.FirstOrDefault(u => u.Username == winUser) ?? db.Users.FirstOrDefault();
                return user?.UserId ?? 1;
            }
            catch
            {
                return 1;
            }
        }
    }
}
