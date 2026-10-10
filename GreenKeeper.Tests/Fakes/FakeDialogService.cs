using GreenKeeper.Services;

namespace GreenKeeper.Tests.Fakes
{
    public class FakeDialogService : IDialogService
    {
        public bool ConfirmResult { get; set; } = true;
        public bool ConfirmWasCalled { get; private set; }

        // The answer the last question offered first; null while nothing was asked.
        public Answer? OfferedAnswer { get; private set; }

        public bool ShowErrorWasCalled { get; private set; }

        public bool Confirm(string message, string title, Answer safeAnswer)
        {
            ConfirmWasCalled = true;
            OfferedAnswer = safeAnswer;
            return ConfirmResult;
        }

        public void ShowError(string message, string title)
        {
            ShowErrorWasCalled = true;
        }
    }
}
