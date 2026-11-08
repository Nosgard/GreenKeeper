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
        /// A failed database call is caught only by private helpers such as TryPersistAsync,
        /// which call this, so their class writes the message and decides what happens next.
        /// App.ReportUnexpectedError calls it too, with its own text, for a UI-thread exception
        /// that no helper caught.
        /// </summary>
        /// <param name="message">What went wrong</param>
        /// <param name="title">The title of the window that tells it</param>
        void ShowError(string message, string title);
    }
}
