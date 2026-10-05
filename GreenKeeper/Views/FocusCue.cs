using System.Windows;
using System.Windows.Input;

namespace GreenKeeper.Views
{
    /// <summary>
    /// Shows the focus ring in a window only while the keyboard is used in that window
    /// (see FocusCueState for the rule). The rings of FocusStyles.xaml read IsHidden.
    /// </summary>
    public static class FocusCue
    {
        /// <summary>
        /// Set on a window and inherited by everything in it, the focus rings included:
        /// they are drawn in the adorner layer of the window.
        /// </summary>
        public static readonly DependencyProperty IsHiddenProperty = DependencyProperty.RegisterAttached(
            "IsHidden",
            typeof(bool),
            typeof(FocusCue),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

        public static bool GetIsHidden(DependencyObject element)
        {
            return (bool)element.GetValue(IsHiddenProperty);
        }

        public static void SetIsHidden(DependencyObject element, bool isHidden)
        {
            element.SetValue(IsHiddenProperty, isHidden);
        }

        /// <summary>
        /// Lets the window follow its own keyboard and mouse use from now on. The
        /// events of the window hold the state, so it lives as long as the window.
        /// </summary>
        public static void Attach(Window window)
        {
            var state = new FocusCueState(openedWithKeyboard: InputManager.Current.MostRecentInputDevice is KeyboardDevice);

            window.PreviewKeyDown += (_, _) =>
            {
                state.KeyPressed();
                SetIsHidden(window, state.IsHidden);
            };

            window.PreviewMouseDown += (_, _) => state.MousePressed();

            window.Activated += (_, _) =>
            {
                state.WindowActivated();
                SetIsHidden(window, state.IsHidden);
            };
        }
    }
}
