using GreenKeeper.ViewModels.Wizards.Base.Abstract;

namespace GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps.Active
{
    public class FertilizingStepViewModel : ActiveStepViewModel
    {
        // Optional field: empty, it can be skipped. Once something is entered it has to be
        // valid, because skipping would drop the entry without a word.
        public override bool CanProceed => IsLeftEmpty || HasValidAmount;

        public override string NextButtonLabel => IsLeftEmpty ? "Skip" : "Next";

        private bool IsLeftEmpty => AmountText.Length == 0;
    }
}
