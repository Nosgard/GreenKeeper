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
        /// by the day keeps the button consistent with the "Today" text - a card must
        /// not read "Today" while the button stays disabled until the exact due time.
        /// </summary>
        public bool IsCompletable => _schedule?.NextDueAt is DateTime dueAt && DueDateRules.IsDue(dueAt, Today);

        /// <summary>
        /// A due date that is still ahead reads as "Due in 3 days" rather than just
        /// "3 days", so the card states what the number actually means. "Today" and
        /// "Overdue for ..." already say that themselves and stay as they are.
        ///
        /// The prefix sits here and not in DueDateCalculator on purpose: the calculator
        /// also feeds the edit dialog's preview, which puts its own "New due date:"
        /// in front of the text - prefixing there would read "New due date: Due in 4 days".
        /// </summary>
        public override string StatusText
        {
            get
            {
                var dueDateText = DueDateCalculator.ToDueDateText(_schedule?.NextDueAt, Today);

                return IsUpcoming ? $"Due in {dueDateText}" : dueDateText;
            }
        }

        // Deliberately false when no due date is set at all, so an empty status text
        // never turns into a bare "Due in".
        private bool IsUpcoming => _schedule?.NextDueAt is DateTime dueAt && DueDateRules.IsUpcoming(dueAt, Today);

        // Drives the red status text. Strict ("before today"), so a card reading "Today"
        // is not painted red - the plant's status dot only turns red once a day was missed.
        public override bool IsOverdue => _schedule?.NextDueAt is DateTime dueAt && DueDateRules.IsOverdue(dueAt, Today);

        private DateTime Today => _timeProvider.GetLocalNow().DateTime;
    }
}
