using Orbit.Core;
using Xunit;

namespace Orbit.Tests
{
    public class DialogHelperTests
    {
        [Fact]
        public void ShowError_CallsCustomErrorHandler()
        {
            string? capturedMessage = null;
            string? capturedTitle = null;

            DialogHelper.ErrorHandler = (msg, title) =>
            {
                capturedMessage = msg;
                capturedTitle = title;
            };

            try
            {
                DialogHelper.ShowError("Có lỗi xảy ra", "Tiêu đề lỗi");

                Assert.Equal("Có lỗi xảy ra", capturedMessage);
                Assert.Equal("Tiêu đề lỗi", capturedTitle);
            }
            finally
            {
                DialogHelper.ErrorHandler = null;
            }
        }

        [Fact]
        public void ShowError_DoesNotThrowWhenNoApplicationCurrent()
        {
            DialogHelper.ErrorHandler = null;

            // In test environment Application.Current is null; must not throw
            var exception = Record.Exception(() =>
            {
                DialogHelper.ShowError("Test message", "Test title");
            });

            Assert.Null(exception);
        }
    }
}
