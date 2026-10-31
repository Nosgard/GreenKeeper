using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using GreenKeeper.ViewModels.Base;
using System.Globalization;

namespace GreenKeeper.ViewModels.CareStatuses.EditOption
{
    /// <summary>
    /// Input of the Edit button for the sunlight card. Unlike the wizard step it cannot be
    /// skipped: the user edits an existing value, so a valid amount is required. It needs no
    /// PreviewText, since Sunlight has no due date.
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
