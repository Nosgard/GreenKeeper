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

        // Title of the care type (Watering / Fertilizing).
        public string Title => Care.DisplayName();

        public EditActiveScheduleViewModel(CareType care, int? initialAmount, TimeUnit initialUnit, TimeProvider timeProvider)
        {
            Care = care;
            _timeProvider = timeProvider;

            // Fill the amount text with the original value that was set in the wizard beforehand.
            if (initialAmount.HasValue)
            {
                AmountText = initialAmount.Value.ToString(CultureInfo.InvariantCulture);
            }

            SelectedUnit = initialUnit;
        }

        /// <summary>
        /// Shows the next expected due date.
        /// This is important so that the user knows that the countdown is NOW running.
        /// The calculation of the next due date takes place NOW and not at the old due date.
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

                return $"New due date: {DueDateCalculator.ToDueDateText(nextDueAt, now)}";
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
