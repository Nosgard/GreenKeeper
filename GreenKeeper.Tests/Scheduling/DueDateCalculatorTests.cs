using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using System.Globalization;

namespace GreenKeeper.Tests.Scheduling
{
    public class DueDateCalculatorTests
    {
        // A pinned "today", so the results do not depend on the day the tests run.
        private static readonly DateTime Today = new(2025, 5, 1);

        [Fact]
        public void ToDueDateText_GivenDueDateIsToday_ReturnsToday()
        {
            // Given: a due date that falls on the current day, later in the day
            var nextDueAt = Today.AddHours(15);

            // When: the due date text is calculated
            var result = DueDateCalculator.ToDueDateText(nextDueAt, Today);

            // Then: it reads "Today"
            Assert.Equal("Today", result);
        }

        /// <summary>
        /// Overdue spans read in the largest unit that has elapsed in full, in the singular for
        /// one and in the plural for more. A due date one calendar day back reads "Overdue for 1 day",
        /// not "Overdue for 0 days".
        /// </summary>
        [Theory]
        [InlineData(1, TimeUnit.Days, "Overdue for 1 day")]
        [InlineData(2, TimeUnit.Days, "Overdue for 2 days")]
        [InlineData(1, TimeUnit.Weeks, "Overdue for 1 week")]
        [InlineData(2, TimeUnit.Weeks, "Overdue for 2 weeks")]
        [InlineData(35, TimeUnit.Days, "Overdue for 1 month")]
        [InlineData(2, TimeUnit.Months, "Overdue for 2 months")]
        [InlineData(1, TimeUnit.Years, "Overdue for 1 year")]
        [InlineData(2, TimeUnit.Years, "Overdue for 2 years")]
        public void ToDueDateText_GivenOverdueSpan_ReportsTheCompletedUnitInSingularOrPlural(
            int amount, TimeUnit unit, string expected)
        {
            // Given: a due date that lies the given span in the past
            var nextDueAt = DueDateCalculator.ToDueDate(Today, -amount, unit);

            // When: the due date text is calculated
            var result = DueDateCalculator.ToDueDateText(nextDueAt, Today);

            // Then: the span reads as overdue, in the largest unit that has elapsed in full
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(1, TimeUnit.Days, "1 day")]
        [InlineData(3, TimeUnit.Days, "3 days")]
        [InlineData(1, TimeUnit.Weeks, "1 week")]
        [InlineData(2, TimeUnit.Weeks, "2 weeks")]
        [InlineData(1, TimeUnit.Months, "1 month")]
        [InlineData(2, TimeUnit.Months, "2 months")]
        [InlineData(1, TimeUnit.Years, "1 year")]
        [InlineData(2, TimeUnit.Years, "2 years")]
        public void ToDueDateText_GivenUpcomingSpan_ReportsTheCompletedUnitInSingularOrPlural(
            int amount, TimeUnit unit, string expected)
        {
            // Given: a due date that lies the given span ahead
            var nextDueAt = DueDateCalculator.ToDueDate(Today, amount, unit);

            // When: the due date text is calculated
            var result = DueDateCalculator.ToDueDateText(nextDueAt, Today);

            // Then: the span is reported in the largest unit that has elapsed in full
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// A month step can be clamped: Jan 31 + 1 month is Feb 28, so Feb 28 is one full month
        /// after Jan 31, although 28 &lt; 31. Comparing the day-of-month numbers would count
        /// zero months here, and the card would read "4 weeks" instead of "1 month".
        ///
        /// The dates are pinned rather than derived from DateTime.Now: a test relative to today
        /// would reach such a month end on a few days per year only and pass on every other day.
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
            var result = DueDateCalculator.ToDueDateText(nextDueAt, reference);

            // Then: the span counts as a full month, not as four weeks
            Assert.Equal("1 month", result);
        }

        /// <summary>
        /// The overdue direction of the same clamping: the due date falls on a day the next month
        /// does not have, and "today" is the clamped target.
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
            var result = DueDateCalculator.ToDueDateText(nextDueAt, reference);

            // Then: the span counts as a full month, not as four weeks
            Assert.Equal("Overdue for 1 month", result);
        }

        /// <summary>
        /// Years clamp like months: Feb 29 plus one year is Feb 28, which counts as one full
        /// year.
        /// </summary>
        [Fact]
        public void ToDueDateText_GivenSpanFromALeapDayToTheClampedAnniversary_ReturnsOneYear()
        {
            // Given: a due date on a leap day and a reference exactly one year later,
            // which AddYears clamps to Feb 28
            var reference = new DateTime(2025, 2, 28);
            var nextDueAt = new DateTime(2024, 2, 29);

            // When: the due date text is calculated
            var result = DueDateCalculator.ToDueDateText(nextDueAt, reference);

            // Then: the anniversary counts as a full year
            Assert.Equal("Overdue for 1 year", result);
        }

        /// <summary>
        /// Unit and amount have to come from the same count; a separate amount could disagree
        /// with the unit and make the card read "12 months". The text switches to years once a
        /// whole calendar year has elapsed, not a day earlier.
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
            var result = DueDateCalculator.ToDueDateText(nextDueAt, reference);

            // Then: the unit changes exactly at the completed year
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToDueDateText_GivenDueDateJustUnderOneYearAway_StillReportsElevenMonths()
        {
            // Given: a due date a week short of a full calendar year in the future
            var nextDueAt = Today.AddMonths(12).AddDays(-7);

            // When: the due date text is calculated
            var result = DueDateCalculator.ToDueDateText(nextDueAt, Today);

            // Then: the not-yet-completed twelfth month is not counted
            Assert.Equal("11 months", result);
        }

        [Fact]
        public void ToDueDateText_GivenDueDateJustUnderOneYearOverdue_StillReportsElevenMonths()
        {
            // Given: a due date a week short of a full calendar year in the past
            var nextDueAt = Today.AddMonths(-12).AddDays(7);

            // When: the due date text is calculated
            var result = DueDateCalculator.ToDueDateText(nextDueAt, Today);

            // Then: the not-yet-completed twelfth month is not counted
            Assert.Equal("Overdue for 11 months", result);
        }

        /// <summary>
        /// Checks every calendar position, not only the two pinned dates above, for the text
        /// "12 months". Counting whole units rules it out; this test notices if a separate
        /// amount calculation comes back.
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
                    var upcoming = DueDateCalculator.ToDueDateText(reference.AddDays(days), reference);
                    var overdue = DueDateCalculator.ToDueDateText(reference.AddDays(-days), reference);

                    // Then: neither direction ever names twelve months
                    Assert.DoesNotContain("12 months", upcoming);
                    Assert.DoesNotContain("12 months", overdue);
                }
            }
        }

        /// <summary>
        /// Weeks do not depend on the calendar and count in steps of seven days. Pinned days
        /// make the boundaries explicit: a second week is only reported once all fourteen days
        /// are over.
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
            var result = DueDateCalculator.ToDueDateText(nextDueAt, reference);

            // Then: only whole weeks are counted, the partial one is dropped
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Pins the day on which the text switches from one unit to the next. The property test
        /// below only checks the amount, so without these pairs a span of 28 days could read
        /// "28 days" instead of "4 weeks" and still pass.
        ///
        /// May is the reference month because its 31 days let the week branch reach its widest
        /// before a full month is complete.
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
            var result = DueDateCalculator.ToDueDateText(nextDueAt, reference);

            // Then: the text switches to the larger unit on the day that unit is complete
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Checks the rule on every calendar position instead of on single pinned dates: the
        /// reported amount has fully elapsed, and one unit more has not.
        ///
        /// The check turns the text back into a date with ToDueDate, so it measures against the
        /// same definition of a unit that the app schedules by.
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
        // The direction decides which end of the span is the due date, so the caller passes it;
        // "earlier" is the start of the span.
        private static void AssertTruncated(DateTime earlier, DateTime later, bool isOverdue)
        {
            var nextDueAt = isOverdue ? earlier : later;
            var reference = isOverdue ? later : earlier;

            var text = DueDateCalculator.ToDueDateText(nextDueAt, reference);
            var (amount, unit) = ParseDueDateText(text);

            Assert.True(DueDateCalculator.ToDueDate(earlier, amount, unit) <= later,
                $"\"{text}\" claims more time than the span {earlier:yyyy-MM-dd}..{later:yyyy-MM-dd} holds");

            Assert.True(DueDateCalculator.ToDueDate(earlier, amount + 1, unit) > later,
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
            var result = DueDateCalculator.ToDueDateText(nextDueAt, Today);

            // Then: the text is empty, not an exception or "null"
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
            var result = DueDateCalculator.ToDueDate(start, amount, unit);

            // Then: the start date moved by exactly that interval
            Assert.Equal(DateTime.Parse(expected, CultureInfo.InvariantCulture), result);
        }

        /// <summary>
        /// Months and years are calendar steps, not fixed day counts: a target month that is too
        /// short clamps to its last day, so a schedule set on the 31st still falls due in
        /// February.
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
            var result = DueDateCalculator.ToDueDate(startDate, amount, unit);

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
            // Given: an interval of the given amount of days or weeks (InlineData)

            // When: the interval is converted into a flat TimeSpan
            var result = DueDateCalculator.ToTimeSpan(amount, unit);

            // Then: days and weeks have a fixed length, so the hours are exact
            Assert.Equal(TimeSpan.FromHours(expectedHours), result);
        }

        [Theory]
        [InlineData(TimeUnit.Months)]
        [InlineData(TimeUnit.Years)]
        public void ToTimeSpan_GivenACalendarUnit_Throws(TimeUnit unit)
        {
            // Given: a unit whose length depends on the calendar
            Action convert = () => DueDateCalculator.ToTimeSpan(1, unit);

            // When: it is converted into a flat TimeSpan
            var exception = Record.Exception(convert);

            // Then: the conversion refuses instead of assuming 30 or 365 days
            Assert.IsType<ArgumentOutOfRangeException>(exception);
        }
    }
}
