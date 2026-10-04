using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using System.Globalization;
using System.Windows.Data;

namespace GreenKeeper.Converters
{
    /// <summary>
    /// The status dot shown next to a plant in the sidebar: the plant's status as of
    /// today (PlantStatusRules decides which state wins). StatusDotStyles.xaml decides
    /// how each status looks.
    /// </summary>
    public class PlantStatusDotConverter : IValueConverter
    {
        private readonly TimeProvider _timeProvider;

        // XAML creates the converter through this constructor, so the app judges by the system clock.
        public PlantStatusDotConverter()
            : this(TimeProvider.System)
        {
        }

        public PlantStatusDotConverter(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Plant plant)
            {
                return PlantStatus.UpToDate;
            }

            return PlantStatusRules.For(plant, _timeProvider.GetLocalNow().DateTime);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
