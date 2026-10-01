using GreenKeeper.Models;

namespace GreenKeeper.Scheduling
{
    /// <summary>
    /// Where a ScheduleInput gets saved: the two operations MainViewModel offers
    /// for the selected plant. The interface keeps the inputs unaware of the
    /// ViewModel itself.
    /// </summary>
    public interface IPlantCareEditor
    {
        Task AddOrReplaceCareScheduleAsync(CareSchedule careSchedule);

        Task AddOrReplaceSunlightRequirementAsync(SunlightRequirement sunlightRequirement);
    }
}
