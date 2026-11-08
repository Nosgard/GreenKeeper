using GreenKeeper.Layout;
using GreenKeeper.ViewModels.Base;
using System.Windows;
using System.Windows.Controls;

namespace GreenKeeper.Views
{
    /// <summary>
    /// Base of every modal window. It opens at the scale of its owner, binds its
    /// ViewModel and closes with the result the ViewModel asks for, so most code-behinds
    /// only create their ViewModel and expose the result.
    /// </summary>
    public class DialogWindow : Window
    {
        public DialogWindow()
        {
            TitleBarTheme.Attach(this);
            FocusCue.Attach(this);
            ArrowNavigation.Attach(this);

            Loaded += (_, _) =>
            {
                GrowWithContent();
                TryFocusFirstInput();
            };
        }

        /// <summary>
        /// Shows the window as a modal dialog of the owner, at the owner's scale; true if it was
        /// confirmed. The XAML describes the dialog at scale 1, like the main window.
        /// </summary>
        public bool ShowDialog(Window owner)
        {
            Owner = owner;
            ScaleTo(UiScale.GetFactor(owner));

            return ShowDialog() == true;
        }

        // The title bar and the frame keep their size, so only the area inside them is
        // scaled.
        private void ScaleTo(double factor)
        {
            Size frame = FrameSize();

            Width = Scale(Width, frame.Width);
            Height = Scale(Height, frame.Height);
            MinWidth = Scale(MinWidth, frame.Width);
            MinHeight = Scale(MinHeight, frame.Height);

            UiScale.SetFactor(this, factor);

            // A length of 0 is a minimum that was never set and stays that way.
            double Scale(double length, double frameLength)
            {
                return length == 0 ? 0 : frameLength + ((length - frameLength) * factor);
            }
        }

        // Every window with a title bar has this frame, even a dialog that cannot be resized.
        // The resize border counts twice in it: once in WindowNonClientFrameThickness with
        // the title bar, and once more on its own, because the frame on screen is about one
        // resize border wider than WindowNonClientFrameThickness alone.
        private static Size FrameSize()
        {
            Thickness frame = SystemParameters.WindowNonClientFrameThickness;
            Thickness border = SystemParameters.WindowResizeBorderThickness;

            return new Size(
                frame.Left + frame.Right + border.Left + border.Right,
                frame.Top + frame.Bottom + border.Top + border.Bottom);
        }

        // The designed height stays the minimum; content such as a long plant name that wraps can
        // make the dialog higher. It runs on Loaded, when ActualHeight still holds the designed
        // height, so MinHeight keeps that height once SizeToContent lets the dialog grow.
        private void GrowWithContent()
        {
            if (ResizeMode != ResizeMode.NoResize)
            {
                return;
            }

            MinHeight = ActualHeight;
            SizeToContent = SizeToContent.Height;
        }

        // The first field of the dialog gets the focus, with the cursor after its text;
        // false if the dialog has no field. Then the focus stays where it is, so Enter
        // still reaches the default button.
        protected bool TryFocusFirstInput()
        {
            var input = VisualTree.Descendants<Control>(this)
                .FirstOrDefault(control => control is TextBox or ComboBox && control.IsVisible && control.IsEnabled);

            if (input == null)
            {
                return false;
            }

            input.Focus();

            if (input is TextBox box)
            {
                box.CaretIndex = box.Text.Length;
            }

            return true;
        }

        protected void Attach(IDialogViewModel viewModel)
        {
            DataContext = viewModel;
            viewModel.RequestClose += ViewModel_RequestClose;
        }

        // A window closes once, so the subscription ends with the request.
        private void ViewModel_RequestClose(object? sender, bool dialogResult)
        {
            ((IDialogViewModel)sender!).RequestClose -= ViewModel_RequestClose;
            CloseWithResult(dialogResult);
        }

        // Overridable for windows that intercept Closing and have to let this close through
        // (see NotesView).
        protected virtual void CloseWithResult(bool dialogResult)
        {
            DialogResult = dialogResult;
        }
    }
}
