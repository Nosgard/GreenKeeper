using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using System.Globalization;
using System.Windows.Data;

namespace GreenKeeper.Converters
{
    /// <summary>
    /// Determines which status dot icon to show next to a plant in the
    /// sidebar, based on the most urgent state among its Watering and
    /// Fertilizing schedules (Sunlight is irrelevant, since it has no
    /// due date). Overdue takes priority over "due today", which in turn
    /// takes priority over the default green state.
    /// </summary>
    public class PlantStatusDotConverter : IValueConverter
    {
        private const string GreenDot = "/Resources/Icons/Dots/GreenDot.png";
        private const string YellowDot = "/Resources/Icons/Dots/YellowDot.png";
        private const string RedDot = "/Resources/Icons/Dots/RedDot.png";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Plant plant)
            {
                return GreenDot;
            }

            var dueDates = plant.CareSchedules
                .Where(s => s.Care == CareType.Watering || s.Care == CareType.Fertilizing)
                .Where(s => s.NextDueAt.HasValue)
                .Select(s => s.NextDueAt!.Value.Date)
                .ToList();

            // A converter is created by XAML, so there is no clock to inject here.
            var today = DateTime.Now;

            if (dueDates.Any(due => DueDateRules.IsOverdue(due, today)))
            {
                return RedDot;
            }

            if (dueDates.Any(due => DueDateRules.IsDueToday(due, today)))
            {
                return YellowDot;
            }

            return GreenDot;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
