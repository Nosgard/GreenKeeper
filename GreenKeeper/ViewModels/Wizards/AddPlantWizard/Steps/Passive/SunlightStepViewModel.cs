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
        // The step is optional: empty, it can be skipped. Once something is entered it has
        // to be valid, because skipping would drop the entry without a word.
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
