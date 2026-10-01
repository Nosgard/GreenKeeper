using GreenKeeper.Models;
using GreenKeeper.Models.Enums;

namespace GreenKeeper.Tests
{
    /// <summary>
    /// Builders for the plants the tests work with, so a test states only what
    /// matters to it: "Aloe Vera, watered every 7 days" instead of the whole object graph.
    /// </summary>
    internal static class TestPlants
    {
        public static Plant AloeVera(params CareSchedule[] schedules) => PlantNamed("Aloe Vera", schedules);

        public static Plant PlantNamed(string name, params CareSchedule[] schedules)
        {
            var plant = new Plant { Name = name };
            plant.CareSchedules.AddRange(schedules);
            return plant;
        }

        public static CareSchedule WateringEveryDays(int days, DateTime? nextDueAt = null) =>
            new() { Care = CareType.Watering, IntervalAmount = days, IntervalUnit = TimeUnit.Days, NextDueAt = nextDueAt };

        public static CareSchedule FertilizingEveryDays(int days, DateTime? nextDueAt = null) =>
            new() { Care = CareType.Fertilizing, IntervalAmount = days, IntervalUnit = TimeUnit.Days, NextDueAt = nextDueAt };

        public static SunlightRequirement DailySunlight(int hours) =>
            new() { Hours = hours, Period = SunlightPeriod.Day };
    }
}
