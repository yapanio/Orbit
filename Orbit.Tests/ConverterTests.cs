using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Orbit.Converters;
using Xunit;

namespace Orbit.Tests
{
    public class ConverterTests
    {
        [Theory]
        [InlineData("Running", 46, 204, 113)]      // #2ECC71 Green
        [InlineData("Stopped", 231, 76, 60)]       // #E74C3C Red
        [InlineData("Starting...", 243, 156, 18)]  // #F39C12 Orange
        [InlineData("Stopping...", 243, 156, 18)]  // #F39C12 Orange
        [InlineData("Paused", 155, 89, 182)]       // #9B59B6 Purple
        public void StatusToColorConverter_ReturnsExpectedColors(string status, byte r, byte g, byte b)
        {
            var converter = new StatusToColorConverter();
            var result = converter.Convert(status, typeof(Brush), null, CultureInfo.InvariantCulture);

            Assert.IsType<SolidColorBrush>(result);
            var brush = (SolidColorBrush)result;
            Assert.Equal(r, brush.Color.R);
            Assert.Equal(g, brush.Color.G);
            Assert.Equal(b, brush.Color.B);
        }

        [Fact]
        public void NullToVisibilityConverter_ConvertsCorrectly()
        {
            var converter = new NullToVisibilityConverter();

            // Default: non-null -> Visible, null -> Collapsed
            Assert.Equal(Visibility.Visible, converter.Convert(new object(), typeof(Visibility), null, CultureInfo.InvariantCulture));
            Assert.Equal(Visibility.Collapsed, converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture));

            // With invert parameter: null -> Visible, non-null -> Collapsed
            Assert.Equal(Visibility.Visible, converter.Convert(null, typeof(Visibility), "invert", CultureInfo.InvariantCulture));
            Assert.Equal(Visibility.Collapsed, converter.Convert(new object(), typeof(Visibility), "invert", CultureInfo.InvariantCulture));
        }

        [Fact]
        public void TrackedToBrushConverter_ConvertsTrueAndFalse()
        {
            var converter = new TrackedToBrushConverter();

            var trueBrush = (SolidColorBrush)converter.Convert(true, typeof(Brush), null, CultureInfo.InvariantCulture);
            var falseBrush = (SolidColorBrush)converter.Convert(false, typeof(Brush), null, CultureInfo.InvariantCulture);

            Assert.NotNull(trueBrush);
            Assert.NotNull(falseBrush);
            Assert.NotEqual(trueBrush.Color, falseBrush.Color);
        }
    }
}
