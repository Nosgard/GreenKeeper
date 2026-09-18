using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.CareStatuses.Active;
using GreenKeeper.ViewModels.CareStatuses.Passive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Tests.ViewModels
{
    /// <summary>
    /// Covers the status text of the active care status cards (Watering and Fertilizing).
    /// The wording of the span itself belongs to TimeUnitConverter and is tested
    /// there - what is checked here is the "Due in" prefix the cards put in front
    /// of it, and above all WHEN they do not.
    /// </summary>
    public class ActiveCareStatusViewModelTests
    {
        // The cards only need a schedule and callbacks they never invoke here,
        // so the actions are empty stand-ins.
        private static WateringStatusViewModel WateringCard(DateTime? nextDueAt) =>
            new(new CareSchedule { Care = CareType.Watering, NextDueAt = nextDueAt },
                onComplete: () => { }, onEdit: () => { });

        private static FertilizingStatusViewModel FertilizingCard(DateTime? nextDueAt) =>
            new(new CareSchedule { Care = CareType.Fertilizing, NextDueAt = nextDueAt },
                onComplete: () => { }, onEdit: () => { }, onRemove: () => { });

        // -- Upcoming due dates get the prefix --

        [Theory]
        [InlineData(1, "Due in 1 day")]
        [InlineData(3, "Due in 3 days")]
        [InlineData(7, "Due in 1 week")]
        [InlineData(14, "Due in 2 weeks")]
        public void StatusText_GivenUpcomingDueDateInDaysOrWeeks_PrefixesTheSpanWithDueIn(
            int daysFromNow, string expected)
        {
            // Given: a watering card whose due date is still ahead
            var card = WateringCard(DateTime.Now.AddDays(daysFromNow));

            // When: the status text is read
            var result = card.StatusText;

            // Then: the span reads as a forecast rather than a bare number
            Assert.Equal(expected, result);
        }

        [Fact]
        public void StatusText_GivenUpcomingDueDateMonthsAway_PrefixesTheSpanWithDueIn()
        {
            // Given: a watering card due two calendar months from now
            var card = WateringCard(DateTime.Now.AddMonths(2));

            // When: the status text is read
            var result = card.StatusText;

            // Then: months are prefixed just like days and weeks
            Assert.Equal("Due in 2 months", result);
        }

        [Fact]
        public void StatusText_GivenUpcomingDueDateYearsAway_PrefixesTheSpanWithDueIn()
        {
            // Given: a watering card due one calendar year from now
            var card = WateringCard(DateTime.Now.AddYears(1));

            // When: the status text is read
            var result = card.StatusText;

            // Then: years are prefixed just like the smaller units
            Assert.Equal("Due in 1 year", result);
        }

        // -- Today and overdue stay untouched --

        [Fact]
        public void StatusText_GivenDueDateIsToday_ReturnsTodayWithoutPrefix()
        {
            // Given: a watering card due today
            var card = WateringCard(DateTime.Now);

            // When: the status text is read
            var result = card.StatusText;

            // Then: "Today" already says what it means and gets no prefix
            Assert.Equal("Today", result);
        }

        /// <summary>
        /// The prefix is decided on the calendar date, not on the timestamp - the
        /// same rule "Today" and the Complete button already follow. A due date
        /// later on the current day is still today and must not turn into
        /// "Due in ...", and the first minute of the next day must.
        /// </summary>
        [Fact]
        public void StatusText_GivenDueDateLaterOnTheCurrentDay_StillReturnsTodayWithoutPrefix()
        {
            // Given: a watering card due just before midnight of the current day
            var card = WateringCard(DateTime.Today.AddHours(23).AddMinutes(59));

            // When: the status text is read
            var result = card.StatusText;

            // Then: the leftover time of day does not make it an upcoming date
            Assert.Equal("Today", result);
        }

        [Fact]
        public void StatusText_GivenDueDateJustAfterMidnight_PrefixesTheSpanWithDueIn()
        {
            // Given: a watering card due one minute into the next calendar day
            var card = WateringCard(DateTime.Today.AddDays(1).AddMinutes(1));

            // When: the status text is read
            var result = card.StatusText;

            // Then: the new calendar day counts as a full day ahead
            Assert.Equal("Due in 1 day", result);
        }

        [Theory]
        [InlineData(1, "Overdue for 1 day")]
        [InlineData(14, "Overdue for 2 weeks")]
        public void StatusText_GivenOverdueDueDate_ReturnsOverdueTextWithoutPrefix(
            int daysOverdue, string expected)
        {
            // Given: a watering card whose due date has passed
            var card = WateringCard(DateTime.Now.AddDays(-daysOverdue));

            // When: the status text is read
            var result = card.StatusText;

            // Then: "Overdue for ..." reads as a sentence already and stays as it is
            Assert.Equal(expected, result);
        }

        // -- No due date must not produce a bare prefix --

        [Fact]
        public void StatusText_GivenScheduleWithoutDueDate_ReturnsEmptyStringWithoutPrefix()
        {
            // Given: a watering card whose schedule carries no due date
            var card = WateringCard(null);

            // When: the status text is read
            var result = card.StatusText;

            // Then: the empty text stays empty instead of becoming a dangling "Due in"
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void StatusText_GivenNoScheduleAtAll_ReturnsEmptyStringWithoutPrefix()
        {
            // Given: a watering card for a plant that has no watering schedule yet -
            // MainViewModel creates the mandatory card even then, with a null schedule
            var card = new WateringStatusViewModel(schedule: null, onComplete: () => { }, onEdit: () => { });

            // When: the status text is read
            var result = card.StatusText;

            // Then: nothing is prefixed onto the empty text
            Assert.Equal(string.Empty, result);
        }

        // -- The rule applies to both active cards, and only to them --

        [Theory]
        [InlineData(5, "Due in 5 days")]
        [InlineData(-5, "Overdue for 5 days")]
        [InlineData(0, "Today")]
        public void StatusText_GivenFertilizingCard_FollowsTheSameRuleAsWatering(
            int dayOffset, string expected)
        {
            // Given: a fertilizing card - the second card sharing ActiveCareStatusViewModel
            var card = FertilizingCard(DateTime.Now.AddDays(dayOffset));

            // When: the status text is read
            var result = card.StatusText;

            // Then: it is worded exactly like the watering card
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Guards the boundary of the change: the passive sunlight card describes an
        /// amount per period rather than a due date, so it must never pick up the
        /// prefix.
        /// </summary>
        [Fact]
        public void StatusText_GivenSunlightCard_IsNotPrefixed()
        {
            // Given: a sunlight card, which has no due date at all
            var requirement = new SunlightRequirement { Hours = 6, Period = SunlightPeriod.Day };
            var card = new SunlightStatusViewModel(requirement, onEdit: () => { }, onRemove: () => { });

            // When: the status text is read
            var result = card.StatusText;

            // Then: the passive card keeps its own wording
            Assert.Equal("6h / day", result);
        }
    }
}
