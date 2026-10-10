using GreenKeeper.Views.Messages;
using System.Windows;

namespace GreenKeeper.Services
{
    /// <summary>
    /// Shows questions and errors in a window of the app, so they follow its theme and
    /// scale like every other dialog. A Windows message box does neither.
    /// </summary>
    public class MessageDialogService : IDialogService
    {
        public bool Confirm(string message, string title, Answer safeAnswer)
        {
            return Show(message, title, isQuestion: true, firstAnswer: safeAnswer);
        }

        // A notice has only its OK button, which stands where "Yes" stands.
        public void ShowError(string message, string title)
        {
            Show(message, title, isQuestion: false, firstAnswer: Answer.Yes);
        }

        private static bool Show(string message, string title, bool isQuestion, Answer firstAnswer)
        {
            var messageView = new MessageView(message, title, isQuestion, firstAnswer);

            // The message belongs to what is in front: the main window, or the dialog that asks.
            var owner = Application.Current.Windows.OfType<Window>().LastOrDefault(window => window.IsVisible);

            if (owner != null)
            {
                return messageView.ShowDialog(owner);
            }

            // Nothing is open yet when the database cannot be prepared at start.
            messageView.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            return messageView.ShowDialog() == true;
        }
    }
}
