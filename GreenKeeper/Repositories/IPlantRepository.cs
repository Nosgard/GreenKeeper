using GreenKeeper.Models;

namespace GreenKeeper.Repositories
{
    public interface IPlantRepository
    {
        /// <summary>
        /// Loads all plants with their care schedules and sunlight requirement.
        /// </summary>
        Task<List<Plant>> GetPlantsAsync();

        /// <summary>
        /// Saves a new plant together with the care schedules and the sunlight requirement
        /// attached to it, and returns it with the Id the database generated.
        /// </summary>
        Task<Plant> AddPlantAsync(Plant plant);

        /// <summary>
        /// Saves a completed care schedule: the next due date and the date of the last care.
        /// DashboardViewModel calculates both; the repository only stores them.
        /// </summary>
        Task CompleteCareScheduleAsync(int careScheduleId, DateTime nextDueAt, DateTime lastCaredAt);

        /// <summary>
        /// Deletes the plant with this Id. The database removes its care schedules and its
        /// sunlight requirement through ON DELETE CASCADE (see GreenKeeperDbContext.OnModelCreating).
        /// </summary>
        Task DeletePlantAsync(int plantId);

        /// <summary>
        /// Adds the care schedule to the plant, or replaces the plant's schedule of the same
        /// care type. The caller sets NextDueAt and LastCaredAt beforehand.
        /// </summary>
        Task<CareSchedule> AddOrReplaceCareScheduleAsync(int plantId, CareSchedule careSchedule);

        /// <summary>
        /// Adds the sunlight requirement to the plant, or replaces the plant's existing one.
        /// </summary>
        Task<SunlightRequirement> AddOrReplaceSunlightRequirementAsync(int plantId, SunlightRequirement sunlightRequirement);

        /// <summary>
        /// Callers remove only optional schedules (Fertilizing); watering is mandatory.
        /// </summary>
        Task RemoveCareScheduleAsync(int careScheduleId);

        Task RemoveSunlightRequirementAsync(int sunlightRequirementId);

        Task UpdatePlantNotesAsync(int plantId, string notes);

        /// <summary>
        /// Saves a new name for the plant. Only the name column changes.
        /// </summary>
        Task RenamePlantAsync(int plantId, string newName);
    }
}
