using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using GreenKeeper.Tests.Fakes;
using GreenKeeper.ViewModels.CareStatuses.EditOption;

namespace GreenKeeper.Tests.ViewModels
{
    public class EditScheduleViewModelTests
    {
        private static readonly TimeProvider Clock = new FakeTimeProvider(new DateTime(2025, 5, 1, 10, 30, 0));

        private static Plant PlantWithWateringAndSunlight()
        {
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Watering, IntervalAmount = 7, IntervalUnit = TimeUnit.Weeks });
            plant.SunlightRequirement = new SunlightRequirement { Hours = 6, Period = SunlightPeriod.Week };
            return plant;
        }

        // -- Pre-fill Tests --

        [Fact]
        public void Constructor_GivenExistingWateringSchedule_PrefillsTheActiveStep()
        {
            // Given: a plant watered every 7 weeks
            var plant = PlantWithWateringAndSunlight();

            // When: the edit dialog is opened for the watering schedule
            var viewModel = new EditScheduleViewModel(plant, CareType.Watering, Clock);

            // Then: the active step shows the stored interval under the care type's title
            var step = Assert.IsType<EditActiveScheduleViewModel>(viewModel.CurrentStep);
            Assert.Equal("Watering", step.Title);
            Assert.Equal("7", step.AmountText);
            Assert.Equal(TimeUnit.Weeks, step.SelectedUnit);
            Assert.True(viewModel.SaveCommand.CanExecute(null));
        }

        [Fact]
        public void Constructor_GivenNoFertilizingScheduleYet_StartsEmptyAndBlocksSaving()
        {
            // Given: a plant without a fertilizing schedule
            var plant = PlantWithWateringAndSunlight();

            // When: the edit dialog is opened for fertilizing, a care type the plant does not have
            var viewModel = new EditScheduleViewModel(plant, CareType.Fertilizing, Clock);

            // Then: the step is empty, defaults to days and cannot be saved yet
            var step = Assert.IsType<EditActiveScheduleViewModel>(viewModel.CurrentStep);
            Assert.Equal("Fertilizing", step.Title);
            Assert.Equal(string.Empty, step.AmountText);
            Assert.Equal(TimeUnit.Days, step.SelectedUnit);
            Assert.False(viewModel.SaveCommand.CanExecute(null));
        }

        [Fact]
        public void Constructor_GivenExistingSunlightRequirement_PrefillsTheSunlightStep()
        {
            // Given: a plant that needs 6 hours of sunlight per week
            var plant = PlantWithWateringAndSunlight();

            // When: the edit dialog is opened for the sunlight requirement
            var viewModel = new EditScheduleViewModel(plant, CareType.Sunlight, Clock);

            // Then: the sunlight step shows the stored hours and period
            var step = Assert.IsType<EditSunlightViewModel>(viewModel.CurrentStep);
            Assert.Equal("6", step.AmountText);
            Assert.Equal(SunlightPeriod.Week, step.SelectedPeriod);
            Assert.True(viewModel.SaveCommand.CanExecute(null));
        }

        [Fact]
        public void PreviewText_GivenValidAmount_AnnouncesTheNewDueDate()
        {
            // Given: the active step with a 7-day interval entered
            var viewModel = new EditScheduleViewModel(PlantWithWateringAndSunlight(), CareType.Watering, Clock);
            var step = (EditActiveScheduleViewModel)viewModel.CurrentStep;
            step.AmountText = "7";
            step.SelectedUnit = TimeUnit.Days;

            // When: the preview is read
            var preview = step.PreviewText;

            // Then: it names the due date counted from now
            Assert.Equal("New due date: 1 week", preview);
        }

        [Fact]
        public void PreviewText_GivenInvalidAmount_IsEmpty()
        {
            // Given: the active step with an invalid amount
            var viewModel = new EditScheduleViewModel(PlantWithWateringAndSunlight(), CareType.Watering, Clock);
            var step = (EditActiveScheduleViewModel)viewModel.CurrentStep;
            step.AmountText = "abc";

            // When: the preview is read
            var preview = step.PreviewText;

            // Then: there is nothing to announce
            Assert.Equal(string.Empty, preview);
            Assert.False(viewModel.SaveCommand.CanExecute(null));
        }

        // -- Save Tests --

        [Fact]
        public void SaveCommand_GivenChangedInterval_HandsOutANewScheduleAndClosesWithTrue()
        {
            // Given: the watering dialog with the interval changed to 3 days
            var plant = PlantWithWateringAndSunlight();
            var viewModel = new EditScheduleViewModel(plant, CareType.Watering, Clock);
            var step = (EditActiveScheduleViewModel)viewModel.CurrentStep;
            step.AmountText = "3";
            step.SelectedUnit = TimeUnit.Days;
            bool? closeResult = null;
            viewModel.RequestClose += (_, result) => closeResult = result;

            // When: Save is executed
            viewModel.SaveCommand.Execute(null);

            // Then: a new schedule with the entered values is handed out, the plant itself is untouched
            Assert.Equal(true, closeResult);
            var edited = Assert.IsType<CareScheduleInput>(viewModel.Result).CareSchedule;
            Assert.Equal(CareType.Watering, edited.Care);
            Assert.Equal(3, edited.IntervalAmount);
            Assert.Equal(TimeUnit.Days, edited.IntervalUnit);
            Assert.Equal(7, plant.CareSchedules.Single().IntervalAmount);
        }

        [Fact]
        public void SaveCommand_GivenChangedSunlight_HandsOutANewRequirementAndClosesWithTrue()
        {
            // Given: the sunlight dialog with the hours changed to 8 per day
            var plant = PlantWithWateringAndSunlight();
            var viewModel = new EditScheduleViewModel(plant, CareType.Sunlight, Clock);
            var step = (EditSunlightViewModel)viewModel.CurrentStep;
            step.AmountText = "8";
            step.SelectedPeriod = SunlightPeriod.Day;
            bool? closeResult = null;
            viewModel.RequestClose += (_, result) => closeResult = result;

            // When: Save is executed
            viewModel.SaveCommand.Execute(null);

            // Then: a new requirement with the entered values is handed out, the plant itself is untouched
            Assert.Equal(true, closeResult);
            var edited = Assert.IsType<SunlightRequirementInput>(viewModel.Result).SunlightRequirement;
            Assert.Equal(8, edited.Hours);
            Assert.Equal(SunlightPeriod.Day, edited.Period);
            Assert.Equal(6, plant.SunlightRequirement!.Hours);
        }

        [Fact]
        public void CancelCommand_ClosesWithFalseAndHandsOutNothing()
        {
            // Given: the watering dialog with a listener
            var viewModel = new EditScheduleViewModel(PlantWithWateringAndSunlight(), CareType.Watering, Clock);
            bool? closeResult = null;
            viewModel.RequestClose += (_, result) => closeResult = result;

            // When: Cancel is executed
            viewModel.CancelCommand.Execute(null);

            // Then: the window closes with false and no result exists
            Assert.Equal(false, closeResult);
            Assert.Null(viewModel.Result);
        }
    }
}
