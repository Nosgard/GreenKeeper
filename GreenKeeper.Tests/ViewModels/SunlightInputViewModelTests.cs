using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.CareStatuses.EditOption;
using GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps.Passive;
using GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps.Passive;

namespace GreenKeeper.Tests.ViewModels
{
    /// <summary>
    /// Covers the three places where hours of sunlight per period are entered:
    /// the Add Plant wizard, the Add Schedule wizard and the edit dialog.
    /// </summary>
    public class SunlightInputViewModelTests
    {
        // -- Limits --

        [Theory]
        [InlineData(SunlightPeriod.Day, 24)]
        [InlineData(SunlightPeriod.Week, 168)]
        [InlineData(SunlightPeriod.Month, 744)]
        [InlineData(SunlightPeriod.Year, 8760)]
        public void MaxAmount_GivenPeriod_EqualsTheHoursOfThatPeriodEverywhere(SunlightPeriod period, int expectedMaxHours)
        {
            // Given: the three inputs with the period selected
            var wizardStep = new SunlightStepViewModel { SelectedPeriod = period };
            var scheduleStep = new ScheduleSunlightStepViewModel { SelectedPeriod = period };
            var editStep = new EditSunlightViewModel(initialHours: null, period);

            // When: the limit of each input is read
            var wizardLimit = wizardStep.MaxAmount;
            var scheduleLimit = scheduleStep.MaxAmount;
            var editLimit = editStep.MaxAmount;

            // Then: all three agree on the limit
            Assert.Equal(expectedMaxHours, wizardLimit);
            Assert.Equal(expectedMaxHours, scheduleLimit);
            Assert.Equal(expectedMaxHours, editLimit);
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("abc", false)]
        [InlineData("0", false)]
        [InlineData("1", true)]
        [InlineData("24", true)]
        [InlineData("25", false)]
        public void HasValidAmount_GivenHoursPerDay_AcceptsOnlyOneToTwentyFour(string amount, bool expected)
        {
            // Given: the wizard step with a day period
            var step = new SunlightStepViewModel { SelectedPeriod = SunlightPeriod.Day, AmountText = amount };

            // When: the validity is read
            var isValid = step.HasValidAmount;

            // Then: only whole hours within the day count
            Assert.Equal(expected, isValid);
        }

        [Fact]
        public void HasValidAmount_GivenPeriodShrinksBelowTheAmount_BecomesFalse()
        {
            // Given: 100 hours per week, which is valid
            var step = new SunlightStepViewModel { SelectedPeriod = SunlightPeriod.Week, AmountText = "100" };
            Assert.True(step.HasValidAmount);

            // When: the period is switched to a day
            step.SelectedPeriod = SunlightPeriod.Day;

            // Then: the same amount is no longer valid
            Assert.False(step.HasValidAmount);
        }

        // -- Wizard behavior --

        [Theory]
        [InlineData("", "Skip")]
        [InlineData("6", "Next")]
        public void SunlightStep_IsOptional_AndLabelsTheButtonAccordingly(string amount, string expectedLabel)
        {
            // Given: the optional step of the Add Plant wizard
            var step = new SunlightStepViewModel { AmountText = amount };

            // When: the state of the Next button is read
            var canProceed = step.CanProceed;
            var label = step.NextButtonLabel;

            // Then: it can always proceed, the label tells skip from next
            Assert.True(canProceed);
            Assert.Equal(expectedLabel, label);
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("6", true)]
        public void ScheduleSunlightStep_IsMandatory_AndAlwaysOffersFinish(string amount, bool expectedCanProceed)
        {
            // Given: the mandatory step of the Add Schedule wizard
            var step = new ScheduleSunlightStepViewModel { AmountText = amount };

            // When: the state of the Next button is read
            var canProceed = step.CanProceed;
            var label = step.NextButtonLabel;

            // Then: it proceeds only with a valid amount and always reads Finish
            Assert.Equal(expectedCanProceed, canProceed);
            Assert.Equal("Finish", label);
        }

        [Fact]
        public void SelectedPeriod_WhenChanged_RaisesPropertyChangedForTheLimit()
        {
            // Given: a step with a listener
            var step = new SunlightStepViewModel();
            var raisedProperties = new List<string>();
            step.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: the period changes
            step.SelectedPeriod = SunlightPeriod.Week;

            // Then: the view learns about the period and the new limit
            Assert.Contains(nameof(SunlightStepViewModel.SelectedPeriod), raisedProperties);
            Assert.Contains(nameof(SunlightStepViewModel.MaxAmount), raisedProperties);
        }

        // -- Edit dialog --

        [Fact]
        public void EditSunlightViewModel_GivenInitialValues_PrefillsThem()
        {
            // Given: a stored requirement of 6 hours per week
            int storedHours = 6;
            var storedPeriod = SunlightPeriod.Week;

            // When: the edit input is created for it
            var step = new EditSunlightViewModel(storedHours, storedPeriod);

            // Then: both fields are pre-filled and valid
            Assert.Equal("6", step.AmountText);
            Assert.Equal(SunlightPeriod.Week, step.SelectedPeriod);
            Assert.True(step.HasValidAmount);
        }

        [Fact]
        public void EditSunlightViewModel_GivenNoInitialHours_StartsEmpty()
        {
            // Given: no stored hours yet
            int? storedHours = null;

            // When: the edit input is created without them
            var step = new EditSunlightViewModel(storedHours, SunlightPeriod.Day);

            // Then: the field is empty and not valid yet
            Assert.Equal(string.Empty, step.AmountText);
            Assert.False(step.HasValidAmount);
        }
    }
}
