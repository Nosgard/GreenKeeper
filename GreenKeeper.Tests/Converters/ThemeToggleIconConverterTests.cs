using GreenKeeper.Converters;
using System.Globalization;

namespace GreenKeeper.Tests.Converters
{
    public class ThemeToggleIconConverterTests
    {
        /// <summary>
        /// The toggle button shows where a click leads, not where the app is: the sun
        /// while the dark theme is active, the moon while the bright one is.
        /// </summary>
        [Theory]
        [InlineData(true, "/Resources/Icons/Themes/SunIcon.png")]
        [InlineData(false, "/Resources/Icons/Themes/MoonIcon.png")]
        public void Convert_GivenWhetherTheDarkThemeIsActive_ReturnsTheIconOfTheOtherTheme(bool isDarkTheme, string expectedIcon)
        {
            // Given: the converter of the theme toggle button
            var converter = new ThemeToggleIconConverter();

            // When: the state of the theme is converted
            var icon = converter.Convert(isDarkTheme, typeof(object), parameter: null!, CultureInfo.InvariantCulture);

            // Then: the icon of the theme the button switches to is returned
            Assert.Equal(expectedIcon, icon);
        }
    }
}
