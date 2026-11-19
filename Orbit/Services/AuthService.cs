using System;
using System.Linq;
using Orbit.Core;
using Orbit.Models.Entities;

namespace Orbit.Services
{
    /// <summary>
    /// Service xác thực người dùng đăng nhập qua SQLite (EF Core).
    /// </summary>
    public class AuthService
    {
        private readonly Func<OrbitDbContext> _contextFactory;

        public AuthService(Func<OrbitDbContext>? contextFactory = null)
        {
            _contextFactory = contextFactory ?? (() => new OrbitDbContext());
        }

        /// <summary>
        /// Xác thực tài khoản với tên đăng nhập và mật khẩu.
        /// Trả về đối tượng User nếu khớp, null nếu sai thông tin.
        /// </summary>
        public User? Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            try
            {
                using var db = _contextFactory();
                return db.Users.FirstOrDefault(u => u.Username == username.Trim() && u.Password == password);
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"Lỗi kết nối cơ sở dữ liệu khi đăng nhập:\n\n{ex.Message}", "Lỗi kết nối CSDL");
                return null;
            }
        }
    }
}
