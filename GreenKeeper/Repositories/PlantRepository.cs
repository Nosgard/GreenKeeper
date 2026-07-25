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

        // This Repository consists of all the plants that will be depicted in the sidebar
        public async Task<List<Plant>> GetPlantsAsync()
        {
            // Fresh, short-lived Context for this one step.
            // Will be disposed by the end of the "await using"-Block
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Include: Mandatory because otherwise Care-Schedules/Sunlight-Requirements remain empty.
            // AsNoTracking: Data will be shown read-only, so they won't get changed during the execution
            return await context.Plants
                .Include(p => p.CareSchedules)
                .Include(p => p.SunlightRequirement)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Saves a new Plant-Object - together with everything the Add-Plant-Wizard
        /// may have already attached to it in memory to the database in one single
        /// operation.
        /// 
        /// How this works under the hood:
        /// EF-Core's "change tracker" walks the entire object graph reachable
        /// from "plant" once it's added (plant itself, every Care-Schedule in
        /// plant.CareSchedules, and plant.SunlightRequirement if set). Any
        /// object in that graph whose Id is still 0 is treated as "new" and
        /// will be INSERTed. This is why nothing needs to be done manually
        /// here to link the Care-Schedules/Sunlight-Requirement to the plant.
        /// EF-Core figures out the PlantId foreign keys automatically once
        /// it knows the new Plant-Object's generated Id
        /// </summary>
        public async Task<Plant> AddPlantAsync(Plant plant)
        {
            // Fresh, short-lived Context for this one step.
            // Will be disposed by the end of the "await using"-Block
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Marks "plant" as newly-added, pending data to be written to the
            // database on the next SaveChangesAsync() call
            context.Plants.Add(plant);

            // Executes the INSERT statements against the database.
            // After that, the data is permanently stored on disk
            await context.SaveChangesAsync();

            return plant;
        }

        /// <summary>
        /// Loads a single Care-Schedule by it's Id, updates it's next due date (NextDueAt) and
        /// the date of the last care (LastCaredAt), and saves that change back to the database
        /// </summary>
        public async Task CompleteCareScheduleAsync(int careScheduleId, DateTime nextDueAt, DateTime lastCaredAt)
        {
            // Fresh, short-lived Context for this one step.
            // Will be disposed by the end of the "await using"-Block
            await using var context = await _contextFactory.CreateDbContextAsync();

            var schedule = await context.CareSchedules.FindAsync(careScheduleId);

            if (schedule == null)
            {
                // The schedule could get deleted in any other way but via the Remove-Button,
                // so give the user an explaining exception in this case
                throw new InvalidOperationException($"Care-Schedule with Id {careScheduleId} was not found");
            }

            schedule.NextDueAt = nextDueAt;
            schedule.LastCaredAt = lastCaredAt;

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes the Plant-Object identified by the Id (plantId), along with everything
        /// the database's cascading foreign keys automatically remove with it
        /// (Care-Schedules and Sunlight-Requirement)
        /// </summary>
        public async Task DeletePlantAsync(int plantId)
        {
            // Fresh, short-lived Context for this one step.
            // Will be disposed by the end of the "await using"-Block
            await using var context = await _contextFactory.CreateDbContextAsync();

            var plant = await context.Plants.FindAsync(plantId);

            if (plant == null)
            {
                // It can happen that the plant was already deleted through some
                // other means in the meantime
                throw new InvalidOperationException($"Plant with Id {plantId} was not found.");
            }

            context.Plants.Remove(plant);
            await context.SaveChangesAsync();
        }
    }
}
