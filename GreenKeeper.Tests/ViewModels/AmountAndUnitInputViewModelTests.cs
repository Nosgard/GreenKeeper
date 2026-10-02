using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.Base;

namespace GreenKeeper.Tests.ViewModels
{
    /// <summary>
    /// Covers the interval input that the wizard steps and the edit dialog share
    /// for Watering and Fertilizing: which amounts it accepts.
    /// </summary>
    public class AmountAndUnitInputViewModelTests
    {
        [Theory]
        [InlineData("", false)]
        [InlineData("abc", false)]
        [InlineData("1.5", false)]
        [InlineData("-1", false)]
        [InlineData("0", false)]
        [InlineData("1", true)]
        public void HasValidAmount_GivenAmount_AcceptsOnlyPositiveWholeNumbers(string amount, bool expected)
        {
            // Given: the input with the amount entered
            var input = new AmountAndUnitInputViewModel { AmountText = amount };

            // When: the validity is read
            var isValid = input.HasValidAmount;

            // Then: only a whole number above zero counts
            Assert.Equal(expected, isValid);
        }

        [Theory]
        [InlineData(TimeUnit.Days, "365", true)]
        [InlineData(TimeUnit.Days, "366", false)]
        [InlineData(TimeUnit.Weeks, "52", true)]
        [InlineData(TimeUnit.Weeks, "53", false)]
        [InlineData(TimeUnit.Months, "24", true)]
        [InlineData(TimeUnit.Months, "25", false)]
        [InlineData(TimeUnit.Years, "10", true)]
        [InlineData(TimeUnit.Years, "11", false)]
        public void HasValidAmount_GivenUnit_AcceptsAmountsUpToTheMaximumOfThatUnit(TimeUnit unit, string amount, bool expected)
        {
            // Given: the input with the unit selected and the amount entered
            var input = new AmountAndUnitInputViewModel { SelectedUnit = unit, AmountText = amount };

            // When: the validity is read
            var isValid = input.HasValidAmount;

            // Then: the maximum of the unit is the last amount that counts
            Assert.Equal(expected, isValid);
        }

        [Fact]
        public void SelectedUnit_WhenChanged_RaisesPropertyChangedForTheLimit()
        {
            // Given: an input with a listener
            var input = new AmountAndUnitInputViewModel();
            var raisedProperties = new List<string>();
            input.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: the unit changes
            input.SelectedUnit = TimeUnit.Weeks;

            // Then: the view learns about the unit and the new limit
            Assert.Contains(nameof(AmountAndUnitInputViewModel.SelectedUnit), raisedProperties);
            Assert.Contains(nameof(AmountAndUnitInputViewModel.MaxAmount), raisedProperties);
        }
    }
}
