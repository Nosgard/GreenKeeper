using GreenKeeper.Layout;
using GreenKeeper.ViewModels.Base;
using System.Windows;
using System.Windows.Controls;

namespace GreenKeeper.Views
{
    /// <summary>
    /// Base of every modal window. It opens at the scale of its owner, binds its
    /// ViewModel and closes with the result the ViewModel asks for, so a code-behind
    /// only creates its ViewModel and exposes the result. A dialog with a fixed size
    /// grows in height where its content needs the room, and the cursor waits in the
    /// first field.
    /// </summary>
    public class DialogWindow : Window
    {
        public DialogWindow()
        {
            Loaded += (_, _) =>
            {
                GrowWithContent();
                TryFocusFirstInput();
            };
        }

        /// <summary>
        /// Shows the window as a modal dialog of the owner; true if it was confirmed. The
        /// XAML describes the dialog at scale 1, like the main window; it opens at the
        /// scale its owner is drawn at.
        /// </summary>
        public bool ShowDialog(Window owner)
        {
            Owner = owner;
            ScaleTo(UiScale.GetFactor(owner));

            return ShowDialog() == true;
        }

        // Windows draws the title bar and the frame at their own size, so only the
        // area inside them is scaled.
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

        // What Windows adds around the content of a window: the frame with the
        // title bar, plus the border it reserves for resizing.
        private static Size FrameSize()
        {
            Thickness frame = SystemParameters.WindowNonClientFrameThickness;
            Thickness border = SystemParameters.WindowResizeBorderThickness;

            return new Size(
                frame.Left + frame.Right + border.Left + border.Right,
                frame.Top + frame.Bottom + border.Top + border.Bottom);
        }

        // The designed height stays the minimum; the dialog gets higher only where its
        // content needs more room, such as a long plant name that wraps. Switched on once
        // the dialog is loaded: a window sized to its content from the start is laid out
        // before its bindings deliver their values.
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

        // Setting DialogResult closes the window. Overridable for windows that
        // intercept Closing and have to let this close through (see NotesView).
        protected virtual void CloseWithResult(bool dialogResult)
        {
            DialogResult = dialogResult;
        }
    }
}
