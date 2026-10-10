using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using System.Globalization;
using System.Windows.Data;

namespace GreenKeeper.Converters
{
    /// <summary>
    /// The name a screen reader announces for a plant entry in the sidebar: the shown
    /// name, then the state of its status dot, as in "Aloe Vera, overdue".
    /// </summary>
    public class PlantAccessibleNameConverter : IValueConverter
    {
        private readonly TimeProvider _timeProvider;

        // XAML uses this constructor, so the app runs on the system clock.
        public PlantAccessibleNameConverter()
            : this(TimeProvider.System)
        {
        }

        public PlantAccessibleNameConverter(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Plant plant)
            {
                return string.Empty;
            }

            string state = PlantStatusRules.For(plant, _timeProvider.GetLocalNow().DateTime) switch
            {
                PlantStatus.Overdue => "overdue",
                PlantStatus.DueToday => "due today",
                _ => "up to date"
            };

            return $"{plant.Name}, {state}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
