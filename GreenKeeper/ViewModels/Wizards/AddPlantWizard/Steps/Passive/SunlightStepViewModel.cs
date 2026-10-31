using GreenKeeper.ViewModels.Base;
using GreenKeeper.ViewModels.Wizards.Base;

namespace GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps.Passive
{
    /// <summary>
    /// Optional sunlight step of the Add Plant wizard.
    /// </summary>
    public class SunlightStepViewModel : HoursPerPeriodInputViewModel, IWizardStepViewModel
    {
        // Same rule as the fertilizing step (see FertilizingStepViewModel).
        public bool CanProceed => IsLeftEmpty || HasValidAmount;

        public string NextButtonLabel => IsLeftEmpty ? "Skip" : "Next";

        private bool IsLeftEmpty => AmountText.Length == 0;

        protected override void OnHoursOrPeriodChanged()
        {
            OnPropertyChanged(nameof(CanProceed));
            OnPropertyChanged(nameof(NextButtonLabel));
        }
    }
}
