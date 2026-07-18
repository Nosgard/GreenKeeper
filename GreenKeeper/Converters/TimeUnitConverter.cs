using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Calculates the inputs in the Wizard in a TimeSpan.
        /// This is needed, because CareSchedule doesn't save an interval
        /// but a due date (Property -> DueDate). Therefore the calculation
        /// happens singularly
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static TimeSpan ToTimeSpan(int amount, TimeUnit unit)
        {
            double hours = unit switch
            { 
                TimeUnit.Hours => amount,
                TimeUnit.Days => amount * 24,
                TimeUnit.Weeks => amount * 24 * 7,
                TimeUnit.Months => amount * 24 * 30,
                TimeUnit.Years => amount * 24 * 365,
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

            double rawAmount = absHours / HoursPerUnit[effectiveUnit];


                // Given time is overdue -> Ceil the amount of time to make sure delays won't be underestimated
                // Given time is due -> Round the amount of time to the nearest whole number
                int amount = isOverdue
                    ? (int)Math.Ceiling(rawAmount)
                    : (int)Math.Round(rawAmount, MidpointRounding.AwayFromZero);

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
    }
}
