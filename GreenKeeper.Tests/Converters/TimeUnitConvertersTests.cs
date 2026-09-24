using GreenKeeper.Converters;
using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Tests.Converters
{
    public class TimeUnitConvertersTests
    {

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
        /// Regression test for a historical bug: a due date exactly one calendar day
        /// in the past used to be displayed as "Overdue for 0 days" instead of
        /// "Overdue for 1 day". The cause was a comparing the full, time-of-day-inclusive
        /// due date directly against an already Date-truncated "today" value - since
        /// less than 24 full hours had elapsed (due to the leftover time-of-day component),
        /// the day difference was truncated to 0 instead of being calculated on a
        /// pure calendar-day basis.
        /// </summary>
        [Fact]
        public void ToDueDateText_GivenDueDateOneDayOverdue_ReturnsOverdueForOneDay()
        {
            // Given: a due date exactly one calendar day in the past
            var nextDueAt = DateTime.Now.AddDays(-1);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the result should read "Overdue for 1 day", not "0 days"
            Assert.Equal("Overdue for 1 day", result);
        }

        /// <summary>
        /// Regression test: 35 days overdue used to be rounded up to "2 months".
        /// Only whole calendar units count, so everything after the first full
        /// month is dropped.
        /// </summary>
        [Fact]
        public void ToDueDateText_GivenDueDate35DaysOverdue_ReturnsOverdueForOneMonth()
        {
            // Given: a due 35 days in the past
            var nextDueAt = DateTime.Now.AddDays(-35);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the result should read "Overdue for 1 month"
            Assert.Equal("Overdue for 1 month", result);
        }

        [Fact]
        public void ToDueDateText_GivenDueDateTwoMonthsOverdue_ReturnsOverdueForTwoMonths()
        {
            // Given: a due date exactly two calendar months in the past
            var nextDueAt = DateTime.Now.AddMonths(-2);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the result should use the plural form "Overdue for 2 months"
            Assert.Equal("Overdue for 2 months", result);
        }

        [Fact]
        public void ToDueDateText_GivenDueDateOneYearOverdue_ReturnsOverdueForOneYear()
        {
            // Given: a due date exactly one calendar year in the past
            var nextDueAt = DateTime.Now.AddYears(-1);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the result should read "Overdue for 1 year"
            Assert.Equal("Overdue for 1 year", result);
        }

        [Fact]
        public void ToDueDateText_GivenDueDateTwoYearsOverdue_ReturnsOverdueForTwoYears()
        {
            // Given: a due date exactly two calendar years in the past
            var nextDueAt = DateTime.Now.AddYears(-2);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the result should use the plural form "Overdue for 2 years"
            Assert.Equal("Overdue for 2 years", result);
        }

        [Theory]
        [InlineData(2, "Overdue for 2 days")]
        [InlineData(7, "Overdue for 1 week")]
        [InlineData(14, "Overdue for 2 weeks")]
        public void ToDueDateText_GivenOverdueDueDateInDaysOrWeeks_UsesCorrectSingularOrPluralUnit(int daysOverdue, string expected)
        {
            // Given: a due date daysOverdue days in the past
            var nextDueAt = DateTime.Now.AddDays(-daysOverdue);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the unit label should be singular for an amount of 1, plural otherwise
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(1, "1 day")]
        [InlineData(3, "3 days")]
        [InlineData(7, "1 week")]
        [InlineData(14, "2 weeks")]
        public void ToDueDateText_GivenUpcomingDueDateInDaysOrWeeks_UsesCorrectSingularOrPluralUnit(int daysFromNow, string expected)
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

        /// <summary>
        /// Regression test for a structural bug: the number of full calendar months
        /// was derived by comparing the two day-of-month numbers. That comparison is
        /// wrong whenever a month step gets clamped to a shorter month - Jan 31 plus
        /// one month is Feb 28, so Feb 28 IS one full month after Jan 31, but
        /// 28 &lt; 31 made the code count zero months. The span then fell through to
        /// the week branch and the card read "4 weeks" instead of "1 month".
        ///
        /// The dates are pinned rather than derived from DateTime.Now: the bug only
        /// surfaces when one end of the span sits on a day the other month does not
        /// have, which on a "today"-relative test would be reachable on a handful of
        /// days per year and silently pass on every other day.
        /// </summary>
        [Theory]
        [InlineData("2025-01-31", "2025-02-28")] // 28 days, Jan 31 + 1 month clamps to Feb 28
        [InlineData("2024-01-31", "2024-02-29")] // 29 days, same in a leap year
        [InlineData("2025-03-31", "2025-04-30")] // 30 days, 31-day month into a 30-day month
        [InlineData("2025-08-31", "2025-09-30")] // 30 days
        public void ToDueDateText_GivenUpcomingSpanEndingOnAClampedMonthEnd_ReturnsOneMonthNotFourWeeks(
            string today, string due)
        {
            // Given: a due date exactly one calendar month ahead, where that month
            // ends earlier than the current day-of-month
            var reference = DateTime.Parse(today, CultureInfo.InvariantCulture);
            var nextDueAt = DateTime.Parse(due, CultureInfo.InvariantCulture);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt, reference);

            // Then: the span counts as a full month, not as four weeks
            Assert.Equal("1 month", result);
        }

        /// <summary>
        /// The overdue direction of the same clamping bug - here the due date is the
        /// month end and "today" is the clamped target.
        /// </summary>
        [Theory]
        [InlineData("2024-02-29", "2024-01-31")] // 29 days overdue
        [InlineData("2024-02-29", "2024-01-30")] // 30 days overdue
        public void ToDueDateText_GivenOverdueSpanStartingOnAClampedMonthEnd_ReturnsOneMonthNotFourWeeks(
            string today, string due)
        {
            // Given: a due date exactly one calendar month in the past, starting on a
            // day-of-month that the following month does not have
            var reference = DateTime.Parse(today, CultureInfo.InvariantCulture);
            var nextDueAt = DateTime.Parse(due, CultureInfo.InvariantCulture);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt, reference);

            // Then: the span counts as a full month, not as four weeks
            Assert.Equal("Overdue for 1 month", result);
        }

        /// <summary>
        /// The same clamping mistake existed in the year calculation: Feb 29 plus one
        /// year is Feb 28, which the day-number comparison counted as zero full years.
        /// </summary>
        [Fact]
        public void ToDueDateText_GivenSpanFromALeapDayToTheClampedAnniversary_ReturnsOneYear()
        {
            // Given: a due date on a leap day and a reference exactly one year later,
            // which AddYears clamps to Feb 28
            var reference = new DateTime(2025, 2, 28);
            var nextDueAt = new DateTime(2024, 2, 29);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt, reference);

            // Then: the anniversary counts as a full year
            Assert.Equal("Overdue for 1 year", result);
        }

        /// <summary>
        /// Regression test for a second structural bug: the unit was picked from the
        /// FULL month count while the amount was calculated separately, so the two
        /// could disagree and the card read "12 months" - a unit the display should
        /// never produce. Pins the exact boundary: the text switches to years only
        /// once a whole calendar year has elapsed, never a day earlier.
        /// </summary>
        [Theory]
        [InlineData("2025-12-31", "11 months")] // one day short of the year
        [InlineData("2026-01-01", "1 year")]    // the year is complete
        public void ToDueDateText_AroundTheOneYearBoundary_SwitchesToYearsOnlyWhenTheYearIsComplete(
            string due, string expected)
        {
            // Given: a due date on either side of the full calendar year
            var reference = new DateTime(2025, 1, 1);
            var nextDueAt = DateTime.Parse(due, CultureInfo.InvariantCulture);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt, reference);

            // Then: the unit changes exactly at the completed year
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToDueDateText_GivenDueDateJustUnderOneYearAway_StillReportsElevenMonths()
        {
            // Given: a due date a week short of a full calendar year in the future
            var nextDueAt = DateTime.Now.AddMonths(12).AddDays(-7);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the not-yet-completed twelfth month is not counted
            Assert.Equal("11 months", result);
        }

        [Fact]
        public void ToDueDateText_GivenDueDateJustUnderOneYearOverdue_StillReportsElevenMonths()
        {
            // Given: a due date a week short of a full calendar year in the past
            var nextDueAt = DateTime.Now.AddMonths(-12).AddDays(7);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt);

            // Then: the not-yet-completed twelfth month is not counted
            Assert.Equal("Overdue for 11 months", result);
        }

        /// <summary>
        /// Keeps the historical "12 months" symptom nailed down for every calendar
        /// position rather than for the two pinned dates above. Counting only whole
        /// units makes that rendering impossible by construction - this test is what
        /// would notice if a later change reintroduced a separate amount calculation
        /// that can overshoot its own branch again.
        /// </summary>
        [Fact]
        public void ToDueDateText_ForEveryReferenceDayOfALeapYear_NeverRendersTwelveMonths()
        {
            // Given: every day of a leap year as the current day, and all spans that
            // can land inside the twelfth month
            for (var reference = new DateTime(2024, 1, 1); reference.Year == 2024; reference = reference.AddDays(1))
            {
                for (int days = 300; days <= 400; days++)
                {
                    // When: the due date text is calculated in both directions
                    var upcoming = TimeUnitConverter.ToDueDateText(reference.AddDays(days), reference);
                    var overdue = TimeUnitConverter.ToDueDateText(reference.AddDays(-days), reference);

                    // Then: neither direction ever names twelve months
                    Assert.DoesNotContain("12 months", upcoming);
                    Assert.DoesNotContain("12 months", overdue);
                }
            }
        }

        /// <summary>
        /// The week branch is the one unit that is not calendar-dependent, so it
        /// truncates on a plain seven-day basis. Pinned days make the boundaries
        /// explicit: a second week is only reported once all fourteen days are over.
        /// </summary>
        [Theory]
        [InlineData(7, "1 week")]
        [InlineData(13, "1 week")]  // one day short of two weeks
        [InlineData(14, "2 weeks")]
        [InlineData(20, "2 weeks")] // one day short of three weeks
        [InlineData(21, "3 weeks")]
        public void ToDueDateText_WithinTheFirstCalendarMonth_CountsOnlyWholeWeeks(int days, string expected)
        {
            // Given: a span that stays below one full calendar month
            var reference = new DateTime(2025, 5, 1);
            var nextDueAt = reference.AddDays(days);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt, reference);

            // Then: only whole weeks are counted, the partial one is dropped
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Pins the day on which the text switches from one unit to the next. The
        /// property test below only checks the amount, so without these pairs a
        /// span of 28 days could silently start reading "28 days" instead of
        /// "4 weeks" and still pass.
        ///
        /// May is used as the reference month because its 31 days let the week
        /// branch reach its widest before a full month is complete.
        /// </summary>
        [Theory]
        [InlineData(6, "6 days")]     // days ...
        [InlineData(7, "1 week")]     // ... become weeks
        [InlineData(30, "4 weeks")]   // weeks ...
        [InlineData(31, "1 month")]   // ... become months once the month is full
        [InlineData(60, "1 month")]   // one day short of the second month
        [InlineData(61, "2 months")]
        public void ToDueDateText_AtEachUnitBoundary_SwitchesOnlyWhenTheNextUnitIsComplete(
            int days, string expected)
        {
            // Given: a due date the given number of days ahead
            var reference = new DateTime(2025, 5, 1);
            var nextDueAt = reference.AddDays(days);

            // When: the due date text is calculated
            var result = TimeUnitConverter.ToDueDateText(nextDueAt, reference);

            // Then: the unit changes exactly one day after the larger one is full
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// The defining property of the new rule, checked across every calendar
        /// position instead of on single pinned dates: the reported span must have
        /// elapsed completely, and the next one up must not have. Both bounds
        /// together are exactly what "truncate, never round up" means.
        ///
        /// The check converts the rendered text back into a date through the class's
        /// own ToDueDate, so it measures against the same definition of a unit that
        /// the rest of the app schedules by.
        /// </summary>
        [Fact]
        public void ToDueDateText_ForEveryCalendarPosition_NeverReportsMoreTimeThanHasElapsed()
        {
            // Given: every day of a leap year as the current day, and spans from a
            // single day up to well beyond a year
            for (var reference = new DateTime(2024, 1, 1); reference.Year == 2024; reference = reference.AddDays(1))
            {
                for (int days = 1; days <= 400; days++)
                {
                    // When: the due date text is calculated in both directions
                    AssertTruncated(reference, reference.AddDays(days), isOverdue: false);
                    AssertTruncated(reference.AddDays(-days), reference, isOverdue: true);
                }
            }
        }

        // Then: the stated amount fits into the span, and one more would not.
        // Which end of the span is the due date depends on the direction, so it is
        // passed in rather than guessed - "earlier" is always the start of the span.
        private static void AssertTruncated(DateTime earlier, DateTime later, bool isOverdue)
        {
            var nextDueAt = isOverdue ? earlier : later;
            var reference = isOverdue ? later : earlier;

            var text = TimeUnitConverter.ToDueDateText(nextDueAt, reference);
            var (amount, unit) = ParseDueDateText(text);

            Assert.True(TimeUnitConverter.ToDueDate(earlier, amount, unit) <= later,
                $"\"{text}\" claims more time than the span {earlier:yyyy-MM-dd}..{later:yyyy-MM-dd} holds");

            Assert.True(TimeUnitConverter.ToDueDate(earlier, amount + 1, unit) > later,
                $"\"{text}\" understates the span {earlier:yyyy-MM-dd}..{later:yyyy-MM-dd} by a whole unit");
        }

        private static (int Amount, TimeUnit Unit) ParseDueDateText(string text)
        {
            var words = text.Replace("Overdue for ", string.Empty).Split(' ');
            var unit = words[1].TrimEnd('s') switch
            {
                "day" => TimeUnit.Days,
                "week" => TimeUnit.Weeks,
                "month" => TimeUnit.Months,
                "year" => TimeUnit.Years,
                _ => throw new FormatException($"Unexpected unit in \"{text}\"")
            };

            return (int.Parse(words[0], CultureInfo.InvariantCulture), unit);
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

        // -- ToDueDate Tests --

        [Theory]
        [InlineData(3, TimeUnit.Days, "2025-05-04")]
        [InlineData(2, TimeUnit.Weeks, "2025-05-15")]
        [InlineData(4, TimeUnit.Months, "2025-09-01")]
        [InlineData(1, TimeUnit.Years, "2026-05-01")]
        public void ToDueDate_GivenAmountAndUnit_AdvancesTheStartDateAccordingly(
            int amount, TimeUnit unit, string expected)
        {
            // Given: a fixed start date
            var start = new DateTime(2025, 5, 1);

            // When: the due date is calculated
            var result = TimeUnitConverter.ToDueDate(start, amount, unit);

            // Then: the start date moved by exactly that interval
            Assert.Equal(DateTime.Parse(expected, CultureInfo.InvariantCulture), result);
        }

        /// <summary>
        /// Months and years are calendar steps, not fixed day counts: a target month
        /// that is too short clamps to its last day. This is what lets a schedule
        /// set on the 31st survive February.
        /// </summary>
        [Theory]
        [InlineData("2025-01-31", 1, TimeUnit.Months, "2025-02-28")]
        [InlineData("2024-01-31", 1, TimeUnit.Months, "2024-02-29")]
        [InlineData("2024-02-29", 1, TimeUnit.Years, "2025-02-28")]
        public void ToDueDate_GivenATargetTheShorterMonthDoesNotHave_ClampsToItsLastDay(
            string start, int amount, TimeUnit unit, string expected)
        {
            // Given: a start date on a day the target month may not have
            var startDate = DateTime.Parse(start, CultureInfo.InvariantCulture);

            // When: the due date is calculated
            var result = TimeUnitConverter.ToDueDate(startDate, amount, unit);

            // Then: it lands on the last day of the shorter month
            Assert.Equal(DateTime.Parse(expected, CultureInfo.InvariantCulture), result);
        }

        // -- ToTimeSpan Tests --

        [Theory]
        [InlineData(1, TimeUnit.Days, 24)]
        [InlineData(3, TimeUnit.Days, 72)]
        [InlineData(1, TimeUnit.Weeks, 168)]
        [InlineData(2, TimeUnit.Weeks, 336)]
        public void ToTimeSpan_GivenDaysOrWeeks_ReturnsThatManyHours(
            int amount, TimeUnit unit, int expectedHours)
        {
            // Given / When: the interval is converted into a flat TimeSpan
            var result = TimeUnitConverter.ToTimeSpan(amount, unit);

            // Then: days and weeks have a fixed length, so the hours are exact
            Assert.Equal(TimeSpan.FromHours(expectedHours), result);
        }

        [Theory]
        [InlineData(TimeUnit.Months)]
        [InlineData(TimeUnit.Years)]
        public void ToTimeSpan_GivenACalendarUnit_Throws(TimeUnit unit)
        {
            // Given: a unit whose length depends on the calendar
            // When / Then: it cannot be expressed as a flat TimeSpan, so the
            // conversion refuses instead of silently assuming 30 or 365 days
            Assert.Throws<ArgumentOutOfRangeException>(
                () => TimeUnitConverter.ToTimeSpan(1, unit));
        }
    }
}
