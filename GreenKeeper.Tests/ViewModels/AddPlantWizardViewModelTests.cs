using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.Wizards.AddPlantWizard;
using GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps;
using GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps.Active;
using GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps.Passive;

namespace GreenKeeper.Tests.ViewModels
{
    public class AddPlantWizardViewModelTests
    {
        // Walks the wizard up to the summary step with a valid name and watering
        // interval, so a test only has to fill in what it is about.
        private static AddPlantWizardViewModel WizardAtSummary(string fertilizingAmount = "", string sunlightHours = "")
        {
            var wizard = new AddPlantWizardViewModel();

            ((PlantNameStepViewModel)wizard.CurrentStep).PlantName = "Aloe Vera";
            wizard.NextCommand.Execute(null);

            ((WateringStepViewModel)wizard.CurrentStep).AmountText = "7";
            wizard.NextCommand.Execute(null);

            ((FertilizingStepViewModel)wizard.CurrentStep).AmountText = fertilizingAmount;
            wizard.NextCommand.Execute(null);

            ((SunlightStepViewModel)wizard.CurrentStep).AmountText = sunlightHours;
            wizard.NextCommand.Execute(null);

            return wizard;
        }

        // -- Navigation Tests --

        [Fact]
        public void Constructor_StartsOnThePlantNameStepWithoutAWayBack()
        {
            // Given: no wizard has been opened yet

            // When: a fresh wizard is created
            var wizard = new AddPlantWizardViewModel();

            // Then: the first step is the name, and Back is not available yet
            Assert.IsType<PlantNameStepViewModel>(wizard.CurrentStep);
            Assert.False(wizard.BackCommand.CanExecute(null));
        }

        [Fact]
        public void NextCommand_GivenEmptyPlantName_CannotExecute()
        {
            // Given: a wizard whose name step is still empty
            var wizard = new AddPlantWizardViewModel();

            // When: the enabled state of Next is read
            var canProceed = wizard.NextCommand.CanExecute(null);

            // Then: the mandatory name blocks the way forward
            Assert.False(canProceed);
        }

        [Fact]
        public void NextCommand_VisitsTheStepsInOrder()
        {
            // Given: a wizard with a valid name and watering interval
            var wizard = new AddPlantWizardViewModel();
            ((PlantNameStepViewModel)wizard.CurrentStep).PlantName = "Aloe Vera";

            // When: Next is executed step by step
            wizard.NextCommand.Execute(null);
            var second = wizard.CurrentStep;
            ((WateringStepViewModel)second).AmountText = "7";
            wizard.NextCommand.Execute(null);
            var third = wizard.CurrentStep;
            wizard.NextCommand.Execute(null);
            var fourth = wizard.CurrentStep;
            wizard.NextCommand.Execute(null);
            var fifth = wizard.CurrentStep;

            // Then: name, watering, fertilizing, sunlight, summary
            Assert.IsType<WateringStepViewModel>(second);
            Assert.IsType<FertilizingStepViewModel>(third);
            Assert.IsType<SunlightStepViewModel>(fourth);
            Assert.IsType<SummaryStepViewModel>(fifth);
        }

        [Fact]
        public void BackCommand_GivenSecondStep_ReturnsToThePlantNameStep()
        {
            // Given: a wizard on the watering step
            var wizard = new AddPlantWizardViewModel();
            ((PlantNameStepViewModel)wizard.CurrentStep).PlantName = "Aloe Vera";
            wizard.NextCommand.Execute(null);

            // When: Back is executed
            wizard.BackCommand.Execute(null);

            // Then: the name step is shown again, with its value kept
            var nameStep = Assert.IsType<PlantNameStepViewModel>(wizard.CurrentStep);
            Assert.Equal("Aloe Vera", nameStep.PlantName);
        }

        [Fact]
        public void PlantNameStep_GivenTypedName_CountsDownTheRemainingCharacters()
        {
            // Given: the name step with a listener
            var step = new PlantNameStepViewModel();
            var raisedProperties = new List<string>();
            step.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: a name of 9 characters is typed
            step.PlantName = "Aloe Vera";

            // Then: 41 of the 50 characters are left, and the view learns about it
            Assert.Equal(41, step.CharactersRemaining);
            Assert.Contains(nameof(PlantNameStepViewModel.CharactersRemaining), raisedProperties);
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("7", true)]
        public void WateringStep_IsMandatory_AndAlwaysOffersNext(string amount, bool expectedCanProceed)
        {
            // Given: the mandatory watering step
            var step = new WateringStepViewModel { AmountText = amount };

            // When: the state of the Next button is read
            var canProceed = step.CanProceed;
            var label = step.NextButtonLabel;

            // Then: it proceeds only with a valid amount and never offers to skip
            Assert.Equal(expectedCanProceed, canProceed);
            Assert.Equal("Next", label);
        }

        [Theory]
        [InlineData("", "Skip")]
        [InlineData("30", "Next")]
        public void FertilizingStep_NextButtonLabel_DependsOnWhetherAnAmountWasEntered(string amount, string expectedLabel)
        {
            // Given: the optional fertilizing step
            var step = new FertilizingStepViewModel { AmountText = amount };

            // When: the label of the Next button is read
            var label = step.NextButtonLabel;

            // Then: an empty step offers to skip, a filled one to continue
            Assert.Equal(expectedLabel, label);
            Assert.True(step.CanProceed);
        }

        [Fact]
        public void CurrentStep_WhenChanged_RaisesPropertyChanged()
        {
            // Given: a wizard with a listener and a valid name
            var wizard = new AddPlantWizardViewModel();
            ((PlantNameStepViewModel)wizard.CurrentStep).PlantName = "Aloe Vera";
            var raisedProperties = new List<string>();
            wizard.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: the wizard moves on
            wizard.NextCommand.Execute(null);

            // Then: the view learns that the step changed
            Assert.Contains(nameof(AddPlantWizardViewModel.CurrentStep), raisedProperties);
        }

        // -- Finish Tests --

        [Fact]
        public void NextCommand_OnTheSummaryStep_BuildsThePlantAndClosesWithTrue()
        {
            // Given: a wizard on the summary step with a name and a 7-day watering interval
            var wizard = WizardAtSummary();
            bool? closeResult = null;
            wizard.RequestClose += (_, result) => closeResult = result;

            // When: Finish is executed
            wizard.NextCommand.Execute(null);

            // Then: the plant carries the name and a 7-day watering interval, and the window closes with true
            Assert.Equal(true, closeResult);
            var plant = wizard.CreatedPlant;
            Assert.NotNull(plant);
            Assert.Equal("Aloe Vera", plant.Name);

            var watering = Assert.Single(plant.CareSchedules);
            Assert.Equal(CareType.Watering, watering.Care);
            Assert.Equal(7, watering.IntervalAmount);
            Assert.Equal(TimeUnit.Days, watering.IntervalUnit);
            Assert.Null(watering.NextDueAt);
            Assert.Null(plant.SunlightRequirement);
        }

        [Fact]
        public void NextCommand_OnTheSummaryStep_GivenFertilizingAndSunlight_AddsBoth()
        {
            // Given: a wizard on the summary step with fertilizing every 30 days and 6 hours of sunlight
            var wizard = WizardAtSummary(fertilizingAmount: "30", sunlightHours: "6");

            // When: Finish is executed
            wizard.NextCommand.Execute(null);

            // Then: the plant has both optional entries
            var plant = wizard.CreatedPlant!;
            var fertilizing = Assert.Single(plant.CareSchedules, s => s.Care == CareType.Fertilizing);
            Assert.Equal(30, fertilizing.IntervalAmount);

            Assert.NotNull(plant.SunlightRequirement);
            Assert.Equal(6, plant.SunlightRequirement.Hours);
            Assert.Equal(SunlightPeriod.Day, plant.SunlightRequirement.Period);
        }

        [Fact]
        public void SummaryStep_ReflectsTheValuesEnteredBefore()
        {
            // Given: a wizard on the summary step with fertilizing and sunlight entered
            var wizard = WizardAtSummary(fertilizingAmount: "30", sunlightHours: "6");

            // When: the summary step is read
            var summary = (SummaryStepViewModel)wizard.CurrentStep;

            // Then: it repeats the entered values in words
            Assert.Equal("Aloe Vera", summary.PlantName);
            Assert.Equal("7 Days", summary.Watering);
            Assert.True(summary.HasFertilizing);
            Assert.Equal("30 Days", summary.Fertilizing);
            Assert.True(summary.HasSunlight);
            Assert.Equal("6 Hours / Day", summary.Sunlight);
            Assert.Equal("Finish", summary.NextButtonLabel);
        }

        [Fact]
        public void SummaryStep_GivenValueChangedAfterGoingBack_ShowsTheNewValue()
        {
            // Given: a wizard that went back from the summary to the sunlight step and changed the hours
            var wizard = WizardAtSummary(sunlightHours: "6");
            wizard.BackCommand.Execute(null);
            ((SunlightStepViewModel)wizard.CurrentStep).AmountText = "8";

            // When: the summary is reached again
            wizard.NextCommand.Execute(null);

            // Then: it shows the changed value
            var summary = Assert.IsType<SummaryStepViewModel>(wizard.CurrentStep);
            Assert.Equal("8 Hours / Day", summary.Sunlight);
        }

        [Fact]
        public void CancelCommand_ClosesWithFalseAndCreatesNoPlant()
        {
            // Given: a wizard with a listener
            var wizard = new AddPlantWizardViewModel();
            bool? closeResult = null;
            wizard.RequestClose += (_, result) => closeResult = result;

            // When: Cancel is executed
            wizard.CancelCommand.Execute(null);

            // Then: the window closes with false and nothing was built
            Assert.Equal(false, closeResult);
            Assert.Null(wizard.CreatedPlant);
        }
    }
}
