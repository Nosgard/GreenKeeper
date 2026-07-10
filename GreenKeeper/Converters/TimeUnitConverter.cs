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
    }
}
