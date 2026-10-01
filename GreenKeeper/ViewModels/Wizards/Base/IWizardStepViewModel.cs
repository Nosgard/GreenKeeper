using System.ComponentModel;

namespace GreenKeeper.ViewModels.Wizards.Base
{
    public interface IWizardStepViewModel : INotifyPropertyChanged
    {
        // Controls whether the "Next" button is active on every page.
        bool CanProceed { get; }

        // Controls the text of the "Next" button.
        // Usually "Next", in case of optional and empty steps "Skip".
        string NextButtonLabel { get; }
    }
}
