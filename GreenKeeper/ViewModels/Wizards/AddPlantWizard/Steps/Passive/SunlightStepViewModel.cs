using GreenKeeper.ViewModels.Base;
using GreenKeeper.ViewModels.Wizards.Base;

namespace GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps.Passive
{
    /// <summary>
    /// Optional sunlight step of the Add Plant wizard: the user either enters
    /// a positive amount of hours per period or leaves it empty and skips.
    /// </summary>
    public class SunlightStepViewModel : HoursPerPeriodInputViewModel, IWizardStepViewModel
    {
        // The step is optional, so the button is always active; only its label changes.
        public bool CanProceed => true;

        // Depending on the entered amount of hours per period, show "Next" or "Skip".
        public string NextButtonLabel => HasValidAmount ? "Next" : "Skip";

        protected override void OnHoursOrPeriodChanged()
        {
            OnPropertyChanged(nameof(NextButtonLabel));
        }
    }
}
