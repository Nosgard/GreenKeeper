using GreenKeeper.Scheduling;
using GreenKeeper.ViewModels.Base;

namespace GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps.Passive
{
    /// <summary>
    /// Passive step just like in the AddPlantWizard, but mandatory
    /// and with an explicit "Finish" on the Next button, because the user
    /// selected the status on purpose.
    /// </summary>
    public class ScheduleSunlightStepViewModel : HoursPerPeriodInputViewModel, IScheduleWizardStep
    {
        // Different from SunlightStepViewModel: Mandatory!
        public bool CanProceed => HasValidAmount;

        public string NextButtonLabel => "Finish";

        public ScheduleInput CreateInput()
        {
            return new SunlightRequirementInput(ToSunlightRequirement());
        }

        protected override void OnHoursOrPeriodChanged()
        {
            OnPropertyChanged(nameof(CanProceed));
        }
    }
}
