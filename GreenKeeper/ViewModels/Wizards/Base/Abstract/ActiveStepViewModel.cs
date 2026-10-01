using GreenKeeper.ViewModels.Base;

namespace GreenKeeper.ViewModels.Wizards.Base.Abstract
{
    public abstract class ActiveStepViewModel : AmountAndUnitInputViewModel, IWizardStepViewModel
    {
        // Mandatory for watering + optional for fertilizing.
        // Will be implemented by the respective class.
        public abstract bool CanProceed { get; }

        // Default: "Next"
        // In the context of fertilizing: "Skip" or "Next"
        public virtual string NextButtonLabel => "Next";

        protected override void OnAmountOrUnitChanged()
        {
            OnPropertyChanged(nameof(CanProceed));
            OnPropertyChanged(nameof(NextButtonLabel));
        }
    }
}
