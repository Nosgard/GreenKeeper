using GreenKeeper.Models;

namespace GreenKeeper.Scheduling
{
    public static class CareScheduleExtensions
    {
        // Whether the interval is complete enough to calculate a due date from.
        public static bool HasInterval(this CareSchedule schedule)
        {
            return schedule.IntervalAmount != null && schedule.IntervalUnit != null;
        }

        /// <summary>
        /// The due date one interval after the given moment. Requires HasInterval.
        /// </summary>
        public static DateTime NextDueDateFrom(this CareSchedule schedule, DateTime start)
        {
            return DueDateCalculator.ToDueDate(start, schedule.IntervalAmount!.Value, schedule.IntervalUnit!.Value);
        }

        /// <summary>
        /// Starts the interval at the given moment: the care counts as done now and is due
        /// again one interval later. Requires HasInterval.
        ///
        /// PlantListViewModel calls it for each schedule of a new plant, DashboardViewModel for a
        /// schedule from the Add Schedule wizard or the edit dialog. Completing a care sets the
        /// same two dates in DashboardViewModel, but only after the database has saved them.
        /// </summary>
        public static void StartIntervalAt(this CareSchedule schedule, DateTime now)
        {
            schedule.NextDueAt = schedule.NextDueDateFrom(now);
            schedule.LastCaredAt = now;
        }
    }
}
