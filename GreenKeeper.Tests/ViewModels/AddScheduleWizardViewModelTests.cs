using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using GreenKeeper.Tests.Fakes;
using GreenKeeper.ViewModels.Wizards.AddScheduleWizard;
using GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps.Active;
using GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps;
using GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps.Passive;

namespace GreenKeeper.Tests.ViewModels
{
    public class AddScheduleWizardViewModelTests
    {
        private static Plant PlantWithWatering()
        {
            var plant = new Plant { Name = "Aloe Vera" };
            plant.CareSchedules.Add(new CareSchedule { Care = CareType.Watering, IntervalAmount = 7, IntervalUnit = TimeUnit.Days });
            return plant;
        }

        // Moves the wizard past the selection step for the given care type.
        private static AddScheduleWizardViewModel WizardOnDetailStep(Plant plant, CareType careType, FakeDialogService? dialogService = null)
        {
            var wizard = new AddScheduleWizardViewModel(plant, dialogService ?? new FakeDialogService());
            ((CareTypeSelectionStepViewModel)wizard.CurrentStep).SelectedCareType = careType;
            wizard.NextCommand.Execute(null);
            return wizard;
        }

        // -- Navigation Tests --

        [Fact]
        public void Constructor_StartsOnTheSelectionStep()
        {
            // Given: a plant that is watered every 7 days
            var plant = PlantWithWatering();

            // When: a fresh wizard is opened for it
            var wizard = new AddScheduleWizardViewModel(plant, new FakeDialogService());

            // Then: the care type is chosen first, Next is open and Back is not
            var selection = Assert.IsType<CareTypeSelectionStepViewModel>(wizard.CurrentStep);
            Assert.Equal(CareType.Watering, selection.SelectedCareType);
            Assert.True(wizard.NextCommand.CanExecute(null));
            Assert.False(wizard.BackCommand.CanExecute(null));
        }

        [Theory]
        [InlineData(CareType.Watering, "Watering")]
        [InlineData(CareType.Fertilizing, "Fertilizing")]
        public void NextCommand_GivenActiveCareType_OpensTheActiveStepWithItsTitle(CareType careType, string expectedTitle)
        {
            // Given: a plant that is watered every 7 days
            var plant = PlantWithWatering();

            // When: the wizard moves past the selection of an active care type
            var wizard = WizardOnDetailStep(plant, careType);

            // Then: the active step is shown, titled after the care type, and Finish waits for a valid amount
            var step = Assert.IsType<ScheduleActiveStepViewModel>(wizard.CurrentStep);
            Assert.Equal(expectedTitle, step.Title);
            Assert.Equal("Finish", step.NextButtonLabel);
            Assert.False(wizard.NextCommand.CanExecute(null));
            Assert.True(wizard.BackCommand.CanExecute(null));
        }

        [Fact]
        public void NextCommand_GivenSunlight_OpensTheSunlightStep()
        {
            // Given: a plant that is watered every 7 days
            var plant = PlantWithWatering();

            // When: the wizard moves past the selection of sunlight
            var wizard = WizardOnDetailStep(plant, CareType.Sunlight);

            // Then: the sunlight step is shown and Finish waits for a valid amount
            var step = Assert.IsType<ScheduleSunlightStepViewModel>(wizard.CurrentStep);
            Assert.Equal("Finish", step.NextButtonLabel);
            Assert.False(wizard.NextCommand.CanExecute(null));
        }

        [Fact]
        public void BackCommand_OnTheDetailStep_ReturnsToTheSelection()
        {
            // Given: a wizard on the fertilizing step
            var wizard = WizardOnDetailStep(PlantWithWatering(), CareType.Fertilizing);

            // When: Back is executed
            wizard.BackCommand.Execute(null);

            // Then: the selection step is shown again with the choice kept
            var selection = Assert.IsType<CareTypeSelectionStepViewModel>(wizard.CurrentStep);
            Assert.Equal(CareType.Fertilizing, selection.SelectedCareType);
        }

        // -- Finish Tests --

        [Fact]
        public void Finish_GivenNewFertilizingSchedule_CreatesItWithoutDueDateAndClosesWithTrue()
        {
            // Given: a plant without fertilizing and the wizard on the fertilizing step with 14 days entered
            var wizard = WizardOnDetailStep(PlantWithWatering(), CareType.Fertilizing);
            ((ScheduleActiveStepViewModel)wizard.CurrentStep).AmountText = "14";
            bool? closeResult = null;
            wizard.RequestClose += (_, result) => closeResult = result;

            // When: Finish is executed
            wizard.NextCommand.Execute(null);

            // Then: the schedule is handed out with the entered interval, the due date is left to MainViewModel
            Assert.Equal(true, closeResult);
            var schedule = Assert.IsType<CareScheduleInput>(wizard.Result).CareSchedule;
            Assert.Equal(CareType.Fertilizing, schedule.Care);
            Assert.Equal(14, schedule.IntervalAmount);
            Assert.Equal(TimeUnit.Days, schedule.IntervalUnit);
            Assert.Null(schedule.NextDueAt);
        }

        [Fact]
        public void Finish_GivenNewSunlightRequirement_CreatesItAndClosesWithTrue()
        {
            // Given: a plant without sunlight and the wizard on the sunlight step with 6 hours per week entered
            var wizard = WizardOnDetailStep(PlantWithWatering(), CareType.Sunlight);
            var step = (ScheduleSunlightStepViewModel)wizard.CurrentStep;
            step.AmountText = "6";
            step.SelectedPeriod = SunlightPeriod.Week;
            bool? closeResult = null;
            wizard.RequestClose += (_, result) => closeResult = result;

            // When: Finish is executed
            wizard.NextCommand.Execute(null);

            // Then: the requirement is handed out and the window closes with true
            Assert.Equal(true, closeResult);
            var requirement = Assert.IsType<SunlightRequirementInput>(wizard.Result).SunlightRequirement;
            Assert.Equal(6, requirement.Hours);
            Assert.Equal(SunlightPeriod.Week, requirement.Period);
        }

        [Fact]
        public void Finish_GivenExistingScheduleAndUserDeclines_KeepsTheWizardOpen()
        {
            // Given: a plant that already has a watering schedule, the user declining to replace it
            var dialogService = new FakeDialogService { ConfirmResult = false };
            var wizard = WizardOnDetailStep(PlantWithWatering(), CareType.Watering, dialogService);
            ((ScheduleActiveStepViewModel)wizard.CurrentStep).AmountText = "3";
            bool closeWasRequested = false;
            wizard.RequestClose += (_, _) => closeWasRequested = true;

            // When: Finish is executed
            wizard.NextCommand.Execute(null);

            // Then: the user was asked, nothing was created and the wizard stays open
            Assert.True(dialogService.ConfirmWasCalled);
            Assert.Null(wizard.Result);
            Assert.False(closeWasRequested);
        }

        [Fact]
        public void Finish_GivenExistingScheduleAndUserConfirms_CreatesTheReplacement()
        {
            // Given: a plant that already has a watering schedule, the user confirming the replacement
            var dialogService = new FakeDialogService { ConfirmResult = true };
            var wizard = WizardOnDetailStep(PlantWithWatering(), CareType.Watering, dialogService);
            ((ScheduleActiveStepViewModel)wizard.CurrentStep).AmountText = "3";

            // When: Finish is executed
            wizard.NextCommand.Execute(null);

            // Then: the replacement is handed out
            Assert.True(dialogService.ConfirmWasCalled);
            Assert.Equal(3, Assert.IsType<CareScheduleInput>(wizard.Result).CareSchedule.IntervalAmount);
        }

        [Fact]
        public void Finish_GivenExistingSunlightRequirementAndUserDeclines_KeepsTheWizardOpen()
        {
            // Given: a plant that already has a sunlight requirement, the user declining to replace it
            var plant = PlantWithWatering();
            plant.SunlightRequirement = new SunlightRequirement { Hours = 6, Period = SunlightPeriod.Day };
            var dialogService = new FakeDialogService { ConfirmResult = false };
            var wizard = WizardOnDetailStep(plant, CareType.Sunlight, dialogService);
            ((ScheduleSunlightStepViewModel)wizard.CurrentStep).AmountText = "8";
            bool closeWasRequested = false;
            wizard.RequestClose += (_, _) => closeWasRequested = true;

            // When: Finish is executed
            wizard.NextCommand.Execute(null);

            // Then: the user was asked, nothing was created and the wizard stays open
            Assert.True(dialogService.ConfirmWasCalled);
            Assert.Null(wizard.Result);
            Assert.False(closeWasRequested);
        }

        [Fact]
        public void Finish_GivenNoExistingEntry_DoesNotAskTheUser()
        {
            // Given: a plant without fertilizing and the wizard on the fertilizing step
            var dialogService = new FakeDialogService();
            var wizard = WizardOnDetailStep(PlantWithWatering(), CareType.Fertilizing, dialogService);
            ((ScheduleActiveStepViewModel)wizard.CurrentStep).AmountText = "14";

            // When: Finish is executed
            wizard.NextCommand.Execute(null);

            // Then: there was nothing to overwrite, so no question was asked
            Assert.False(dialogService.ConfirmWasCalled);
        }

        [Fact]
        public void CancelCommand_ClosesWithFalseAndCreatesNothing()
        {
            // Given: a wizard with a listener
            var wizard = new AddScheduleWizardViewModel(PlantWithWatering(), new FakeDialogService());
            bool? closeResult = null;
            wizard.RequestClose += (_, result) => closeResult = result;

            // When: Cancel is executed
            wizard.CancelCommand.Execute(null);

            // Then: the window closes with false and nothing was created
            Assert.Equal(false, closeResult);
            Assert.Null(wizard.Result);
        }
    }
}
