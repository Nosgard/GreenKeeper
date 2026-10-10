using GreenKeeper.Views;

namespace GreenKeeper.Tests.Views
{
    /// <summary>
    /// When a window holds its focus ring back: after it comes back to the front
    /// without the keyboard having been used in it, and only until a key is pressed.
    /// </summary>
    public class FocusCueStateTests
    {
        [Fact]
        public void WindowActivated_GivenTheMouseWasUsedLast_HidesTheRing()
        {
            // Given: a window in which a button was clicked, for instance one that opens a dialog
            var state = new FocusCueState(openedWithKeyboard: true);
            state.MousePressed();

            // When: the window comes back to the front, as it does once the dialog closes
            state.WindowActivated();

            // Then: the ring is held back, although the dialog was confirmed with Enter
            Assert.True(state.IsHidden);
        }

        [Fact]
        public void WindowActivated_GivenTheKeyboardWasUsedLast_KeepsTheRing()
        {
            // Given: a window in which the keyboard was used after a click: Tab reached the
            // button that opens a dialog
            var state = new FocusCueState(openedWithKeyboard: false);
            state.MousePressed();
            state.KeyPressed();

            // When: the window comes back to the front
            state.WindowActivated();

            // Then: the ring shows where the keyboard user left off
            Assert.False(state.IsHidden);
        }

        [Theory]
        [InlineData(true, false)]    // opened with a key: the ring shows from the start
        [InlineData(false, true)]    // opened with a click: the ring is held back
        public void WindowActivated_GivenAWindowThatWasJustOpened_FollowsHowItWasOpened(bool openedWithKeyboard, bool expectedHidden)
        {
            // Given: a new window in which nothing was pressed yet
            var state = new FocusCueState(openedWithKeyboard);

            // When: the window comes to the front for the first time
            state.WindowActivated();

            // Then: the ring is held back only if the window was not opened with a key
            Assert.Equal(expectedHidden, state.IsHidden);
        }

        [Fact]
        public void KeyPressed_GivenAHiddenRing_ShowsItAgain()
        {
            // Given: a window whose ring is held back
            var state = new FocusCueState(openedWithKeyboard: false);
            state.WindowActivated();

            // When: any key is pressed in the window
            state.KeyPressed();

            // Then: the ring shows at once
            Assert.False(state.IsHidden);
        }
    }
}
