using GreenKeeper.Models;

namespace GreenKeeper.Repositories
{
    public interface IPlantRepository
    {
        // Loads all plants including their care schedules and
        // sunlight requirement from the database.
        Task<List<Plant>> GetPlantsAsync();

        /// <summary>
        /// Persists a new plant object including any care schedules and the sunlight requirement attached to it.
        /// Returns the same plant instance, but with its generated Id populated by the database.
        /// The caller should use this returned instance going forward, not the original one passed in,
        /// even though it's technically the same object reference here.
        /// </summary>
        Task<Plant> AddPlantAsync(Plant plant);

        /// <summary>
        /// Persists the "completed now" state of an existing care schedule:
        /// Updates its next due date (NextDueAt) to the given values.
        /// The actual calculation of these new values (via DueDateCalculator)
        /// happens in the MainViewModel. This method is a pure persistence
        /// operation. It doesn't contain any business logic about HOW the new
        /// due date is determined.
        /// </summary>
        Task CompleteCareScheduleAsync(int careScheduleId, DateTime nextDueAt, DateTime lastCaredAt);

        /// <summary>
        /// Permanently deletes a plant object identified by its Id.
        /// 
        /// Its care schedules and sunlight requirement do NOT need to be deleted
        /// separately or even loaded here - the database itself removes them
        /// automatically as soon as the plant object is deleted, thanks to the
        /// ON DELETE CASCADE foreign key behavior
        /// (for more info go to GreenKeeperDbContext.OnModelCreating).
        /// </summary>
        Task DeletePlantAsync(int plantId);

        /// <summary>
        /// Adds a new care schedule for the given plant, or replaces the existing
        /// one of the same care type if one already exists.
        /// The passed-in schedule should already have the next due date (NextDueAt)
        /// and the last date of care (LastCaredAt).
        /// </summary>
        Task<CareSchedule> AddOrReplaceCareScheduleAsync(int plantId, CareSchedule careSchedule);

        /// <summary>
        /// Adds a new sunlight requirement for the given plant,
        /// or replaces the existing one if present.
        /// </summary>
        Task<SunlightRequirement> AddOrReplaceSunlightRequirementAsync(int plantId, SunlightRequirement sunlightRequirement);

        /// <summary>
        /// Permanently deletes a single care schedule row, identified by its Id.
        /// Used for the optional schedules (Fertilizing).
        /// </summary>
        Task RemoveCareScheduleAsync(int careScheduleId);

        /// <summary>
        /// Permanently deletes a single sunlight requirement row, identified by
        /// its Id.
        /// </summary>
        Task RemoveSunlightRequirementAsync(int sunlightRequirementId);

        /// <summary>
        /// Persists the given text as the notes for the selected plant.
        /// </summary>
        Task UpdatePlantNotesAsync(int plantId, string notes);

        /// <summary>
        /// Persists a new name for the plant identified by plantId.
        /// 
        /// Only the name column is touched - care schedules, the sunlight requirement
        /// and all due dates remain untouched. Renaming is purely cosmetic and must
        /// never affect a plant's care state.
        /// </summary>
        Task RenamePlantAsync(int plantId, string newName);
    }
}
