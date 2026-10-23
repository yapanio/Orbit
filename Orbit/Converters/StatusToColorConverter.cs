using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Orbit.Converters
{
    /// <summary>
    /// Converts a Windows Service status string into a representative SolidColorBrush.
    /// Running: Emerald Green (#2ECC71)
    /// Stopped: Crimson Red (#E74C3C)
    /// Starting / Stopping: Amber Orange (#F39C12)
    /// Paused: Amethyst Purple (#9B59B6)
    /// Default / Unknown: Slate Gray (#95A5A6)
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        private static readonly SolidColorBrush RunningBrush = new(Color.FromRgb(46, 204, 113));
        private static readonly SolidColorBrush StoppedBrush = new(Color.FromRgb(231, 76, 60));
        private static readonly SolidColorBrush PendingBrush = new(Color.FromRgb(243, 156, 18));
        private static readonly SolidColorBrush PausedBrush = new(Color.FromRgb(155, 89, 182));
        private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(149, 165, 166));

        static StatusToColorConverter()
        {
            RunningBrush.Freeze();
            StoppedBrush.Freeze();
            PendingBrush.Freeze();
            PausedBrush.Freeze();
            DefaultBrush.Freeze();
        }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string status || string.IsNullOrWhiteSpace(status))
            {
                return DefaultBrush;
            }

            if (status.EndsWith("...", StringComparison.Ordinal) ||
                status.Contains("Starting", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("Stopping", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("Pending", StringComparison.OrdinalIgnoreCase))
            {
                return PendingBrush;
            }

            if (string.Equals(status, "Running", StringComparison.OrdinalIgnoreCase))
            {
                return RunningBrush;
            }

            if (string.Equals(status, "Stopped", StringComparison.OrdinalIgnoreCase))
            {
                return StoppedBrush;
            }

            if (string.Equals(status, "Paused", StringComparison.OrdinalIgnoreCase))
            {
                return PausedBrush;
            }

            return DefaultBrush;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Converts a status string to a subtle background brush for badges.
    /// </summary>
    public class StatusToBadgeBackgroundConverter : IValueConverter
    {
        private static readonly SolidColorBrush RunningBg = new(Color.FromArgb(40, 46, 204, 113));
        private static readonly SolidColorBrush StoppedBg = new(Color.FromArgb(40, 231, 76, 60));
        private static readonly SolidColorBrush PendingBg = new(Color.FromArgb(40, 243, 156, 18));
        private static readonly SolidColorBrush PausedBg = new(Color.FromArgb(40, 155, 89, 182));
        private static readonly SolidColorBrush DefaultBg = new(Color.FromArgb(30, 149, 165, 166));

        static StatusToBadgeBackgroundConverter()
        {
            RunningBg.Freeze();
            StoppedBg.Freeze();
            PendingBg.Freeze();
            PausedBg.Freeze();
            DefaultBg.Freeze();
        }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string status || string.IsNullOrWhiteSpace(status))
            {
                return DefaultBg;
            }

            if (status.EndsWith("...", StringComparison.Ordinal) ||
                status.Contains("Starting", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("Stopping", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("Pending", StringComparison.OrdinalIgnoreCase))
            {
                return PendingBg;
            }

            if (string.Equals(status, "Running", StringComparison.OrdinalIgnoreCase))
            {
                return RunningBg;
            }

            if (string.Equals(status, "Stopped", StringComparison.OrdinalIgnoreCase))
            {
                return StoppedBg;
            }

            if (string.Equals(status, "Paused", StringComparison.OrdinalIgnoreCase))
            {
                return PausedBg;
            }

            return DefaultBg;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
