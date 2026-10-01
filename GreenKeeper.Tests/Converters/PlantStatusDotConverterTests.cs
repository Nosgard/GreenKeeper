using GreenKeeper.Converters;
using GreenKeeper.Models;
using GreenKeeper.Tests.Fakes;
using System.Globalization;
using static GreenKeeper.Tests.TestPlants;

namespace GreenKeeper.Tests.Converters
{
    /// <summary>
    /// Covers the status dot next to each plant in the sidebar: red once a care
    /// was missed, yellow while one is due today, green otherwise.
    /// </summary>
    public class PlantStatusDotConverterTests
    {
        private const string GreenDot = "/Resources/Icons/Dots/GreenDot.png";
        private const string YellowDot = "/Resources/Icons/Dots/YellowDot.png";
        private const string RedDot = "/Resources/Icons/Dots/RedDot.png";

        // The clock the converter runs on, pinned so "today" is the same on every run.
        private static readonly DateTime Now = new(2025, 5, 1, 10, 30, 0);

        [Theory]
        [InlineData(-1, RedDot)]
        [InlineData(0, YellowDot)]
        [InlineData(1, GreenDot)]
        public void Convert_GivenWateringDueAroundToday_ReturnsTheDotOfThatState(int dayOffset, string expectedDot)
        {
            // Given: a plant whose watering is due the given number of days from today
            var plant = AloeVera(WateringEveryDays(7, nextDueAt: Now.AddDays(dayOffset)));

            // When: the status dot of the plant is determined
            var dot = StatusDotOf(plant);

            // Then: a missed day is red, today is yellow and a day still ahead is green
            Assert.Equal(expectedDot, dot);
        }

        [Theory]
        [InlineData(0, -1, RedDot)]
        [InlineData(3, -1, RedDot)]
        [InlineData(3, 0, YellowDot)]
        public void Convert_GivenTwoSchedulesInDifferentStates_ReturnsTheDotOfTheMoreUrgentOne(
            int wateringDayOffset, int fertilizingDayOffset, string expectedDot)
        {
            // Given: a plant whose watering and fertilizing are due on different days
            var plant = AloeVera(
                WateringEveryDays(7, nextDueAt: Now.AddDays(wateringDayOffset)),
                FertilizingEveryDays(30, nextDueAt: Now.AddDays(fertilizingDayOffset)));

            // When: the status dot of the plant is determined
            var dot = StatusDotOf(plant);

            // Then: overdue outranks due today, and due today outranks still ahead
            Assert.Equal(expectedDot, dot);
        }

        // Converts the way the sidebar binding does: the plant is the bound value,
        // the other arguments of the WPF interface are not used by the converter.
        private static object StatusDotOf(Plant plant)
        {
            var converter = new PlantStatusDotConverter(new FakeTimeProvider(Now));
            return converter.Convert(plant, typeof(object), parameter: null!, CultureInfo.InvariantCulture);
        }
    }
}
