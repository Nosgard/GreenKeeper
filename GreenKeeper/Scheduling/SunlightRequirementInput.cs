using GreenKeeper.Models;

namespace GreenKeeper.Scheduling
{
    public sealed class SunlightRequirementInput : ScheduleInput
    {
        public SunlightRequirement SunlightRequirement { get; }

        public SunlightRequirementInput(SunlightRequirement sunlightRequirement)
        {
            SunlightRequirement = sunlightRequirement;
        }

        public override string ExistingEntryName => "sunlight requirement";

        public override string OverwriteTitle => "Sunlight requirement already exists";

        public override bool ExistsOn(Plant plant)
        {
            return plant.SunlightRequirement != null;
        }

        public override Task SaveAsync(IPlantCareEditor editor)
        {
            return editor.AddOrReplaceSunlightRequirementAsync(SunlightRequirement);
        }
    }
}
