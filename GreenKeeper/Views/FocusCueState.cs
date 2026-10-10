namespace GreenKeeper.Views
{
    /// <summary>
    /// Whether one window holds its focus ring back. WPF decides on the ring by the
    /// device used last anywhere in the app, so a dialog opened with a click and
    /// confirmed with Enter leaves a ring on the clicked button, in a window the
    /// keyboard was never used in. This keeps the answer per window: the ring is held
    /// back while the mouse was the last device used in the window, and shows again
    /// with the first key pressed in it.
    /// </summary>
    public sealed class FocusCueState
    {
        private bool _keyboardWasUsedLast;

        public FocusCueState(bool openedWithKeyboard)
        {
            _keyboardWasUsedLast = openedWithKeyboard;
        }

        public bool IsHidden { get; private set; }

        public void KeyPressed()
        {
            _keyboardWasUsedLast = true;
            IsHidden = false;
        }

        // Does not hide the ring by itself: after a click WPF draws none anyway.
        public void MousePressed()
        {
            _keyboardWasUsedLast = false;
        }

        // The moment the focus comes back from another window, with a ring that the keys
        // pressed over there would cause.
        public void WindowActivated()
        {
            if (!_keyboardWasUsedLast)
            {
                IsHidden = true;
            }
        }
    }
}
