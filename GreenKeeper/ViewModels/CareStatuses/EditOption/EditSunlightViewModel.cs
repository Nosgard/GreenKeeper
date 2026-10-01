using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using GreenKeeper.ViewModels.Base;
using System.Globalization;

namespace GreenKeeper.ViewModels.CareStatuses.EditOption
{
    /// <summary>
    /// Input of the Edit button for the sunlight status. Unlike the wizard step
    /// there is no option to skip: the user is actively editing an existing
    /// value, so a valid amount is always required. No PreviewText needed,
    /// since Sunlight has no due date.
    /// </summary>
    public class EditSunlightViewModel : HoursPerPeriodInputViewModel, IScheduleInputStep
    {
        // Pre-fill the fields with the current sunlight requirement.
        public EditSunlightViewModel(int? initialHours, SunlightPeriod initialPeriod)
        {
            if (initialHours.HasValue)
            {
                AmountText = initialHours.Value.ToString(CultureInfo.InvariantCulture);
            }
            SelectedPeriod = initialPeriod;
        }

        public ScheduleInput CreateInput()
        {
            return new SunlightRequirementInput(ToSunlightRequirement());
        }
    }
}
