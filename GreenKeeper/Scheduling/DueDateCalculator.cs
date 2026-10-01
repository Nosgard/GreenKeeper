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

        // How a single calendar step is taken. Passing these around keeps months
        // and years on one shared implementation instead of two that drift apart.
        private static readonly Func<DateTime, int, DateTime> MonthStep = (date, count) => date.AddMonths(count);
        private static readonly Func<DateTime, int, DateTime> YearStep = (date, count) => date.AddYears(count);

        /// <summary>
        /// For debugging purposes only.
        /// Meant for the debugging tool, where a flat TimeSpan is subtracted from
        /// existing dates rather than added onto a fixed start date.
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
        /// Calculates a concrete due date from a start date,
        /// an amount and a unit - calendar-exact for Months/Years
        /// and exact by definition for Days/Weeks.
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
        /// The text of a status card: "Today", "3 days", "Overdue for 2 weeks".
        ///
        /// The "today" it measures against is passed in rather than read from the
        /// clock: every result depends on the calendar position of the current day -
        /// whether a month has 28, 30 or 31 days decides which unit a given number
        /// of days belongs to - so the callers hand in their clock and the tests a
        /// pinned date.
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

            // Each branch is entered only once its own unit has fully elapsed, so
            // the amount is always at least 1 and needs no further guarding.
            string unitLabel = UnitLabels[unit] + (amount == 1 ? "" : "s");

            return isOverdue
                ? $"Overdue for {amount} {unitLabel}"
                : $"{amount} {unitLabel}";
        }

        /// <summary>
        /// Picks the largest unit that has COMPLETELY elapsed between the two dates
        /// and counts it. Nothing is rounded, so the text never claims more time
        /// than has actually passed: "2 weeks" begins on day 14 and not on day 11,
        /// and "2 months" on the second full calendar month and not halfway into it.
        ///
        /// Unit and amount come from the same full count - the branch is chosen
        /// by it and the amount IS it - so the two can never disagree. That also
        /// rules out "12 months" by construction: months are only reported below
        /// twelve, so the amount can never reach it.
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
                // A week is always exactly seven days, so plain integer division
                // truncates here just as the calendar counts do below.
                return (daysDiff / DaysPerWeek, TimeUnit.Weeks);
            }

            if (fullMonths < MonthsPerYear)
            {
                return (fullMonths, TimeUnit.Months);
            }

            return (FullCalendarYearsBetween(earlier, later), TimeUnit.Years);
        }

        // -- Calculation of time differences for calendar months and years --

        /// <summary>
        /// Counts the number of FULL calendar units between two dates (floor, not rounded).
        /// For example: Jan 15 to Mar 10 is 1 full month (Jan 15 to Feb 15), not 2, since
        /// Mar 10 hasn't reached Feb 15 + 1 month yet.
        ///
        /// The count is verified by actually stepping the date forward, never by
        /// comparing day-of-month numbers. Those numbers lie whenever a step lands
        /// on a shorter month and gets clamped: Jan 31 + 1 month is Feb 28, so Feb
        /// 28 IS a full month after Jan 31 - a day comparison (28 &lt; 31) would
        /// count it as zero months and the card would read "4 weeks".
        ///
        /// The caller passes the difference of the month or year numbers. That
        /// estimate is never too small and at most one step too large (when the
        /// later day of the month has not been reached yet), so the loop only
        /// ever has to step back.
        /// </summary>
        private static int FullCalendarUnitsBetween(
            DateTime earlier, DateTime later, int estimate, Func<DateTime, int, DateTime> step)
        {
            int units = Math.Max(estimate, 0);

            while (units > 0 && step(earlier, units) > later)
            {
                units--;
            }

            return units;
        }

        private static int FullCalendarMonthsBetween(DateTime earlier, DateTime later)
        {
            int estimate = ((later.Year - earlier.Year) * MonthsPerYear) + (later.Month - earlier.Month);

            return FullCalendarUnitsBetween(earlier, later, estimate, MonthStep);
        }

        private static int FullCalendarYearsBetween(DateTime earlier, DateTime later)
        {
            return FullCalendarUnitsBetween(earlier, later, later.Year - earlier.Year, YearStep);
        }
    }
}
