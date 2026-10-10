using GreenKeeper.Layout;
using System.Windows;

namespace GreenKeeper.Tests.Layout
{
    /// <summary>
    /// The size the main window opens at: the design size on the screen it is meant
    /// for, larger on a larger screen, never larger than the work area.
    /// </summary>
    public class StartSizeTests
    {
        // The size MainWindow.xaml is designed at, on the screen it is designed for.
        private static readonly Size DesignSize = new(900, 650);
        private static readonly Size ReferenceScreen = new(1920, 1080);

        [Theory]
        [InlineData(1920, 1032)]   // the reference screen with a taskbar
        [InlineData(1366, 728)]    // a smaller screen that still has room for the design size
        public void For_GivenScreenUpToTheReferenceScreen_OpensAtTheDesignSize(double workAreaWidth, double workAreaHeight)
        {
            // Given: the work area of the screen
            var workArea = new Size(workAreaWidth, workAreaHeight);

            // When: the start size is calculated
            var startSize = StartSize.For(DesignSize, ReferenceScreen, workArea);

            // Then: the window opens exactly as designed
            Assert.Equal(DesignSize, startSize);
        }

        [Theory]
        [InlineData(3840, 2160, 1800, 1300)]   // twice the reference screen in both directions
        [InlineData(3840, 1080, 900, 650)]     // only wider: no more height than the reference screen
        public void For_GivenLargerScreen_GrowsByTheTighterSide(
            double workAreaWidth, double workAreaHeight, double expectedWidth, double expectedHeight)
        {
            // Given: the work area of the screen
            var workArea = new Size(workAreaWidth, workAreaHeight);

            // When: the start size is calculated
            var startSize = StartSize.For(DesignSize, ReferenceScreen, workArea);

            // Then: the window grows as far as both sides of the screen allow
            Assert.Equal(new Size(expectedWidth, expectedHeight), startSize);
        }

        [Theory]
        [InlineData(1200, 574, 900, 574)]   // wide enough, but too low for the design size
        [InlineData(800, 728, 800, 650)]    // tall enough, but too narrow for the design size
        public void For_GivenScreenTooSmallOnOneSide_StaysInsideTheWorkArea(
            double workAreaWidth, double workAreaHeight, double expectedWidth, double expectedHeight)
        {
            // Given: the work area of the screen
            var workArea = new Size(workAreaWidth, workAreaHeight);

            // When: the start size is calculated
            var startSize = StartSize.For(DesignSize, ReferenceScreen, workArea);

            // Then: only the side that does not fit is cut down to the work area
            Assert.Equal(new Size(expectedWidth, expectedHeight), startSize);
        }
    }
}
