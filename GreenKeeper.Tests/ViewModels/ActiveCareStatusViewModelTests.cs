using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Tests.Fakes;
using GreenKeeper.ViewModels.CareStatuses.Active;
using GreenKeeper.ViewModels.CareStatuses.Passive;

namespace GreenKeeper.Tests.ViewModels
{
    /// <summary>
    /// The active cards (Watering and Fertilizing): the "Due in" prefix of their status text,
    /// and from which day the Complete button works and the text turns red. DueDateCalculator
    /// words the span itself and is tested there.
    /// </summary>
    public class ActiveCareStatusViewModelTests
    {
        // The clock the cards run on, pinned so "today" is the same on every run.
        private static readonly DateTime Now = new(2025, 5, 1, 10, 30, 0);
        private static readonly TimeProvider Clock = new FakeTimeProvider(Now);

        // The cards only need a schedule and callbacks they never invoke here,
        // so the callbacks are empty stand-ins.
        private static WateringStatusViewModel WateringCard(DateTime? nextDueAt) =>
            new(new CareSchedule { Care = CareType.Watering, NextDueAt = nextDueAt },
                onComplete: () => Task.CompletedTask, onEdit: () => { }, Clock);

        private static FertilizingStatusViewModel FertilizingCard(DateTime? nextDueAt) =>
            new(new CareSchedule { Care = CareType.Fertilizing, NextDueAt = nextDueAt },
                onComplete: () => Task.CompletedTask, onEdit: () => { }, onRemove: () => Task.CompletedTask, Clock);

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
            var card = WateringCard(Now.AddDays(daysFromNow));

            // When: the status text is read
            var result = card.StatusText;

            // Then: the span reads as a forecast rather than a bare number
            Assert.Equal(expected, result);
        }

        [Fact]
        public void StatusText_GivenUpcomingDueDateMonthsAway_PrefixesTheSpanWithDueIn()
        {
            // Given: a watering card due two calendar months from now
            var card = WateringCard(Now.AddMonths(2));

            // When: the status text is read
            var result = card.StatusText;

            // Then: months are prefixed like days and weeks
            Assert.Equal("Due in 2 months", result);
        }

        [Fact]
        public void StatusText_GivenUpcomingDueDateYearsAway_PrefixesTheSpanWithDueIn()
        {
            // Given: a watering card due one calendar year from now
            var card = WateringCard(Now.AddYears(1));

            // When: the status text is read
            var result = card.StatusText;

            // Then: years are prefixed like the smaller units
            Assert.Equal("Due in 1 year", result);
        }

        // -- Today reads "Due today", overdue stays untouched --

        [Fact]
        public void StatusText_GivenDueDateIsToday_ReturnsDueToday()
        {
            // Given: a watering card due today
            var card = WateringCard(Now);

            // When: the status text is read
            var result = card.StatusText;

            // Then: it reads like the other states, "Due today" next to "Due in 3 days"
            Assert.Equal("Due today", result);
        }

        /// <summary>
        /// The prefix is decided on the calendar date, not on the timestamp, by the same rule
        /// that "Due today" and the Complete button follow. A due date later on the current day
        /// is still today and must not turn into "Due in ...", and the first minute of the next
        /// day must.
        /// </summary>
        [Fact]
        public void StatusText_GivenDueDateLaterOnTheCurrentDay_StillReturnsDueToday()
        {
            // Given: a watering card due one minute before midnight of the current day
            var card = WateringCard(Now.Date.AddHours(23).AddMinutes(59));

            // When: the status text is read
            var result = card.StatusText;

            // Then: the leftover time of day does not make it an upcoming date
            Assert.Equal("Due today", result);
        }

        [Fact]
        public void StatusText_GivenDueDateJustAfterMidnight_PrefixesTheSpanWithDueIn()
        {
            // Given: a watering card due one minute into the next calendar day
            var card = WateringCard(Now.Date.AddDays(1).AddMinutes(1));

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
            var card = WateringCard(Now.AddDays(-daysOverdue));

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
            // Given: a watering card for a plant that has no watering schedule yet; DashboardViewModel
            // creates the mandatory card even then, with a null schedule
            var card = new WateringStatusViewModel(schedule: null, onComplete: () => Task.CompletedTask, onEdit: () => { }, Clock);

            // When: the status text is read
            var result = card.StatusText;

            // Then: nothing is prefixed onto the empty text
            Assert.Equal(string.Empty, result);
        }

        // -- The rule applies to both active cards, and only to them --

        [Theory]
        [InlineData(5, "Due in 5 days")]
        [InlineData(-5, "Overdue for 5 days")]
        [InlineData(0, "Due today")]
        public void StatusText_GivenFertilizingCard_FollowsTheSameRuleAsWatering(
            int dayOffset, string expected)
        {
            // Given: a fertilizing card, the second card built on ActiveCareStatusViewModel
            var card = FertilizingCard(Now.AddDays(dayOffset));

            // When: the status text is read
            var result = card.StatusText;

            // Then: it is worded exactly like the watering card
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// The passive sunlight card describes an amount per period rather than a due date, so
        /// it never gets the prefix.
        /// </summary>
        [Fact]
        public void StatusText_GivenSunlightCard_IsNotPrefixed()
        {
            // Given: a sunlight card, which has no due date
            var requirement = new SunlightRequirement { Hours = 6, Period = SunlightPeriod.Day };
            var card = new SunlightStatusViewModel(requirement, onEdit: () => { }, onRemove: () => Task.CompletedTask);

            // When: the status text is read
            var result = card.StatusText;

            // Then: the passive card keeps its own wording
            Assert.Equal("6h / day", result);
        }

        // -- Complete button and overdue coloring --

        /// <summary>
        /// IsCompletable is bound to the Complete button. A card due today has to be completable:
        /// its text reads "Due today", so a disabled button would contradict it.
        /// </summary>
        [Theory]
        [InlineData(-1, true)]  // overdue
        [InlineData(0, true)]   // due today
        [InlineData(1, false)]  // still ahead
        public void IsCompletable_IsTrueFromTheDueDayOnwards(int dayOffset, bool expected)
        {
            // Given: a watering card with a due date around today
            var card = WateringCard(Now.AddDays(dayOffset));

            // When: the enabled state of the Complete button is read
            var result = card.IsCompletable;

            // Then: it follows the calendar day
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// IsOverdue drives the red status text. A card due today is not overdue yet: red would
        /// contradict the green-yellow-red ladder of the plant's status dot, which turns red only
        /// once a day was missed.
        /// </summary>
        [Theory]
        [InlineData(-1, true)]  // overdue
        [InlineData(0, false)]  // due today is not late yet
        [InlineData(1, false)]  // still ahead
        public void IsOverdue_IsTrueOnlyAfterTheDueDayHasPassed(int dayOffset, bool expected)
        {
            // Given: a watering card with a due date around today
            var card = WateringCard(Now.AddDays(dayOffset));

            // When: the overdue state is read
            var result = card.IsOverdue;

            // Then: only a missed day counts as overdue
            Assert.Equal(expected, result);
        }

        [Fact]
        public void IsCompletableAndIsOverdue_GivenDueDateLaterOnTheCurrentDay_TreatItAsToday()
        {
            // Given: a watering card due one minute before midnight of the current day
            var card = WateringCard(Now.Date.AddHours(23).AddMinutes(59));

            // When: both states are read
            var isCompletable = card.IsCompletable;
            var isOverdue = card.IsOverdue;

            // Then: both judge by the calendar day and not by the clock, so the card
            // can already be completed and is not painted red
            Assert.True(isCompletable);
            Assert.False(isOverdue);
        }

        [Fact]
        public void IsCompletableAndIsOverdue_GivenNoDueDate_AreBothFalse()
        {
            // Given: a watering card without a due date
            var card = WateringCard(null);

            // When: both states are read
            var isCompletable = card.IsCompletable;
            var isOverdue = card.IsOverdue;

            // Then: there is nothing to complete and nothing to miss
            Assert.False(isCompletable);
            Assert.False(isOverdue);
        }

        [Fact]
        public void IsOverdue_GivenSunlightCard_IsAlwaysFalse()
        {
            // Given: a sunlight card, which has no due date to miss
            var requirement = new SunlightRequirement { Hours = 6, Period = SunlightPeriod.Day };
            var card = new SunlightStatusViewModel(requirement, onEdit: () => { }, onRemove: () => Task.CompletedTask);

            // When: the overdue state is read
            var result = card.IsOverdue;

            // Then: the passive card is never painted as overdue
            Assert.False(result);
        }
    }
}
