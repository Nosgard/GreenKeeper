using GreenKeeper.ViewModels.Wizards.Base.Abstract;

namespace GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps.Active
{
    public class WateringStepViewModel : ActiveStepViewModel
    {
        // Mandatory field: Next will be active when a valid number is entered.
        public override bool CanProceed => HasValidAmount;
    }
}
