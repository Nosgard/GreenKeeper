using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;

namespace GreenKeeper.Tests.Scheduling
{
    /// <summary>
    /// Covers what the result of the Add Schedule wizard or the edit dialog knows
    /// about itself: the question to ask before it replaces an existing entry,
    /// and the operation it is saved through.
    /// </summary>
    public class ScheduleInputTests
    {
        // -- Overwrite Question Tests --
        // The care type sits in the middle of the sentence there, so it has to read in lower case.

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

        // -- Save Tests --

        [Fact]
        public async Task SaveAsync_GivenCareSchedule_SavesItAsCareSchedule()
        {
            // Given: a new fertilizing schedule, and an editor that records what it is asked to save
            var schedule = new CareSchedule { Care = CareType.Fertilizing, IntervalAmount = 30, IntervalUnit = TimeUnit.Days };
            var input = new CareScheduleInput(schedule);
            var editor = new RecordingPlantCareEditor();

            // When: the input is saved through the editor
            await input.SaveAsync(editor);

            // Then: the schedule arrived as a care schedule, and nothing as a sunlight requirement
            Assert.Same(schedule, editor.SavedCareSchedule);
            Assert.Null(editor.SavedSunlightRequirement);
        }

        [Fact]
        public async Task SaveAsync_GivenSunlightRequirement_SavesItAsSunlightRequirement()
        {
            // Given: a new sunlight requirement, and an editor that records what it is asked to save
            var requirement = new SunlightRequirement { Hours = 6, Period = SunlightPeriod.Day };
            var input = new SunlightRequirementInput(requirement);
            var editor = new RecordingPlantCareEditor();

            // When: the input is saved through the editor
            await input.SaveAsync(editor);

            // Then: the requirement arrived as a sunlight requirement, and nothing as a care schedule
            Assert.Same(requirement, editor.SavedSunlightRequirement);
            Assert.Null(editor.SavedCareSchedule);
        }

        // Stands in for MainViewModel, which the app passes as the editor.
        private sealed class RecordingPlantCareEditor : IPlantCareEditor
        {
            public CareSchedule? SavedCareSchedule { get; private set; }
            public SunlightRequirement? SavedSunlightRequirement { get; private set; }

            public Task AddOrReplaceCareScheduleAsync(CareSchedule careSchedule)
            {
                SavedCareSchedule = careSchedule;
                return Task.CompletedTask;
            }

            public Task AddOrReplaceSunlightRequirementAsync(SunlightRequirement sunlightRequirement)
            {
                SavedSunlightRequirement = sunlightRequirement;
                return Task.CompletedTask;
            }
        }
    }
}
