using GreenKeeper.Models.Enums;

namespace GreenKeeper.Converters
{
    public class TimeUnitConverter
    {

        /// <summary>
        /// Debug-only purposes
        /// Meant for the Debugging-Tool, where a flat TimeSpan is subtracted from
        /// existing dates rather than added onto a fixed start date
        /// </summary>
        public static TimeSpan ToTimeSpan(int amount, TimeUnit unit)
        {
            double hours = unit switch
            {
                TimeUnit.Days => amount * 24,
                TimeUnit.Weeks => amount * 24 * 7,
                _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
            };

            return TimeSpan.FromHours(hours);
        }

        private static readonly Dictionary<TimeUnit, string> UnitLabels = new()
        {
            { TimeUnit.Days, "day" },
            { TimeUnit.Weeks, "week" },
            { TimeUnit.Months, "month" },
            { TimeUnit.Years, "year" },

        };

        /// <summary>
        /// Calculates a concrete due date from a start date,
        /// an amount and a unit - calendar-exact for Months/Years
        /// and exact by definition for Hours/Days/Weeks
        /// </summary>
        public static DateTime ToDueDate(DateTime start, int amount, TimeUnit unit)
        {
            return unit switch
            {
                TimeUnit.Days => start.AddDays(amount),
                TimeUnit.Weeks => start.AddDays(amount * 7),
                TimeUnit.Months => start.AddMonths(amount),
                TimeUnit.Years => start.AddYears(amount),
                _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
            };
        }

        public static string ToDueDateText(DateTime? nextDueAt)
        {
            return ToDueDateText(nextDueAt, DateTime.Now);
        }

        /// <summary>
        /// The actual calculation, with the "today" it measures against passed in
        /// rather than read from the clock.
        ///
        /// Why this overload exists: every result here depends on the calendar
        /// position of the current day - whether a month has 28, 30 or 31 days
        /// decides which unit a given number of days belongs to. Bound to
        /// DateTime.Now, entire classes of calendar edge cases (month ends, leap
        /// days) are only reachable on a handful of days per year and therefore
        /// cannot be covered by a deterministic test. It stays internal: the
        /// public surface of the class is unchanged, only the test assembly gets
        /// to pin the reference date.
        /// </summary>
        internal static string ToDueDateText(DateTime? nextDueAt, DateTime reference)
        {
            if (nextDueAt == null)
            {
                return string.Empty;
            }

            var due = nextDueAt.Value.Date;
            var today = reference.Date;

            // The due date "Today" will be determined by the Calendar-Date to prevent a drift
            if (due == today)
            {
                return "Today";
            }

            bool isOverdue = due < today;

            DateTime earlier = isOverdue ? due : today;
            DateTime later = isOverdue ? today : due;

            int daysDiff = (later - earlier).Days;
            int fullMonths = FullCalendarMonthsBetween(earlier, later);

            // Nothing is rounded: every unit reports only what has COMPLETELY
            // elapsed, so the text never claims more time than has actually
            // passed. "2 weeks" therefore begins on day 14 and not on day 11, and
            // "2 months" on the second full calendar month and not halfway into it.
            //
            // Unit and amount come from the same full count - the branch is chosen
            // by it and the amount IS it - so the two can no longer disagree. That
            // also rules out "12 months" by construction: this branch is only
            // taken below twelve, so the amount can never reach it.
            TimeUnit effectiveUnit;
            int amount;

            if (daysDiff < 7)
            {
                effectiveUnit = TimeUnit.Days;
                amount = daysDiff;
            }
            else if (fullMonths == 0)
            {
                // A week is always exactly seven days, so plain integer division
                // truncates here just as the calendar counts do below.
                effectiveUnit = TimeUnit.Weeks;
                amount = daysDiff / 7;
            }
            else if (fullMonths < 12)
            {
                effectiveUnit = TimeUnit.Months;
                amount = fullMonths;
            }
            else
            {
                effectiveUnit = TimeUnit.Years;
                amount = FullCalendarYearsBetween(earlier, later);
            }

            // Each branch is entered only once its own unit has fully elapsed, so
            // the amount is always at least 1 and needs no further guarding.
            string unitLabel = UnitLabels[effectiveUnit] + (amount == 1 ? "" : "s");

            return isOverdue
                ? $"Overdue for {amount} {unitLabel}"
                : $"{amount} {unitLabel}";
        }

        // -- Calculation of time differences for calendar months/-years --

        // How a single calendar step is taken. Passing these around keeps months
        // and years on one shared implementation instead of two that drift apart.
        private static readonly Func<DateTime, int, DateTime> MonthStep = (date, count) => date.AddMonths(count);
        private static readonly Func<DateTime, int, DateTime> YearStep = (date, count) => date.AddYears(count);

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
        /// The caller passes a cheap estimate; the loops only correct it by the
        /// one step it can ever be off by.
        /// </summary>
        private static int FullCalendarUnitsBetween(
            DateTime earlier, DateTime later, int estimate, Func<DateTime, int, DateTime> step)
        {
            int units = Math.Max(estimate, 0);

            while (units > 0 && step(earlier, units) > later)
            {
                units--;
            }

            while (step(earlier, units + 1) <= later)
            {
                units++;
            }

            return units;
        }

        // The month-number difference is at most one step away from the real
        // answer, which makes it a safe starting estimate.
        private static int FullCalendarMonthsBetween(DateTime earlier, DateTime later)
        {
            int estimate = ((later.Year - earlier.Year) * 12) + (later.Month - earlier.Month);

            return FullCalendarUnitsBetween(earlier, later, estimate, MonthStep);
        }

        private static int FullCalendarYearsBetween(DateTime earlier, DateTime later)
        {
            return FullCalendarUnitsBetween(earlier, later, later.Year - earlier.Year, YearStep);
        }
    }
}
