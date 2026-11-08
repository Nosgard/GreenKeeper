using System.Globalization;
using System.Windows.Data;

namespace GreenKeeper.Converters
{
    /// <summary>
    /// The theme toggle shows where a click leads: a sun in the dark theme, a moon in the
    /// bright one. The view uses only the shape of the image (see MainWindow.xaml).
    /// </summary>
    public class ThemeToggleIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isDarkTheme = value is bool b && b;

            return isDarkTheme
                ? "/Resources/Icons/Themes/SunIcon.png"
                : "/Resources/Icons/Themes/MoonIcon.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
