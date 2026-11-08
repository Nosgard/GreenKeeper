using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using GreenKeeper.ViewModels.Wizards.Base.Abstract;

namespace GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps.Active
{
    /// <summary>
    /// The watering or fertilizing step of the Add Schedule wizard. A value is always required,
    /// fertilizing included, because the user chose the care type on purpose.
    /// </summary>
    public class ScheduleActiveStepViewModel : ActiveStepViewModel, IScheduleWizardStep
    {
        public CareType Care { get; }

        public string Title => Care.DisplayName();

        public ScheduleActiveStepViewModel(CareType care)
        {
            Care = care;
        }

        public override bool CanProceed => HasValidAmount;

        public override string NextButtonLabel => "Finish";

        public ScheduleInput CreateInput()
        {
            return new CareScheduleInput(ToCareSchedule(Care));
        }
    }
}
