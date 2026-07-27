using GreenKeeper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Repositories
{
    public interface IPlantRepository
    {
        // Loads all plants including their Care-Schedules and
        // Sunlight-Requirement from the database.
        Task<List<Plant>> GetPlantsAsync();

        /// <summary>
        /// Persists a new Plant-Object including any Care-Schedules and the Sunlight-Requirement attached to it.
        /// Returns the same Plant-Instance, but with it's generated Id populated by the database.
        /// The caller should use this returned instance going forward, not the original one passed in,
        /// even thought it's technically the same object reference here
        /// </summary>
        Task<Plant> AddPlantAsync(Plant plant);

        /// <summary>
        /// Persists the "completed now" state of an existing Care-Schedule:
        /// Updates it's next due date (NextDueAt) to the given values.
        /// The actual calculation of these new values (via TimeUnitConverter)
        /// happens in the MainViewModel. This method is a pure persistence
        /// operation. It doesn't contain any business logic about HOW the new
        /// due date is determined
        /// </summary>
        Task CompleteCareScheduleAsync(int careScheduleId, DateTime nextDueAt, DateTime lastCaredAt);

        /// <summary>
        /// Permanently deletes a Plant-Object identified by it's Id.
        /// 
        /// It's Care-Schedules and Sunlight-Requirement do NOT need to be deleted
        /// separately or even loaded here - the database itself removes them
        /// automatically as soon as the Plant-Object is deleted, thanks to the
        /// ON DELETE CASCADE foreign key behavior
        /// (for more info go to GreenKeeperDbContext.OnModelCreating)
        /// </summary>
        Task DeletePlantAsync(int plantId);

        /// <summary>
        /// Adds a new Care-Schedule for the given plant, or replaces the existing
        /// one of the same Care-Type if one already exists.
        /// The passed-in schedule should already have the next due date (NextDueAt)
        /// and the last date of care (LastCaredAt)
        /// </summary>
        Task<CareSchedule> AddOrReplaceCareScheduleAsync(int plantId, CareSchedule careSchedule);

        /// <summary>
        /// Adds a new Sunlight-Requirement for the given plant,
        /// or replaces the existing one if present
        /// </summary>
        Task<SunlightRequirement> AddOrReplaceSunlightRequirementAsync(int plantId, SunlightRequirement sunlightRequirement);

        /// <summary>
        /// Permanently deletes a single Care-Schedule row, identified by it's Id.
        /// Used for the optional schedules (Fertilizing)
        /// </summary>
        Task RemoveCareScheduleAsync(int careScheduleId);

        /// <summary>
        /// Permanently deletes a single Sunlight-Requirement row, identified by
        /// it's Id
        /// </summary>
        Task RemoveSunlightRequirementAsync(int sunlightRequirementId);

        /// <summary>
        /// Persists the given text as the notes for the selected plant
        /// </summary>
        Task UpdatePlantNotesAsync(int plantId, string notes);
    }
}
