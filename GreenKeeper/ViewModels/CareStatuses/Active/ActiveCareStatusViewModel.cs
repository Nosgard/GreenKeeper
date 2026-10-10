using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using GreenKeeper.ViewModels.CareStatuses.Abstract;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.CareStatuses.Active
{
    /// <summary>
    /// A status card with a due date (Watering / Fertilizing). Everything that
    /// depends on "today" judges by the calendar day - see DueDateRules - so the
    /// text, the Complete button and the plant's status dot always agree.
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

        // Fires the completion of the care;
        // once it's done, the new due date is recalculated from NOW.
        public ICommand? CompleteCommand { get; protected set; }

        /// <summary>
        /// Whether the Complete button can be used: from the due day onwards. Judging
        /// by the day keeps the button in line with the "Due today" text; a card must
        /// not read "Due today" while the button waits for the exact due time.
        /// </summary>
        public bool IsCompletable => _schedule?.NextDueAt is DateTime dueAt && DueDateRules.IsDue(dueAt, Today);

        /// <summary>
        /// A due date that is still ahead reads as "Due in 3 days" rather than just
        /// "3 days", and the due day itself as "Due today", so the card uses the same
        /// form every time. "Overdue for ..." already says it.
        ///
        /// The wording lives here and not in DueDateCalculator: the calculator also feeds
        /// the preview of the edit dialog, which puts "New due date in:" in front of the
        /// text. A prefix there would read "New due date in: Due in 4 days".
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

        // Deliberately false when no due date is set at all, so an empty status text
        // never turns into a bare "Due in".
        private bool IsUpcoming => _schedule?.NextDueAt is DateTime dueAt && DueDateRules.IsUpcoming(dueAt, Today);

        private bool IsDueToday => _schedule?.NextDueAt is DateTime dueAt && DueDateRules.IsDueToday(dueAt, Today);

        // Drives the red status text. Strict ("before today"), so a card reading "Due today"
        // is not painted red - the plant's status dot only turns red once a day was missed.
        public override bool IsOverdue => _schedule?.NextDueAt is DateTime dueAt && DueDateRules.IsOverdue(dueAt, Today);

        // The Complete button becomes usable on the due day, so it follows the day too.
        public override void NotifyDueStatusChanged()
        {
            base.NotifyDueStatusChanged();
            OnPropertyChanged(nameof(IsCompletable));
        }

        private DateTime Today => _timeProvider.GetLocalNow().DateTime;
    }
}
