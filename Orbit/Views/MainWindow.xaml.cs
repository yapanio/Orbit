using System.Windows;
using Orbit.Models.Entities;
using Orbit.ViewModels;

namespace Orbit.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// Strict MVVM code-behind: only initializes components and binds DataContext.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() : this(null)
        {
        }

        public MainWindow(User? loggedInUser)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(loggedInUser);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Close();

            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true && loginWindow.LoggedInUser != null)
            {
                var newWindow = new MainWindow(loginWindow.LoggedInUser);
                Application.Current.MainWindow = newWindow;
                Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                newWindow.Show();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
    }
}
