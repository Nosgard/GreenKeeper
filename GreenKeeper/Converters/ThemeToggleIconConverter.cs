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
    /// Picks the icon shape for the theme toggle button based on IsDarkTheme:
    /// a sun while the dark theme is active (switching TO bright), a moon while
    /// bright is active (switching TO dark). Only the shape is used - the view
    /// takes the image as an opacity mask and tints it with PrimaryText.
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
