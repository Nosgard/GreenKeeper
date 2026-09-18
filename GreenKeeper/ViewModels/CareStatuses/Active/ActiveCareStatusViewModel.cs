using GreenKeeper.Converters;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.CareStatuses.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.CareStatuses.Active
{
    public class ActiveCareStatusViewModel : CareStatusViewModel
    {
        private readonly CareSchedule? _schedule;

        protected ActiveCareStatusViewModel(CareType care, CareSchedule? schedule, string title, string iconSource, string iconBackgroundHex)
            : base(care, title, iconSource, iconBackgroundHex)
        {
            _schedule = schedule;
        }

        // 
        /// <summary>
        /// Checks if the Complete-Button can be used or not,
        /// depending on whether the next due date is today, overdue or in the future.
        /// 
        /// Important!
        /// Compares calendar dates, NOT exact timestamps. This keeps the button consistent with
        /// the "Today" text shown in the status text. Without this alignment, a Status-Card could
        /// display "Today" while the Complete-Button stays disabled until the exact due time was reached,
        /// which felt inconsistent, since "Today" already signals to the user that completion is expected
        /// on this day
        /// </summary>
        public bool IsCompletable => _schedule?.NextDueAt != null && _schedule.NextDueAt.Value.Date <= DateTime.Now.Date;

        // Fires the Completion of the care,
        // once it's done, recalculate the new due date for NOW
        public ICommand? CompleteCommand { get; protected set; }

        // The whole logic for the conversion of the time units is being controlled by the TimeUnitConverter
        /// <summary>
        /// A due date that is still ahead reads as "Due in 3 days" rather than just
        /// "3 days", so the card states what the number actually means. "Today" and
        /// "Overdue for ..." already say that themselves and stay as they are.
        ///
        /// The prefix sits here and not in TimeUnitConverter on purpose: the converter
        /// also feeds the edit dialog's preview, which puts its own "New due date:"
        /// in front of the text - prefixing there would read "New due date: Due in 4 days"
        /// </summary>
        public override string StatusText
        {
            get
            {
                var dueDateText = TimeUnitConverter.ToDueDateText(_schedule?.NextDueAt);

                return IsUpcoming ? $"Due in {dueDateText}" : dueDateText;
            }
        }

        /// <summary>
        /// True while the due date has not been reached yet - the exact counterpart
        /// of the "today or earlier" that IsCompletable covers, and deliberately
        /// false when no due date is set at all, so an empty status text never
        /// turns into a bare "Due in".
        ///
        /// Compares calendar dates for the same reason IsCompletable does.
        /// </summary>
        private bool IsUpcoming =>
            _schedule?.NextDueAt != null && _schedule.NextDueAt.Value.Date > DateTime.Now.Date;

        /// <summary>
        /// True once the due date has passed. Deliberately strict ("before today",
        /// not "today or earlier") so the text agrees with the plant's status dot,
        /// which only turns red once a date is actually missed - a card reading
        /// "Today" in red would contradict the green-to-yellow-to-red ladder.
        ///
        /// Compares calendar dates for the same reason IsCompletable does.
        /// </summary>
        public override bool IsOverdue =>
            _schedule?.NextDueAt != null && _schedule.NextDueAt.Value.Date < DateTime.Now.Date;
    }
}
