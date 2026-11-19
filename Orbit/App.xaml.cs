using System;
using System.Threading.Tasks;
using System.Windows;
using Orbit.Core;
using Orbit.Views;

namespace Orbit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Bắt toàn bộ lỗi giao diện chưa được xử lý
            DispatcherUnhandledException += (sender, args) =>
            {
                DialogHelper.ShowError($"Đã xảy ra lỗi không mong muốn:\n\n{args.Exception.Message}", "Lỗi hệ thống");
                args.Handled = true;
            };

            // Bắt toàn bộ lỗi từ các luồng nền (background threads)
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    DialogHelper.ShowError($"Lỗi hệ thống nghiêm trọng:\n\n{ex.Message}", "Lỗi nghiêm trọng");
                }
            };

            // Bắt toàn bộ lỗi từ Task bất đồng bộ (async tasks)
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                DialogHelper.ShowError($"Lỗi tiến trình bất đồng bộ:\n\n{args.Exception.Message}", "Lỗi tiến trình");
                args.SetObserved();
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ngăn WPF tự động tắt app khi LoginWindow đóng lại
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true && loginWindow.LoggedInUser != null)
            {
                var mainWindow = new MainWindow(loginWindow.LoggedInUser);
                MainWindow = mainWindow;
                // Khi MainWindow mở, chuyển sang chế độ tắt app khi MainWindow đóng
                ShutdownMode = ShutdownMode.OnMainWindowClose;
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }
    }
}
