using GreenKeeper.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Tests.Converters
{
    public class TimeUnitConvertersTests
    {
        [Fact]
        public void ToDueDateText_GivenDueDate3DaysInFuture_Returns3Days()
        {
            // Given: a due date three days from now
            var nextDueAt = DateTime.Now.AddDays(3);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the result should read "3 days", without an "Overdue" prefix
            Assert.Equal("3 days", result);
        }


        [Fact]
        public void ToDueDateText_GivenDueDateIsToday_ReturnsToday()
        {
            // Given: a due date that falls on the current day
            var nextDueAt = DateTime.Now;

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the result should read "Today"
            Assert.Equal("Today", result);
        }

        /// <summary>
        /// Regression test for historical bug: 35 days overdue used to be
        /// displayed as "Overdue for 2 months" instead of "Overdue for 1 month",
        /// because the rounding logic used Math.Ceiling instead of rounding
        /// to the nearest calendar month. Ensures this specific miscalculation
        /// never silently returns
        /// </summary>
        [Fact]
        public void ToDueDateText_GivenDueDate35DaysOverdue_ReturnsOverdueForMonth()
        {
            // Given: a due 35 days in the past
            var nextDueAt = DateTime.Now.AddDays(-35);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the result should read "Overdue for 1 month"
            Assert.Equal("Overdue for 1 month", result);
        }

        /// <summary>
        /// Regression test for a historical bug: a due date exactly one calendar day
        /// in the past used to be displayed as "Overdue for 0 days" instead of
        /// "Overdue for 1 day". The cause was acomparing the full, time-of-day-inclusive
        /// due date directly against an already Date-truncated "today" value - since
        /// less than 24 full hours had elapsed (due to the leftover time-of-day component),
        /// the day difference was truncated to 0 instead of being calculated on a
        /// pure calendar-day basis.
        /// </summary>
        [Fact]
        public void ToDueDateText_GivenDueDateOnOneDayOverdue_ReturnsOverdueForOneDay()
        {
            // Given: a due date exactly one calendar day in the past
            var nextDueAt = DateTime.Now.AddDays(-1);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the result should read "Overdue for 1 day", not "0 days"
            Assert.Equal("Overdue for 1 day", result);
        }


    }
}
