using Orbit.Services;
using Xunit;

namespace Orbit.Tests
{
    public class AuthServiceTests
    {
        [Fact]
        public void Login_ValidCredentials_ReturnsUser()
        {
            var auth = new AuthService();
            var user = auth.Login("camer", "123456");

            Assert.NotNull(user);
            Assert.Equal(2, user.UserId);
            Assert.Equal("camer", user.Username);
            Assert.Equal("Developer Camer", user.FullName);
        }

        [Fact]
        public void Login_AdminCredentials_ReturnsAdminUser()
        {
            var auth = new AuthService();
            var user = auth.Login("admin", "admin123");

            Assert.NotNull(user);
            Assert.Equal(1, user.UserId);
            Assert.Equal("Admin", user.Role);
        }

        [Fact]
        public void Login_WrongPassword_ReturnsNull()
        {
            var auth = new AuthService();
            var user = auth.Login("camer", "wrong_password_999");

            Assert.Null(user);
        }

        [Fact]
        public void Login_NonExistentUser_ReturnsNull()
        {
            var auth = new AuthService();
            var user = auth.Login("user_does_not_exist", "123456");

            Assert.Null(user);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("camer", "")]
        [InlineData("", "123456")]
        [InlineData("   ", "   ")]
        public void Login_EmptyOrWhitespace_ReturnsNull(string username, string password)
        {
            var auth = new AuthService();
            var user = auth.Login(username, password);

            Assert.Null(user);
        }
    }
}
