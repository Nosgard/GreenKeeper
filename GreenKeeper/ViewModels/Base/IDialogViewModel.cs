namespace GreenKeeper.ViewModels.Base
{
    /// <summary>
    /// A ViewModel behind a modal window. It never closes the window itself but
    /// asks for it through the event: true = confirmed (saved, finished),
    /// false = canceled (discarded). DialogWindow turns that into the DialogResult.
    /// </summary>
    public interface IDialogViewModel
    {
        event EventHandler<bool>? RequestClose;
    }
}
