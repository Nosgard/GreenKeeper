namespace GreenKeeper.Views
{
    /// <summary>
    /// Whether one window holds its focus ring back. Without this, a dialog opened with a
    /// click and confirmed with Enter would leave a ring on the clicked button. One
    /// instance per window decides for that window alone: if the mouse was used last in
    /// it, the ring stays hidden after the window comes back to the front, until a key is
    /// pressed there.
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

        // Does not hide the ring by itself: after a click no ring shows anyway.
        public void MousePressed()
        {
            _keyboardWasUsedLast = false;
        }

        // Called when the window opens and when the focus comes back from another window:
        // keys pressed over there would otherwise show a ring here.
        public void WindowActivated()
        {
            if (!_keyboardWasUsedLast)
            {
                IsHidden = true;
            }
        }
    }
}
