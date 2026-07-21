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
            if (nextDueAt == null)
            {
                return string.Empty;
            }

            var due = nextDueAt.Value.Date;
            var today = DateTime.Now.Date;

            // The due date "Today" will be determined by the Calendar-Date to prevent a drift
            if (due == today)
            {
                return "Today";
            }

            bool isOverdue = due < today;

            DateTime earlier = isOverdue ? due : today;
            DateTime later = isOverdue ? today : due;

            int daysDiff = (later - earlier).Days;

            TimeUnit effectiveUnit = daysDiff switch
            {
                < 7 => TimeUnit.Days,
                < 30 => TimeUnit.Weeks,
                < 365 => TimeUnit.Months,
                _ => TimeUnit.Years
            };

            int amount = effectiveUnit switch
            {
                TimeUnit.Days => daysDiff,
                TimeUnit.Weeks => Math.Max((int)Math.Round(daysDiff / 7.0, MidpointRounding.AwayFromZero), 1),
                TimeUnit.Months => Math.Max(RoundedCalendarMonthsBetween(earlier, later), 1),
                TimeUnit.Years => Math.Max(RoundedCalendarYearsBetween(earlier, later), 1),
                _ => daysDiff
            };

            string unitLabel = UnitLabels[effectiveUnit] + (amount == 1 ? "" : "s");

            return isOverdue
                ? $"Overdue for {amount} {unitLabel}"
                : $"{amount} {unitLabel}";
        }

        // -- Calculation of time differences for calendar months/-years --

        /// <summary>
        /// Counts the number of FULL calendar months between two dates (floor, not rounded).
        /// For example: Jan 15 to Mar 10 is 1 full month (Jan 15 to Feb 15), not 2, since
        /// Mar 10 hasn't reached Feb 15 + 1 month yet
        /// </summary>
        private static int CalendarMonthsBetween(DateTime from, DateTime to)
        {
            int months = ((to.Year - from.Year) * 12) + (to.Month - from.Month);
            if (to.Day < from.Day)
            {
                months--;
            }
            return Math.Max(months, 0);
        }

        /// <summary>
        /// Rounds the elapsed time between "due" and "now" to the NEAREST full calendar month,
        /// respecting the actual (variable) length of each individual month.
        /// 
        /// Finds the lower full-month boundary (due.AddMonths(n)) and the next
        /// one (due.AddMonths(n+1)), calculates the exact midpoint between them,
        /// and rounds up or down depending on which side "now" falls on. This way,
        /// for example "just before 2 months" correctly rounds to 2 months even if the specific
        /// months involved are shorter or longer than 30 days
        /// </summary>
        private static int RoundedCalendarMonthsBetween(DateTime earlier, DateTime later)
        {
            int lowerMonths = CalendarMonthsBetween(earlier, later);
            DateTime lowerBound = earlier.AddMonths(lowerMonths);
            DateTime upperBound = earlier.AddMonths(lowerMonths + 1);
            DateTime midpoint = lowerBound.AddTicks((upperBound - lowerBound).Ticks / 2);

            return later >= midpoint ? lowerMonths + 1 : lowerMonths;
        }

        // Counts full calendar years between two dates (floor) analogous to
        // CalendarMonthsBetween, just for years
        private static int CalendarYearsBetween(DateTime from, DateTime to)
        {
            int years = to.Year - from.Year;
            if (to.Month < from.Month || (to.Month == from.Month && to.Day < from.Day))
            {
                years--;
            }
            return Math.Max(years, 0);
        }

        // Rounds to the nearest full calendar year, analogous to
        // RoundedCalendarMonthsOverdue - accounts for leap years automatically thanks to AddYears/DateTime
        private static int RoundedCalendarYearsBetween(DateTime earlier, DateTime later)
        {
            int lowerYears = CalendarYearsBetween(earlier, later);
            DateTime lowerBound = earlier.AddYears(lowerYears);
            DateTime upperBound = earlier.AddYears(lowerYears + 1);
            DateTime midpoint = lowerBound.AddTicks((upperBound - lowerBound).Ticks / 2);

            return later >= midpoint ? lowerYears + 1 : lowerYears;
        }
    }
}
