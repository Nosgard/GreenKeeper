using GreenKeeper.Models;
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
    /// Determines which status dot icon to show next to a plant in the
    /// sidebar, based on the most urgent state among its Watering and
    /// Fertilizing schedules (Sunlight is irrelevant, since it has no
    /// due date). Overdue takes priority over "due today", which in turn
    /// takes priority over the default green state
    /// </summary>
    public class PlantStatusDotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Plant plant)
            {
                return "/Resources/Icons/Dots/GreenDot.png";
            }

            var relevantSchedules = plant.CareSchedules
                .Where(s => s.Care == Models.Enums.CareType.Water || s.Care == Models.Enums.CareType.Nutrients)
                .Where(s => s.NextDueAt.HasValue);

            var today = DateTime.Now.Date;

            bool anyOverdue = relevantSchedules.Any(s => s.NextDueAt!.Value.Date < today);
            if (anyOverdue)
            {
                return "/Resources/Icons/Dots/RedDot.png";
            }

            bool anyDueToday = relevantSchedules.Any(s => s.NextDueAt!.Value.Date == today);
            if (anyDueToday)
            {
                return "/Resources/Icons/Dots/YellowDot.png";
            }

            return "/Resources/Icons/Dots/GreenDot.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
