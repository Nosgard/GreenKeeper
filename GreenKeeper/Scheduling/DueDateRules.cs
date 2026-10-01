namespace GreenKeeper.Scheduling
{
    /// <summary>
    /// The one place that decides what "overdue", "due today" and "upcoming" mean.
    /// The status cards, the plant's status dot and the due date text all follow
    /// these rules, so they can never disagree with each other.
    ///
    /// All of them compare calendar dates, not timestamps: a schedule due later
    /// on the current day is due today, and it is only overdue once the day has
    /// actually been missed.
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

        // Due today or overdue: the care can be completed.
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
