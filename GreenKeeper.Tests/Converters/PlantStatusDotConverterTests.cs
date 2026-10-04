using GreenKeeper.Converters;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Tests.Fakes;
using System.Globalization;
using static GreenKeeper.Tests.TestPlants;

namespace GreenKeeper.Tests.Converters
{
    /// <summary>
    /// The state the status dot of a sidebar entry is drawn from: overdue after a
    /// missed care, due today while one falls on today, otherwise up to date.
    /// StatusDotStyles.xaml decides how each state looks.
    /// </summary>
    public class PlantStatusDotConverterTests
    {
        // The clock the converter runs on, pinned so "today" is the same on every run.
        private static readonly DateTime Now = new(2025, 5, 1, 10, 30, 0);

        [Theory]
        [InlineData(-1, PlantStatus.Overdue)]
        [InlineData(0, PlantStatus.DueToday)]
        [InlineData(1, PlantStatus.UpToDate)]
        public void Convert_GivenWateringDueAroundToday_ReturnsThatState(int dayOffset, PlantStatus expectedStatus)
        {
            // Given: a plant whose watering is due the given number of days from today
            var plant = AloeVera(WateringEveryDays(7, nextDueAt: Now.AddDays(dayOffset)));

            // When: the state of the plant is determined
            var status = StatusOf(plant);

            // Then: a missed day, today and a day still ahead each give their own state
            Assert.Equal(expectedStatus, status);
        }

        [Theory]
        [InlineData(0, -1, PlantStatus.Overdue)]
        [InlineData(3, -1, PlantStatus.Overdue)]
        [InlineData(3, 0, PlantStatus.DueToday)]
        public void Convert_GivenTwoSchedulesInDifferentStates_ReturnsTheStateOfTheMoreUrgentOne(
            int wateringDayOffset, int fertilizingDayOffset, PlantStatus expectedStatus)
        {
            // Given: a plant whose watering and fertilizing are due on different days
            var plant = AloeVera(
                WateringEveryDays(7, nextDueAt: Now.AddDays(wateringDayOffset)),
                FertilizingEveryDays(30, nextDueAt: Now.AddDays(fertilizingDayOffset)));

            // When: the state of the plant is determined
            var status = StatusOf(plant);

            // Then: overdue outranks due today, and due today outranks still ahead
            Assert.Equal(expectedStatus, status);
        }

        // Converts the way the sidebar binding does: the plant is the bound value,
        // the other arguments of the WPF interface are not used by the converter.
        private static PlantStatus StatusOf(Plant plant)
        {
            var converter = new PlantStatusDotConverter(new FakeTimeProvider(Now));
            return (PlantStatus)converter.Convert(plant, typeof(object), parameter: null!, CultureInfo.InvariantCulture);
        }
    }
}
