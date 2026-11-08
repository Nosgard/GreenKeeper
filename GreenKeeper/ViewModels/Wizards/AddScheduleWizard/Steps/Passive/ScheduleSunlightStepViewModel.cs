using GreenKeeper.Scheduling;
using GreenKeeper.ViewModels.Base;

namespace GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps.Passive
{
    /// <summary>
    /// The sunlight step of the Add Schedule wizard. Unlike the one in the Add Plant wizard
    /// it is mandatory and ends with "Finish", because the user chose sunlight on purpose.
    /// </summary>
    public class ScheduleSunlightStepViewModel : HoursPerPeriodInputViewModel, IScheduleWizardStep
    {
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
