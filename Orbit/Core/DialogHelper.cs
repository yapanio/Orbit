using System;
using System.Windows;

namespace Orbit.Core
{
    /// <summary>
    /// Helper hiển thị thông báo lỗi qua MessageBox một cách ngắn gọn, an toàn trên mọi luồng.
    /// </summary>
    public static class DialogHelper
    {
        public static Action<string, string>? ErrorHandler { get; set; }

        public static void ShowError(string message, string title = "Lỗi")
        {
            if (ErrorHandler != null)
            {
                ErrorHandler(message, title);
                return;
            }

            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }
    }
}
