namespace GreenKeeper.Services
{
    public interface IDialogService
    {
        /// <summary>
        /// Asks a question answered with Yes or No; true for Yes.
        /// </summary>
        /// <param name="message">The question</param>
        /// <param name="title">The title of the window that asks</param>
        /// <param name="safeAnswer">The answer that loses nothing. It is offered first, so
        /// pressing Enter right away neither deletes nor discards anything</param>
        bool Confirm(string message, string title, Answer safeAnswer);

        /// <summary>
        /// Tells the user about an error; the message is only acknowledged with OK.
        /// Work that uses the database starts in an async void method of App or
        /// MainWindow, or in a method an AsyncRelayCommand runs. That method awaits a
        /// private helper. Only the helper catches a failed database call and calls
        /// ShowError, so the helper's class writes the message and decides what happens
        /// next, for example whether the app closes. While the main window is open, an
        /// exception that no helper catches reaches App.ReportUnexpectedError, which
        /// writes its own message and decides whether the app keeps running.
        /// </summary>
        /// <param name="message">What went wrong</param>
        /// <param name="title">The title of the window that tells it</param>
        void ShowError(string message, string title);
    }
}
