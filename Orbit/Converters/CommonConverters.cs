using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Orbit.Converters
{
    /// <summary>
    /// Converts a boolean (e.g., IsTracked) to a color brush (Gold for tracked, Slate for untracked).
    /// </summary>
    public class TrackedToBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush TrackedBrush = new(Color.FromRgb(241, 196, 15)); // Gold / Amber
        private static readonly SolidColorBrush UntrackedBrush = new(Color.FromArgb(120, 150, 160, 175)); // Muted gray

        static TrackedToBrushConverter()
        {
            TrackedBrush.Freeze();
            UntrackedBrush.Freeze();
        }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is true ? TrackedBrush : UntrackedBrush;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Converts null/non-null object references to Visibility.
    /// By default: non-null -> Visible, null -> Collapsed.
    /// With parameter="invert" or Invert=true: null -> Visible, non-null -> Collapsed.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool invert = Invert || (parameter is string p && string.Equals(p, "invert", StringComparison.OrdinalIgnoreCase));
            bool isNull = value == null;
            bool isVisible = invert ? isNull : !isNull;
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Inverts a boolean value to Visibility (true -> Collapsed, false -> Visible).
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is true ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
