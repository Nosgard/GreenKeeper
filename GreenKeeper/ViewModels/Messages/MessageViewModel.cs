using GreenKeeper.Commands;
using GreenKeeper.ViewModels.Base;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.Messages
{
    /// <summary>
    /// Backs the message window: a question answered with Yes or No, or a notice
    /// acknowledged with OK. Like the other dialog ViewModels it only signals the
    /// answer; what follows is up to the caller.
    /// </summary>
    public class MessageViewModel : IDialogViewModel
    {
        public MessageViewModel(string message, bool isQuestion)
        {
            Message = message;
            IsQuestion = isQuestion;

            ConfirmCommand = new RelayCommand(_ => RequestClose?.Invoke(this, true));
            DeclineCommand = new RelayCommand(_ => RequestClose?.Invoke(this, false));
        }

        public string Message { get; }

        // A question offers Yes and No, a notice only OK.
        public bool IsQuestion { get; }

        public string ConfirmLabel => IsQuestion ? "Yes" : "OK";

        public ICommand ConfirmCommand { get; }
        public ICommand DeclineCommand { get; }

        public event EventHandler<bool>? RequestClose;
    }
}
