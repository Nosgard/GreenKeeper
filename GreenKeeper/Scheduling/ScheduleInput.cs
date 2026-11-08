using GreenKeeper.Models;

namespace GreenKeeper.Scheduling
{
    /// <summary>
    /// What the Add Schedule wizard or the edit dialog hands back: a new care schedule
    /// or a new sunlight requirement. Callers do not have to ask which kind they hold,
    /// because each kind knows whether the plant already has one and how it is saved.
    /// </summary>
    public abstract class ScheduleInput
    {
        // Fills the question "There is already a ... for this plant.", hence the lower case.
        public abstract string ExistingEntryName { get; }

        public abstract string OverwriteTitle { get; }

        public string OverwriteQuestion => $"There is already a {ExistingEntryName} for this plant. Do you want to replace it?";

        public abstract bool ExistsOn(Plant plant);

        public abstract Task SaveAsync(IPlantCareEditor editor);
    }
}
