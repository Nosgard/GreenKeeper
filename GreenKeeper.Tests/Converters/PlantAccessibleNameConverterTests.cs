using GreenKeeper.Converters;
using GreenKeeper.Models;
using GreenKeeper.Tests.Fakes;
using System.Globalization;
using static GreenKeeper.Tests.TestPlants;

namespace GreenKeeper.Tests.Converters
{
    /// <summary>
    /// What a screen reader announces for a plant entry in the sidebar: the name,
    /// then the state its status dot shows only by its color.
    /// </summary>
    public class PlantAccessibleNameConverterTests
    {
        // A fixed clock, so "today" is the same in every run.
        private static readonly DateTime Now = new(2025, 5, 1, 10, 30, 0);

        [Theory]
        [InlineData(-1, "Aloe Vera, overdue")]
        [InlineData(0, "Aloe Vera, due today")]
        [InlineData(1, "Aloe Vera, up to date")]
        public void Convert_GivenWateringDueAroundToday_NamesThePlantAndItsState(int dayOffset, string expectedName)
        {
            // Given: a plant whose watering is due the given number of days from today
            var plant = AloeVera(WateringEveryDays(7, nextDueAt: Now.AddDays(dayOffset)));

            // When: the accessible name of its entry is determined
            var name = AccessibleNameOf(plant);

            // Then: the visible name comes first, then the state of the status dot in words
            Assert.Equal(expectedName, name);
        }

        // Converts the way the entry's binding does: the plant is the bound value.
        private static object AccessibleNameOf(Plant plant)
        {
            var converter = new PlantAccessibleNameConverter(new FakeTimeProvider(Now));
            return converter.Convert(plant, typeof(string), parameter: null!, CultureInfo.InvariantCulture);
        }
    }
}
