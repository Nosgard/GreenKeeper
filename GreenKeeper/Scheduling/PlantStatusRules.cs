using GreenKeeper.Models;
using GreenKeeper.Models.Enums;

namespace GreenKeeper.Scheduling
{
    /// <summary>
    /// The status of a plant from its Watering and Fertilizing schedules; Sunlight has
    /// no due date. Overdue wins over due today, due today over up to date. The status
    /// dot and the name a screen reader announces both follow it.
    /// </summary>
    public static class PlantStatusRules
    {
        public static PlantStatus For(Plant plant, DateTime today)
        {
            var dueDates = plant.CareSchedules
                .Where(s => s.Care == CareType.Watering || s.Care == CareType.Fertilizing)
                .Where(s => s.NextDueAt.HasValue)
                .Select(s => s.NextDueAt!.Value.Date)
                .ToList();

            if (dueDates.Any(due => DueDateRules.IsOverdue(due, today)))
            {
                return PlantStatus.Overdue;
            }

            if (dueDates.Any(due => DueDateRules.IsDueToday(due, today)))
            {
                return PlantStatus.DueToday;
            }

            return PlantStatus.UpToDate;
        }
    }
}
