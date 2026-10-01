using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using GreenKeeper.ViewModels.Wizards.Base.Abstract;

namespace GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps.Active
{
    /// <summary>
    /// Is used for both Watering and Fertilizing, if the user
    /// chose one of these two statuses.
    /// Unlike in the AddPlantWizard, entering a value is always mandatory
    /// because the user selected the status on purpose.
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
