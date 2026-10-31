using GreenKeeper.ViewModels.Wizards.Base.Abstract;

namespace GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps.Active
{
    public class WateringStepViewModel : ActiveStepViewModel
    {
        public override bool CanProceed => HasValidAmount;
    }
}
