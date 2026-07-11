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
        // Value to compensate the time span between the calculation of NextDueAt in the Wizard
        // and the actual presentation in the Status-Card. This is needed to show for example one exact week
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

            double totalHours = (nextDueAt.Value - DateTime.Now).TotalHours;
            bool isOverdue = totalHours < 0;
            double absHours = Math.Abs(totalHours);

            double thresholdCheck = absHours + TOLERANCEHOURS;

            TimeUnit effectiveUnit = thresholdCheck switch
            {
                < 24 => TimeUnit.Hours,
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
