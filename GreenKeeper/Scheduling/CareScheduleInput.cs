using GreenKeeper.Models;
using GreenKeeper.Models.Enums;

namespace GreenKeeper.Scheduling
{
    public sealed class CareScheduleInput : ScheduleInput
    {
        public CareSchedule CareSchedule { get; }

        public CareScheduleInput(CareSchedule careSchedule)
        {
            CareSchedule = careSchedule;
        }

        public override string ExistingEntryName => $"{CareSchedule.Care.DisplayName()} schedule";

        public override string OverwriteTitle => "Schedule already exists";

        public override bool ExistsOn(Plant plant)
        {
            return plant.CareSchedules.Any(s => s.Care == CareSchedule.Care);
        }

        public override Task SaveAsync(IPlantCareEditor editor)
        {
            return editor.AddOrReplaceCareScheduleAsync(CareSchedule);
        }
    }
}
