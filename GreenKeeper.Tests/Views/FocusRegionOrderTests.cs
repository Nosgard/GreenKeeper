using GreenKeeper.Views;

namespace GreenKeeper.Tests.Views
{
    /// <summary>
    /// Which region F6 moves the focus to: the next one, with Shift the previous one,
    /// around the end and past regions with nothing to focus. Nowhere when no other
    /// region can take it.
    /// </summary>
    public class FocusRegionOrderTests
    {
        // The main window: the sidebar is region 0, the dashboard region 1.
        private const int Sidebar = 0, Dashboard = 1;

        [Fact]
        public void Next_GivenTheFocusInTheSidebar_MovesToTheDashboard()
        {
            // Given: the focus in the sidebar, and something to focus in both regions
            var canTakeFocus = new[] { true, true };

            // When: F6 is pressed
            var next = FocusRegionOrder.Next(Sidebar, canTakeFocus, backwards: false);

            // Then: the focus goes on to the dashboard
            Assert.Equal(Dashboard, next);
        }

        [Fact]
        public void Next_GivenTheFocusInTheLastRegion_StartsOverAtTheFirst()
        {
            // Given: the focus in the dashboard, the last region
            var canTakeFocus = new[] { true, true };

            // When: F6 is pressed
            var next = FocusRegionOrder.Next(Dashboard, canTakeFocus, backwards: false);

            // Then: the order starts over, so F6 alone reaches every region
            Assert.Equal(Sidebar, next);
        }

        [Fact]
        public void Next_GivenShift_GoesBackwards()
        {
            // Given: three regions (with two, forwards and backwards would end in the same
            // place) and the focus in the first one
            var canTakeFocus = new[] { true, true, true };

            // When: Shift+F6 is pressed
            var next = FocusRegionOrder.Next(0, canTakeFocus, backwards: true);

            // Then: the focus goes back around to the last region, not on to the second
            Assert.Equal(2, next);
        }

        [Fact]
        public void Next_GivenARegionWithNothingToFocus_SkipsIt()
        {
            // Given: three regions, the middle one without anything to focus
            var canTakeFocus = new[] { true, false, true };

            // When: F6 is pressed in the first one
            var next = FocusRegionOrder.Next(0, canTakeFocus, backwards: false);

            // Then: the focus goes past the empty region to the next one that can take it
            Assert.Equal(2, next);
        }

        [Fact]
        public void Next_GivenNoOtherRegionCanTakeTheFocus_StaysWhereItIs()
        {
            // Given: the focus in the sidebar, and a dashboard with nothing to focus because
            // no plant is selected and its buttons are disabled
            var canTakeFocus = new[] { true, false };

            // When: F6 is pressed
            var next = FocusRegionOrder.Next(Sidebar, canTakeFocus, backwards: false);

            // Then: there is no region to go to, so the focus stays where it is
            Assert.Null(next);
        }

        [Theory]
        [InlineData(false, Sidebar)]     // F6 starts at the first region
        [InlineData(true, Dashboard)]    // Shift+F6 starts at the last one
        public void Next_GivenTheFocusInNoRegion_StartsAtTheEndTheKeyPointsTo(bool backwards, int expected)
        {
            // Given: the focus in no region, as right after the start of the app
            var canTakeFocus = new[] { true, true };

            // When: F6 or Shift+F6 is pressed
            var next = FocusRegionOrder.Next(null, canTakeFocus, backwards);

            // Then: the focus goes into the region at the end the key points to
            Assert.Equal(expected, next);
        }
    }
}
