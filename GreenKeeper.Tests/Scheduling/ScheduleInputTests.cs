using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;

namespace GreenKeeper.Tests.Scheduling
{
    /// <summary>
    /// Covers the overwrite question the Add Schedule wizard asks before it
    /// replaces an existing entry. The care type sits in the middle of the
    /// sentence there, so it has to read in lower case.
    /// </summary>
    public class ScheduleInputTests
    {
        [Theory]
        [InlineData(CareType.Watering, "There is already a watering schedule for this plant. Do you want to replace it?")]
        [InlineData(CareType.Fertilizing, "There is already a fertilizing schedule for this plant. Do you want to replace it?")]
        public void OverwriteQuestion_GivenCareSchedule_NamesTheCareTypeInLowerCase(CareType careType, string expected)
        {
            // Given: a new schedule of the given care type
            var input = new CareScheduleInput(new CareSchedule { Care = careType });

            // When: the overwrite question is read
            var question = input.OverwriteQuestion;

            // Then: the care type reads as part of the sentence
            Assert.Equal(expected, question);
        }

        [Fact]
        public void OverwriteQuestion_GivenSunlightRequirement_NamesTheRequirementInLowerCase()
        {
            // Given: a new sunlight requirement
            var input = new SunlightRequirementInput(new SunlightRequirement { Hours = 6, Period = SunlightPeriod.Day });

            // When: the overwrite question is read
            var question = input.OverwriteQuestion;

            // Then: the requirement reads as part of the sentence
            Assert.Equal("There is already a sunlight requirement for this plant. Do you want to replace it?", question);
        }
    }
}
