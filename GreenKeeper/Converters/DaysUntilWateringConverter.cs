using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GreenKeeper.Converters
{
    public class DaysUntilWateringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int days)
            {
                return string.Empty;
            }

            if (days == 0)
            {
                return "Today";
            }

            if (days < 0)
            {
                return $"Overdue for {Math.Abs(days)} days";
            }

            return $"{days} days";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
