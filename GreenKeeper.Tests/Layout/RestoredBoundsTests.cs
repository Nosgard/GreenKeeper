using GreenKeeper.Layout;
using GreenKeeper.Models;
using System.Windows;

namespace GreenKeeper.Tests.Layout
{
    /// <summary>
    /// Where the main window opens after a restart: at its stored place, as far as
    /// the current screen still has that place.
    /// </summary>
    public class RestoredBoundsTests
    {
        private static readonly Rect Screen = new(0, 0, 1920, 1080);

        [Fact]
        public void For_GivenPlacementOnTheScreen_KeepsIt()
        {
            // Given: a window that was closed in the middle of the screen
            var stored = new WindowPlacement { Left = 300, Top = 200, Width = 1100, Height = 700 };

            // When: the bounds for the next start are calculated
            var bounds = RestoredBounds.For(stored, Screen);

            // Then: the window opens exactly where it was
            Assert.Equal(new Rect(300, 200, 1100, 700), bounds);
        }

        [Theory]
        [InlineData(2500, 300, 820, 300)]   // closed on a second screen to the right that is gone
        [InlineData(-400, 300, 0, 300)]     // closed on a second screen to the left that is gone
        [InlineData(300, 900, 300, 380)]    // lower edge below the screen
        [InlineData(300, -50, 300, 0)]      // title bar above the screen
        public void For_GivenPlacementOutsideTheScreen_MovesTheWindowBackInside(
            double left, double top, double expectedLeft, double expectedTop)
        {
            // Given: a window of 1100 x 700 that was closed outside of today's screen
            var stored = new WindowPlacement { Left = left, Top = top, Width = 1100, Height = 700 };

            // When: the bounds for the next start are calculated
            var bounds = RestoredBounds.For(stored, Screen);

            // Then: the window keeps its size and moves just far enough to be fully visible
            Assert.Equal(new Rect(expectedLeft, expectedTop, 1100, 700), bounds);
        }

        [Fact]
        public void For_GivenWindowLargerThanTheScreen_ShrinksItToTheScreen()
        {
            // Given: a window that was closed on a larger screen
            var stored = new WindowPlacement { Left = 100, Top = 50, Width = 2400, Height = 1300 };

            // When: the bounds for the next start are calculated
            var bounds = RestoredBounds.For(stored, Screen);

            // Then: the window fills the screen instead of reaching beyond it
            Assert.Equal(Screen, bounds);
        }

        [Fact]
        public void For_GivenSizeBelowZero_TreatsItAsNoSize()
        {
            // Given: a settings file that was changed by hand to a size no window can have
            var stored = new WindowPlacement { Left = 300, Top = 200, Width = -1100, Height = -700 };

            // When: the bounds for the next start are calculated
            var bounds = RestoredBounds.For(stored, Screen);

            // Then: the stored size is dropped and the window's own minimum applies
            Assert.Equal(new Rect(300, 200, 0, 0), bounds);
        }
    }
}
