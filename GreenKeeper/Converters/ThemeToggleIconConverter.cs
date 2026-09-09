using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GreenKeeper.Converters
{
    /// <summary>
    /// Picks the correct icon file for the theme toggle button based on
    /// IsDarkTheme. While the dark theme is active, a light-colored sun
    /// icon is shown (switching TO bright); while bright is active, a
    /// dark-colored moon icon is shown (switching TO dark).
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
