using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using System.Globalization;
using System.Windows.Data;

namespace GreenKeeper.Converters
{
    /// <summary>
    /// The plant's status as of today, for the status dot in the sidebar. PlantStatusRules
    /// picks it, StatusDotStyles.xaml sets its look.
    /// </summary>
    public class PlantStatusDotConverter : IValueConverter
    {
        private readonly TimeProvider _timeProvider;

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
