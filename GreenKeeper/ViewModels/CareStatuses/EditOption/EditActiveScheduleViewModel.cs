using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using GreenKeeper.ViewModels.Base;
using System.Globalization;

namespace GreenKeeper.ViewModels.CareStatuses.EditOption
{
    /// <summary>
    /// Input of the Edit button for the active statuses (Watering / Fertilizing).
    /// </summary>
    public class EditActiveScheduleViewModel : AmountAndUnitInputViewModel, IScheduleInputStep
    {
        private readonly TimeProvider _timeProvider;

        public CareType Care { get; }

        public string Title => Care.DisplayName();

        public EditActiveScheduleViewModel(CareType care, int? initialAmount, TimeUnit initialUnit, TimeProvider timeProvider)
        {
            Care = care;
            _timeProvider = timeProvider;

            // Pre-fill with the interval of the existing schedule.
            if (initialAmount.HasValue)
            {
                AmountText = initialAmount.Value.ToString(CultureInfo.InvariantCulture);
            }

            SelectedUnit = initialUnit;
        }

        /// <summary>
        /// The due date the schedule gets when saved now. Saving restarts the interval at this
        /// moment, not at the old due date, and the preview tells the user so.
        /// </summary>
        public string PreviewText
        {
            get
            {
                if (!HasValidAmount)
                {
                    return string.Empty;
                }

                var now = _timeProvider.GetLocalNow().DateTime;
                var nextDueAt = DueDateCalculator.ToDueDate(now, Amount!.Value, SelectedUnit);

                return $"New due date in: {DueDateCalculator.ToDueDateText(nextDueAt, now)}";
            }
        }

        public ScheduleInput CreateInput()
        {
            return new CareScheduleInput(ToCareSchedule(Care));
        }

        protected override void OnAmountOrUnitChanged()
        {
            OnPropertyChanged(nameof(PreviewText));
        }
    }
}
