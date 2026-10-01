using GreenKeeper.ViewModels.Base;
using System.Windows;

namespace GreenKeeper.Views
{
    /// <summary>
    /// Base of every modal window: binds its ViewModel and closes with the
    /// result the ViewModel asks for, so the code-behinds only create their
    /// ViewModel and expose its result.
    /// </summary>
    public class DialogWindow : Window
    {
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
