using GreenKeeper.Models;

namespace GreenKeeper.Tests.Models
{
    public class PlantTests
    {
        /// <summary>
        /// The Add Plant wizard and the rename dialog both follow this rule, so the
        /// limit is checked once here instead of in each of them.
        /// </summary>
        [Theory]
        [InlineData(1, true)]
        [InlineData(50, true)]
        [InlineData(51, false)]
        public void IsValidName_GivenNameOfLength_AcceptsUpToFiftyCharacters(int length, bool expected)
        {
            // Given: a name with the given number of characters
            var name = new string('a', length);

            // When: the name is checked
            var isValid = Plant.IsValidName(name);

            // Then: 50 characters are the longest name that counts
            Assert.Equal(expected, isValid);
        }
    }
}
