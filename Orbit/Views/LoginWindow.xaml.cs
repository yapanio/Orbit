using System.Windows;
using Orbit.Models.Entities;
using Orbit.Services;

namespace Orbit.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService;

        public User? LoggedInUser { get; private set; }

        public LoginWindow(AuthService? authService = null)
        {
            InitializeComponent();
            _authService = authService ?? new AuthService();

            txtUsername.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                txtError.Text = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return;
            }

            var user = _authService.Login(username, password);
            if (user != null)
            {
                LoggedInUser = user;
                DialogResult = true;
                Close();
            }
            else
            {
                txtError.Text = "Tên đăng nhập hoặc mật khẩu không chính xác.";
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
