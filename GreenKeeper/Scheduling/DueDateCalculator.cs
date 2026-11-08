using GreenKeeper.Models.Enums;

namespace GreenKeeper.Scheduling
{
    /// <summary>
    /// Calendar arithmetic for care schedules: turns an interval into a due date
    /// and a due date into the text shown on the status cards.
    /// </summary>
    public static class DueDateCalculator
    {
        private const int DaysPerWeek = 7;
        private const int MonthsPerYear = 12;

        private static readonly Dictionary<TimeUnit, string> UnitLabels = new()
        {
            { TimeUnit.Days, "day" },
            { TimeUnit.Weeks, "week" },
            { TimeUnit.Months, "month" },
            { TimeUnit.Years, "year" },
        };

        /// <summary>
        /// Only for the time simulation of the debug panel, which subtracts a flat TimeSpan
        /// from existing dates instead of adding onto a start date.
        /// </summary>
        public static TimeSpan ToTimeSpan(int amount, TimeUnit unit)
        {
            return unit switch
            {
                TimeUnit.Days => TimeSpan.FromDays(amount),
                TimeUnit.Weeks => TimeSpan.FromDays(amount * DaysPerWeek),
                _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
            };
        }

        /// <summary>
        /// The due date an amount of units after the start: calendar steps for months and years,
        /// fixed lengths for days and weeks.
        /// </summary>
        public static DateTime ToDueDate(DateTime start, int amount, TimeUnit unit)
        {
            return unit switch
            {
                TimeUnit.Days => start.AddDays(amount),
                TimeUnit.Weeks => start.AddDays(amount * DaysPerWeek),
                TimeUnit.Months => start.AddMonths(amount),
                TimeUnit.Years => start.AddYears(amount),
                _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
            };
        }

        /// <summary>
        /// The due date as text: "Today", "3 days", "Overdue for 2 weeks". The status cards show
        /// the first two as "Due today" and "Due in 3 days".
        ///
        /// The caller passes today's date, so the tests can pin it.
        /// </summary>
        public static string ToDueDateText(DateTime? nextDueAt, DateTime reference)
        {
            if (nextDueAt == null)
            {
                return string.Empty;
            }

            var due = nextDueAt.Value.Date;
            var today = reference.Date;

            if (DueDateRules.IsDueToday(due, today))
            {
                return "Today";
            }

            bool isOverdue = DueDateRules.IsOverdue(due, today);

            DateTime earlier = isOverdue ? due : today;
            DateTime later = isOverdue ? today : due;

            var (amount, unit) = ElapsedUnitsBetween(earlier, later);

            // The amount is never 0 here: that case already returned "Today".
            string unitLabel = UnitLabels[unit] + (amount == 1 ? "" : "s");

            return isOverdue
                ? $"Overdue for {amount} {unitLabel}"
                : $"{amount} {unitLabel}";
        }

        /// <summary>
        /// Picks the largest unit that has fully elapsed between the two dates and counts it.
        /// Nothing is rounded up, so the text never claims more time than has passed.
        /// </summary>
        private static (int Amount, TimeUnit Unit) ElapsedUnitsBetween(DateTime earlier, DateTime later)
        {
            int daysDiff = (later - earlier).Days;
            int fullMonths = FullCalendarMonthsBetween(earlier, later);

            if (daysDiff < DaysPerWeek)
            {
                return (daysDiff, TimeUnit.Days);
            }

            if (fullMonths == 0)
            {
                return (daysDiff / DaysPerWeek, TimeUnit.Weeks);
            }

            if (fullMonths < MonthsPerYear)
            {
                return (fullMonths, TimeUnit.Months);
            }

            // The full years are the full months divided by twelve. That matches ToDueDate:
            // from Feb 29, a year ends on Feb 28 of the next year in both.
            return (fullMonths / MonthsPerYear, TimeUnit.Years);
        }

        // -- Calculation of full calendar months --

        /// <summary>
        /// Counts the full calendar months between two dates, floored: Jan 15 to Mar 10 is one
        /// month. The loop starts at the month number difference and only steps back: that
        /// number is never too small and at most one too large.
        ///
        /// The loop adds whole months to the earlier date, so in 2025 Jan 31 to Feb 28 counts
        /// as a full month.
        /// Comparing the days of the month, 28 against 31, would count zero months, and the card
        /// would read "4 weeks".
        /// </summary>
        private static int FullCalendarMonthsBetween(DateTime earlier, DateTime later)
        {
            int monthNumberDifference = ((later.Year - earlier.Year) * MonthsPerYear) + (later.Month - earlier.Month);

            // At least zero, in case the two dates come in the wrong order.
            int months = Math.Max(monthNumberDifference, 0);

            while (months > 0 && earlier.AddMonths(months) > later)
            {
                months--;
            }

            return months;
        }
    }
}
