using GreenKeeper.Views;

namespace GreenKeeper.Tests.Views
{
    /// <summary>
    /// What an amount field keeps of a text: a whole number as written, and no more
    /// digits than the field takes.
    /// </summary>
    public class WholeNumberInputTests
    {
        [Theory]
        [InlineData("12a", "12")]
        [InlineData("abc", "")]
        [InlineData(" 365 ", "365")]
        [InlineData("3 6", "36")]
        [InlineData("-5", "5")]
        public void Clean_GivenOtherCharacters_KeepsOnlyTheDigits(string text, string expected)
        {
            // Given: a field that takes up to four digits, as the amount fields do
            // (AmountAndUnitInputViewModel.MaxAmountLength)
            int maxDigits = 4;

            // When: the text is cleaned
            var cleaned = WholeNumberInput.Clean(text, maxDigits);

            // Then: only the digits are left
            Assert.Equal(expected, cleaned);
        }

        [Theory]
        [InlineData("0365", "365")]
        [InlineData("007", "7")]
        [InlineData("0", "")]        // no real digit at all, so nothing is left
        [InlineData("100", "100")]
        public void Clean_GivenLeadingZeros_RemovesThem(string text, string expected)
        {
            // Given: a field that takes up to four digits, as the amount fields do
            // (AmountAndUnitInputViewModel.MaxAmountLength)
            int maxDigits = 4;

            // When: the text is cleaned
            var cleaned = WholeNumberInput.Clean(text, maxDigits);

            // Then: the number starts with its first real digit, zeros inside it stay
            Assert.Equal(expected, cleaned);
        }

        [Theory]
        [InlineData("1000", "1000")]
        [InlineData("10000", "1000")]
        [InlineData("000012345", "1234")]
        public void Clean_GivenMoreDigitsThanTheFieldTakes_CutsTheRest(string text, string expected)
        {
            // Given: a field that takes up to four digits, as the amount fields do
            // (AmountAndUnitInputViewModel.MaxAmountLength)
            int maxDigits = 4;

            // When: the text is cleaned
            var cleaned = WholeNumberInput.Clean(text, maxDigits);

            // Then: zeros in front do not count, and what goes beyond four digits is cut
            Assert.Equal(expected, cleaned);
        }
    }
}
