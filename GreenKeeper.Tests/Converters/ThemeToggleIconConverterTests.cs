using GreenKeeper.Converters;
using System.Globalization;

namespace GreenKeeper.Tests.Converters
{
    public class ThemeToggleIconConverterTests
    {
        /// <summary>
        /// The button shows where a click leads: the sun in the dark theme, the moon in the
        /// bright one.
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
