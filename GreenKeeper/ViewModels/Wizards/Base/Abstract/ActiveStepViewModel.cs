using GreenKeeper.ViewModels.Base;

namespace GreenKeeper.ViewModels.Wizards.Base.Abstract
{
    public abstract class ActiveStepViewModel : AmountAndUnitInputViewModel, IWizardStepViewModel
    {
        // Each step decides: watering requires a value in both wizards, fertilizing only in the
        // Add Schedule wizard.
        public abstract bool CanProceed { get; }

        public virtual string NextButtonLabel => "Next";

        protected override void OnAmountOrUnitChanged()
        {
            OnPropertyChanged(nameof(CanProceed));
            OnPropertyChanged(nameof(NextButtonLabel));
        }
    }
}
