using GreenKeeper.ViewModels.Wizards.Base.Abstract;

namespace GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps.Active
{
    public class FertilizingStepViewModel : ActiveStepViewModel
    {
        // Optional: it can be skipped while empty. Anything entered has to be valid, because
        // skipping would silently drop it.
        public override bool CanProceed => IsLeftEmpty || HasValidAmount;

        public override string NextButtonLabel => IsLeftEmpty ? "Skip" : "Next";

        private bool IsLeftEmpty => AmountText.Length == 0;
    }
}
