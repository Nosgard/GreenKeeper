using GreenKeeper.Models;

namespace GreenKeeper.Scheduling
{
    /// <summary>
    /// What the Add Schedule wizard or the edit dialog hands back: either a new
    /// care schedule or a new sunlight requirement for the plant. Each kind knows
    /// whether the plant already has one of its sort and where it is saved, so
    /// the callers never have to ask which kind they are holding.
    /// </summary>
    public abstract class ScheduleInput
    {
        // Fills the question "There is already a ... for this plant." - hence the casing.
        public abstract string ExistingEntryName { get; }

        public abstract string OverwriteTitle { get; }

        public string OverwriteQuestion => $"There is already a {ExistingEntryName} for this plant. Do you want to replace it?";

        public abstract bool ExistsOn(Plant plant);

        public abstract Task SaveAsync(IPlantCareEditor editor);
    }
}
