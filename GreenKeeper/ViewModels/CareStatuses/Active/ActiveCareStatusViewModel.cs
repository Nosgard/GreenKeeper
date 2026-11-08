using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using GreenKeeper.ViewModels.CareStatuses.Abstract;

namespace GreenKeeper.ViewModels.CareStatuses.Active
{
    /// <summary>
    /// A status card with a due date (Watering, Fertilizing). Everything that depends on
    /// "today" judges by the calendar day (see DueDateRules), so the text, the Complete
    /// button and the plant's status dot agree.
    /// </summary>
    public abstract class ActiveCareStatusViewModel : CareStatusViewModel
    {
        private readonly CareSchedule? _schedule;
        private readonly TimeProvider _timeProvider;

        protected ActiveCareStatusViewModel(CareType care, CareSchedule? schedule, TimeProvider timeProvider)
            : base(care)
        {
            _schedule = schedule;
            _timeProvider = timeProvider;
        }

        // Completes the care schedule; the next due date then counts from this moment.
        public AsyncRelayCommand? CompleteCommand { get; protected set; }

        /// <summary>
        /// Whether the Complete button can be used: from the due day on, in line with the
        /// "Due today" text.
        /// </summary>
        public bool IsCompletable => _schedule?.NextDueAt is DateTime dueAt && DueDateRules.IsDue(dueAt, Today);

        /// <summary>
        /// Adds "Due in" to a date still ahead and reads "Due today" on the due day, so the card
        /// uses the same form every time. "Overdue for ..." already has that form. The wording
        /// lives here and not in DueDateCalculator, because the edit dialog's preview puts its
        /// own "New due date in:" in front of the calculator's text.
        /// </summary>
        public override string StatusText
        {
            get
            {
                if (IsDueToday)
                {
                    return "Due today";
                }

                var dueDateText = DueDateCalculator.ToDueDateText(_schedule?.NextDueAt, Today);

                return IsUpcoming ? $"Due in {dueDateText}" : dueDateText;
            }
        }

        // False without a due date, so an empty status text never turns into a bare "Due in".
        private bool IsUpcoming => _schedule?.NextDueAt is DateTime dueAt && DueDateRules.IsUpcoming(dueAt, Today);

        private bool IsDueToday => _schedule?.NextDueAt is DateTime dueAt && DueDateRules.IsDueToday(dueAt, Today);

        // Drives the red status text. Strict ("before today"), so a card reading "Due today"
        // is not painted red.
        public override bool IsOverdue => _schedule?.NextDueAt is DateTime dueAt && DueDateRules.IsOverdue(dueAt, Today);

        // IsCompletable depends on the day as well.
        public override void NotifyDueStatusChanged()
        {
            base.NotifyDueStatusChanged();
            OnPropertyChanged(nameof(IsCompletable));
        }

        private DateTime Today => _timeProvider.GetLocalNow().DateTime;
    }
}
