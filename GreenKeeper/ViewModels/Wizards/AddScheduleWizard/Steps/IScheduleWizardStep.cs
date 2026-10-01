using GreenKeeper.ViewModels.Base;
using GreenKeeper.ViewModels.Wizards.Base;

namespace GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps
{
    /// <summary>
    /// The detail step of the Add Schedule wizard: a wizard step that also
    /// produces the schedule or requirement to save.
    /// </summary>
    public interface IScheduleWizardStep : IWizardStepViewModel, IScheduleInputStep
    {
    }
}
