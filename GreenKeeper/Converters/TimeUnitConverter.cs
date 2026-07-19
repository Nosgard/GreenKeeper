using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Converters
{
    public class TimeUnitConverter
    {
        /// <summary>
        /// Value to compensate the time span between the calculation of NextDueAt
        /// and the actual presentation in the Status-Card. This is needed when values are just below a threshold.
        /// Only affects the upcoming branch from below
        /// </summary>
        private const double TOLERANCEHOURS = 1.0 / 60.0;

        /// <summary>
        /// Debug-only purposes
        /// Meant for the Debugging-Tool, where a flat TimeSpan is subtracted from
        /// existing dates rather than added onto a fixed start date
        /// </summary>
        public static TimeSpan ToTimeSpan(int amount, TimeUnit unit)
        {
            double hours = unit switch
            {
                TimeUnit.Hours => amount,
                TimeUnit.Days => amount * 24,
                TimeUnit.Weeks => amount * 24 * 7,
                _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
            };

            return TimeSpan.FromHours(hours);
        }

        /// <summary>
        /// The opposite direction of calculating a time span.
        /// This is used to get a "readable" amount of time.
        /// Uses the same values (30/365 Days) as ToTimeSpan,
        /// so that calculating forward and backwards remains
        /// </summary>
        private static readonly Dictionary<TimeUnit, int> HoursPerUnit = new()
        {
            { TimeUnit.Hours, 1 },
            { TimeUnit.Days, 24 },
            { TimeUnit.Weeks, 24 * 7 },
            { TimeUnit.Months, 24 * 30 },
            { TimeUnit.Years, 24 * 365 },
        };

        private static readonly Dictionary<TimeUnit, string> UnitLabels = new()
        {
            { TimeUnit.Hours, "hour" },
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
                TimeUnit.Hours => start.AddHours(amount),
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

            var due = nextDueAt.Value;
            var now = DateTime.Now;

            // The due date "Today" will be determined by the Calendar-Date to prevent a drift
            if (due.Date == now.Date)
            {
                return "Today";
            }

            bool isOverdue = due.Date < now.Date;

            double absHours;
            bool isHoursUnitAllowed;

            // Overdue will be calculated in days.
            // Calculating in hours would cause a Borderline-Case (e.g Due of Watering is today but can be done throughout the day)
            if (isOverdue)
            {
                int daysOverdue = (now.Date - due.Date).Days;
                absHours = daysOverdue * 24;
                isHoursUnitAllowed = false;
            }
            else
            {
                absHours = (due - now).TotalHours;
                isHoursUnitAllowed = true;
            }

            double thresholdCheck = absHours + TOLERANCEHOURS;

            TimeUnit effectiveUnit = thresholdCheck switch
            {
                < 24 => isHoursUnitAllowed ? TimeUnit.Hours : TimeUnit.Days,
                < 24 * 7 => TimeUnit.Days,
                < 24 * 30 => TimeUnit.Weeks,
                < 24 * 365 => TimeUnit.Months,
                _ => TimeUnit.Years
            };

            // Calculate the overdue date if present
            int amount;

            if (isOverdue && effectiveUnit == TimeUnit.Months)
            {
                amount = Math.Max(RoundedCalendarMonthsOverdue(due.Date, now.Date), 1);
            }
            else if (isOverdue && effectiveUnit == TimeUnit.Years)
            {
                amount = Math.Max(RoundedCalendarYearsOverdue(due.Date, now.Date), 1);
            }
            else
            {
                double rawAmount = absHours / HoursPerUnit[effectiveUnit];
                amount = (int)Math.Round(rawAmount, MidpointRounding.AwayFromZero);
            }

            // For safety reasons. Shouldn't be possible with the check of the date above
            if (amount == 0)
            {
                return "Today";
            }

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
        private static int RoundedCalendarMonthsOverdue(DateTime due, DateTime now)
        {
            int lowerMonths = CalendarMonthsBetween(due, now);
            DateTime lowerBound = due.AddMonths(lowerMonths);
            DateTime upperBound = due.AddMonths(lowerMonths + 1);
            DateTime midpoint = lowerBound.AddTicks((upperBound - lowerBound).Ticks / 2);

            return now >= midpoint ? lowerMonths + 1 : lowerMonths;
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
        private static int RoundedCalendarYearsOverdue(DateTime due, DateTime now)
        {
            int lowerYears = CalendarYearsBetween(due, now);
            DateTime lowerBound = due.AddYears(lowerYears);
            DateTime upperBound = due.AddYears(lowerYears + 1);
            DateTime midpoint = lowerBound.AddTicks((upperBound - lowerBound).Ticks / 2);

            return now >= midpoint ? lowerYears + 1 : lowerYears;
        }
    }
}
