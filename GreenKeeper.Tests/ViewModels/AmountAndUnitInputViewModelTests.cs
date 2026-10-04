using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.Base;

namespace GreenKeeper.Tests.ViewModels
{
    /// <summary>
    /// Covers the interval input that the wizard steps and the edit dialog share
    /// for Watering and Fertilizing: which amounts it accepts and how many digits they may have.
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

        [Theory]
        [InlineData("365", false, "max. 365")]
        [InlineData("366", true, "Too large, max. 365")]
        [InlineData("", false, "max. 365")]
        public void AmountHint_GivenAmount_NamesTheLimitAndSaysWhenItIsExceeded(string amount, bool expectedTooLarge, string expectedHint)
        {
            // Given: the amount field for days, which allows up to 365
            var input = new AmountAndUnitInputViewModel { SelectedUnit = TimeUnit.Days, AmountText = amount };

            // When: the hint below the field is read
            var isTooLarge = input.IsAmountTooLarge;
            var hint = input.AmountHint;

            // Then: it shows the limit, and says so in words once the amount exceeds it
            Assert.Equal(expectedTooLarge, isTooLarge);
            Assert.Equal(expectedHint, hint);
        }

        [Fact]
        public void SelectedUnit_GivenAmountThatExceedsTheNewLimit_RaisesPropertyChangedForTheHint()
        {
            // Given: 100 days, with the changes of the input recorded
            var input = new AmountAndUnitInputViewModel { SelectedUnit = TimeUnit.Days, AmountText = "100" };
            var raisedProperties = new List<string>();
            input.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: the unit changes to weeks, which allow up to 52
            input.SelectedUnit = TimeUnit.Weeks;

            // Then: PropertyChanged is raised for the hint and its error state, so the view shows "Too large, max. 52"
            Assert.Contains(nameof(AmountAndUnitInputViewModel.AmountHint), raisedProperties);
            Assert.Contains(nameof(AmountAndUnitInputViewModel.IsAmountTooLarge), raisedProperties);
            Assert.Equal("Too large, max. 52", input.AmountHint);
        }

        [Fact]
        public void MaxAmountLength_GivenAllUnits_IsOneDigitMoreThanTheLongestMaximum()
        {
            // Given: the longest maximum of all units, 365 days, has three digits
            const int longestMaximumDigits = 3;

            // When: the length limit of the amount field is read
            var limit = AmountAndUnitInputViewModel.MaxAmountLength;

            // Then: one digit more fits, so an amount above the limit stays visible instead
            // of being cut down to a valid one
            Assert.Equal(longestMaximumDigits + 1, limit);
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
