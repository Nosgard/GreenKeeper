namespace GreenKeeper.Scheduling
{
    /// <summary>
    /// Decides what "overdue", "due today" and "upcoming" mean, so the status cards, the
    /// status dot and the due date text agree.
    ///
    /// The rules compare calendar dates, not timestamps: a schedule is overdue only once its
    /// day has passed.
    /// </summary>
    public static class DueDateRules
    {
        public static bool IsOverdue(DateTime dueAt, DateTime today)
        {
            return dueAt.Date < today.Date;
        }

        public static bool IsDueToday(DateTime dueAt, DateTime today)
        {
            return dueAt.Date == today.Date;
        }

        // Due today or overdue: the care schedule can be completed.
        public static bool IsDue(DateTime dueAt, DateTime today)
        {
            return dueAt.Date <= today.Date;
        }

        public static bool IsUpcoming(DateTime dueAt, DateTime today)
        {
            return dueAt.Date > today.Date;
        }
    }
}
