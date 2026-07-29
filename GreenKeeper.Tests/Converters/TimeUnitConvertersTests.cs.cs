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
        public void ToDueDateText_GivenDueDateThreeDaysInFuture_ReturnsThreeDays()
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

        [Fact]
        public void ToDueDateText_GivenNullDueDates_ReturnsEmptyString()
        {
            // Given: no due date is set (null)
            DateTime? nextDueAt = null;

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the result should be an empty string, not an exception or "null"
            Assert.Equal(string.Empty, result);
        }

        [Theory]
        [InlineData(1, "1 day")]
        [InlineData(3, "3 days")]
        [InlineData(7, "1 week")]
        [InlineData(14, "2 weeks")]
        public void ToDueDateText_GivenUpcomingDueDateInDaysOrWeeks_UseCorrectSingularOrPluralUnit(int daysFromNow, string expected)
        {
            // Given: a due date daysFromNow days in the future
            var nextDueAt = DateTime.Now.AddDays(daysFromNow);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the unit label should be singular for an amount of 1, plural otherwise
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToDueDateText_GivenDueDateOneMonthInFuture_ReturnsOneMonthSingular()
        {
            // Given: a due date exactly one calendar month in the future
            var nextDueAt = DateTime.Now.AddMonths(1);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the result should use the singular form "1 month"
            Assert.Equal("1 month", result);
        }

        [Fact]
        public void ToDueDateText_GivenDueDateTwoMonthsInFuture_ReturnsTwoMonthsPlural()
        {
            // Given: a due date exactly two calendar months in the future
            var nextDueAt = DateTime.Now.AddMonths(2);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the result should use the plural form "2 months"
            Assert.Equal("2 months", result);
        }

        [Fact]
        public void ToDueDateText_GivenDueDateOneYearInFuture_ReturnsOneYearSingular()
        {
            // Given: a due date exactly one calendar year in the future
            var nextDueAt = DateTime.Now.AddYears(1);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the result should use the singular form "1 year"
            Assert.Equal("1 year", result);
        }

        [Fact]
        public void ToDueDateText_GivenDueDateTwoYearsInFuture_ReturnsTwoYearsPlural()
        {
            // Given: a due date exactly two calendar years in the future
            var nextDueAt = DateTime.Now.AddYears(2);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the result should use the plural form "2 years"
            Assert.Equal("2 years", result);
        }
    }
}
