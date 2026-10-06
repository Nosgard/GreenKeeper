namespace GreenKeeper.Services
{
    public interface IDialogService
    {
        /// <summary>
        /// Asks a question answered with Yes or No; true for Yes.
        /// </summary>
        /// <param name="message">The question</param>
        /// <param name="title">The title of the window that asks</param>
        /// <param name="safeAnswer">The answer that loses nothing. It is offered first, so a
        /// hasty key neither deletes nor discards anything</param>
        bool Confirm(string message, string title, Answer safeAnswer);

        /// <summary>
        /// Tells the user about an error; the message is only acknowledged with OK.
        /// </summary>
        /// <param name="message">What went wrong</param>
        /// <param name="title">The title of the window that tells it</param>
        void ShowError(string message, string title);
    }
}
