using GreenKeeper.Services;

namespace GreenKeeper.Tests.Fakes
{
    public class FakeDialogService : IDialogService
    {
        public bool ConfirmResult { get; set; } = true;
        public bool ConfirmWasCalled { get; private set; }
        public bool ShowErrorWasCalled { get; private set; }

        public bool Confirm(string message, string title)
        {
            ConfirmWasCalled = true;
            return ConfirmResult;
        }

        public void ShowError(string message, string title)
        {
            ShowErrorWasCalled = true;
        }
    }
}
