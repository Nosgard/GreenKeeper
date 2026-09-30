using GreenKeeper.Database;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Repositories
{
    public class PlantRepository : IPlantRepository
    {
        private readonly IDbContextFactory<GreenKeeperDbContext> _contextFactory;

        public PlantRepository(IDbContextFactory<GreenKeeperDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Loads all plants including their care schedules and
        /// sunlight requirement from the database, to be displayed in the sidebar.
        /// </summary>
        public async Task<List<Plant>> GetPlantsAsync()
        {
            // Fresh, short-lived context for this one step.
            // Will be disposed at the end of the "await using" block.
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Include: Mandatory because otherwise care schedules/sunlight requirements remain empty.
            // AsNoTracking: Data will be shown read-only, so they won't get changed during the execution.
            return await context.Plants
                .Include(p => p.CareSchedules)
                .Include(p => p.SunlightRequirement)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Saves a new plant object - together with everything the Add Plant wizard
        /// may have already attached to it in memory - to the database in one single
        /// operation.
        /// 
        /// How this works under the hood:
        /// EF Core's "change tracker" walks the entire object graph reachable
        /// from "plant" once it's added (plant itself, every care schedule in
        /// plant.CareSchedules, and plant.SunlightRequirement if set). Any
        /// object in that graph whose Id is still 0 is treated as "new" and
        /// will be INSERTed. This is why nothing needs to be done manually
        /// here to link the care schedules/sunlight requirement to the plant.
        /// EF Core figures out the PlantId foreign keys automatically once
        /// it knows the new plant object's generated Id.
        /// </summary>
        public async Task<Plant> AddPlantAsync(Plant plant)
        {
            // Fresh, short-lived context for this one step.
            // Will be disposed at the end of the "await using" block.
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Marks "plant" as newly added, pending data to be written to the
            // database on the next SaveChangesAsync() call.
            context.Plants.Add(plant);

            // Executes the INSERT statements against the database.
            // After that, the data is permanently stored on disk.
            await context.SaveChangesAsync();

            return plant;
        }

        /// <summary>
        /// Loads a single care schedule by its Id, updates its next due date (NextDueAt) and
        /// the date of the last care (LastCaredAt), and saves that change back to the database.
        /// </summary>
        public async Task CompleteCareScheduleAsync(int careScheduleId, DateTime nextDueAt, DateTime lastCaredAt)
        {
            // Fresh, short-lived context for this one step.
            // Will be disposed at the end of the "await using" block.
            await using var context = await _contextFactory.CreateDbContextAsync();

            var schedule = await context.CareSchedules.FindAsync(careScheduleId);

            if (schedule == null)
            {
                // The schedule could get deleted in some other way than via the Remove button,
                // so give the user an explanatory exception in this case.
                throw new InvalidOperationException($"Care-Schedule with Id {careScheduleId} was not found");
            }

            schedule.NextDueAt = nextDueAt;
            schedule.LastCaredAt = lastCaredAt;

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes the plant object identified by the Id (plantId), along with everything
        /// the database's cascading foreign keys automatically remove with it
        /// (care schedules and sunlight requirement).
        /// </summary>
        public async Task DeletePlantAsync(int plantId)
        {
            // Fresh, short-lived context for this one step.
            // Will be disposed at the end of the "await using" block.
            await using var context = await _contextFactory.CreateDbContextAsync();

            var plant = await context.Plants.FindAsync(plantId);

            if (plant == null)
            {
                // It can happen that the plant was already deleted through some
                // other means in the meantime.
                throw new InvalidOperationException($"Plant with Id {plantId} was not found.");
            }

            context.Plants.Remove(plant);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Adds a new care schedule for the given plant, or replaces the existing
        /// one of the same care type if one already exists. Used both by the
        /// Add Schedule wizard and the edit dialog - both cases boil down
        /// to the same database operation. The passed-in schedule should already
        /// have the next due date (NextDueAt) and the last date of care (LastCaredAt)
        /// calculated by the caller (MainViewModel); this method only persists it.
        /// </summary>
        public async Task<CareSchedule> AddOrReplaceCareScheduleAsync(int plantId, CareSchedule careSchedule)
        {
            // Fresh, short-lived context for this one step.
            // Will be disposed at the end of the "await using" block.
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Runs the removal and the insert below as one unit: if the insert fails, the
            // transaction is disposed without a commit and the removal is rolled back too.
            await using var transaction = await context.Database.BeginTransactionAsync();

            var existing = await context.CareSchedules
                .FirstOrDefaultAsync(cs => cs.PlantId == plantId && cs.Care == careSchedule.Care);

            if (existing != null)
            {
                // Remove + own SaveChangesAsync BEFORE the Add:
                // guarantees the old row is gone
                // before the new one is inserted, avoiding a brief clash with the unique index on (PlantId, Care).
                context.CareSchedules.Remove(existing);
                await context.SaveChangesAsync();
            }

            careSchedule.PlantId = plantId;
            context.CareSchedules.Add(careSchedule);
            await context.SaveChangesAsync();

            await transaction.CommitAsync();

            return careSchedule;
        }

        /// <summary>
        /// Adds a new sunlight requirement for the given plant, or replaces the
        /// existing one if present - analogous to AddOrReplaceCareScheduleAsync,
        /// just for the 1:1 sunlight requirement relationship instead of the
        /// 1:many care schedules.
        /// </summary>
        public async Task<SunlightRequirement> AddOrReplaceSunlightRequirementAsync(int plantId, SunlightRequirement sunlightRequirement)
        {
            // Fresh, short-lived context for this one step.
            // Will be disposed at the end of the "await using" block.
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Removal and insert as one unit, same as AddOrReplaceCareScheduleAsync.
            await using var transaction = await context.Database.BeginTransactionAsync();

            var existing = await context.SunlightRequirements
                .FirstOrDefaultAsync(sr => sr.PlantId == plantId);

            if (existing != null)
            {
                // Remove + save first, then insert, same as AddOrReplaceCareScheduleAsync.
                context.SunlightRequirements.Remove(existing);
                await context.SaveChangesAsync();
            }

            sunlightRequirement.PlantId = plantId;
            context.SunlightRequirements.Add(sunlightRequirement);
            await context.SaveChangesAsync();

            await transaction.CommitAsync();

            return sunlightRequirement;
        }

        /// <summary>
        /// Permanently deletes a single care schedule row, identified by its Id.
        /// Only ever called for optional care types (Fertilizing).
        /// </summary>
        public async Task RemoveCareScheduleAsync(int careScheduleId)
        {
            // Fresh, short-lived context for this one step.
            // Will be disposed at the end of the "await using" block.
            await using var context = await _contextFactory.CreateDbContextAsync();

            var schedule = await context.CareSchedules.FindAsync(careScheduleId);

            if (schedule == null)
            {
                throw new InvalidOperationException($"Care-Schedule with Id {careScheduleId} was not found");
            }

            context.CareSchedules.Remove(schedule);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Permanently deletes a single sunlight requirement row, identified
        /// by its Id.
        /// </summary>
        public async Task RemoveSunlightRequirementAsync(int sunlightRequirementId)
        {
            // Fresh, short-lived context for this one step.
            // Will be disposed at the end of the "await using" block.
            await using var context = await _contextFactory.CreateDbContextAsync();

            var requirement = await context.SunlightRequirements.FindAsync(sunlightRequirementId);

            if (requirement == null)
            {
                throw new InvalidOperationException($"Sunlight-Requirement with Id {sunlightRequirementId} was not found");
            }

            context.SunlightRequirements.Remove(requirement);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Persists the given text as the notes for the specified plant,
        /// identified by its Id.
        /// </summary>
        public async Task UpdatePlantNotesAsync(int plantId, string notes)
        {
            // Fresh, short-lived context for this one step.
            // Will be disposed at the end of the "await using" block.
            await using var context = await _contextFactory.CreateDbContextAsync();

            var plant = await context.Plants.FindAsync(plantId);

            if (plant == null)
            {
                throw new InvalidOperationException($"Plant with Id {plantId} was not found");
            }

            plant.Notes = notes;
            await context.SaveChangesAsync();
        }

        public async Task RenamePlantAsync(int plantId, string newName)
        {
            // Fresh, short-lived context for this one step.
            // Will be disposed at the end of the "await using" block.
            await using var context = await _contextFactory.CreateDbContextAsync();

            var plant = await context.Plants.FindAsync(plantId);

            if (plant == null)
            {
                throw new InvalidOperationException($"Plant with Id {plantId} was not found");
            }

            plant.Name = newName;
            await context.SaveChangesAsync();
        }
    }
}
