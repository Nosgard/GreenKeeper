using System.ComponentModel;

namespace GreenKeeper.ViewModels.Wizards.Base
{
    public interface IWizardStepViewModel : INotifyPropertyChanged
    {
        // Whether the Next button can be clicked on this step.
        bool CanProceed { get; }

        // The text of the Next button: "Skip" on an empty optional step, "Finish" on the
        // last step, otherwise "Next".
        string NextButtonLabel { get; }
    }
}
