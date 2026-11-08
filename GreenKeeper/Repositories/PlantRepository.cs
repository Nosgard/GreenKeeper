using GreenKeeper.Database;
using GreenKeeper.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GreenKeeper.Repositories
{
    public class PlantRepository : IPlantRepository
    {
        private readonly IDbContextFactory<GreenKeeperDbContext> _contextFactory;

        public PlantRepository(IDbContextFactory<GreenKeeperDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <inheritdoc />
        public async Task<List<Plant>> GetPlantsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Plants
                .Include(p => p.CareSchedules)
                .Include(p => p.SunlightRequirement)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Plant> AddPlantAsync(Plant plant)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            context.Plants.Add(plant);

            await context.SaveChangesAsync();

            return plant;
        }

        /// <inheritdoc />
        public async Task CompleteCareScheduleAsync(int careScheduleId, DateTime nextDueAt, DateTime lastCaredAt)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var schedule = await context.CareSchedules.FindAsync(careScheduleId)
                ?? throw NotFound("Care schedule", careScheduleId);

            schedule.NextDueAt = nextDueAt;
            schedule.LastCaredAt = lastCaredAt;

            await context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeletePlantAsync(int plantId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var plant = await context.Plants.FindAsync(plantId)
                ?? throw NotFound("Plant", plantId);

            context.Plants.Remove(plant);
            await context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public Task<CareSchedule> AddOrReplaceCareScheduleAsync(int plantId, CareSchedule careSchedule)
        {
            return ReplaceAsync(
                plantId,
                careSchedule,
                context => context.CareSchedules,
                existing => existing.PlantId == plantId && existing.Care == careSchedule.Care);
        }

        /// <inheritdoc />
        public Task<SunlightRequirement> AddOrReplaceSunlightRequirementAsync(int plantId, SunlightRequirement sunlightRequirement)
        {
            return ReplaceAsync(
                plantId,
                sunlightRequirement,
                context => context.SunlightRequirements,
                existing => existing.PlantId == plantId);
        }

        /// <summary>
        /// Removes the row matched by isExisting (if any) and inserts the replacement for the
        /// given plant in one transaction, so a failed insert keeps the old row.
        /// </summary>
        private async Task<TEntity> ReplaceAsync<TEntity>(
            int plantId,
            TEntity replacement,
            Func<GreenKeeperDbContext, DbSet<TEntity>> entitiesOf,
            Expression<Func<TEntity, bool>> isExisting)
            where TEntity : class, IPlantOwned
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var entities = entitiesOf(context);
            var existing = await entities.FirstOrDefaultAsync(isExisting);

            if (existing != null)
            {
                // Save the removal before the Add, so the unique index never sees both rows.
                entities.Remove(existing);
                await context.SaveChangesAsync();
            }

            replacement.PlantId = plantId;
            entities.Add(replacement);
            await context.SaveChangesAsync();

            await transaction.CommitAsync();

            return replacement;
        }

        /// <inheritdoc />
        public async Task RemoveCareScheduleAsync(int careScheduleId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var schedule = await context.CareSchedules.FindAsync(careScheduleId)
                ?? throw NotFound("Care schedule", careScheduleId);

            context.CareSchedules.Remove(schedule);
            await context.SaveChangesAsync();
        }

        public async Task RemoveSunlightRequirementAsync(int sunlightRequirementId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var requirement = await context.SunlightRequirements.FindAsync(sunlightRequirementId)
                ?? throw NotFound("Sunlight requirement", sunlightRequirementId);

            context.SunlightRequirements.Remove(requirement);
            await context.SaveChangesAsync();
        }

        public async Task UpdatePlantNotesAsync(int plantId, string notes)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var plant = await context.Plants.FindAsync(plantId)
                ?? throw NotFound("Plant", plantId);

            plant.Notes = notes;
            await context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task RenamePlantAsync(int plantId, string newName)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var plant = await context.Plants.FindAsync(plantId)
                ?? throw NotFound("Plant", plantId);

            plant.Name = newName;
            await context.SaveChangesAsync();
        }

        // The row may be gone by now (deleting a plant deletes its schedules too). The message
        // names the missing row, because the error dialog shows it.
        private static InvalidOperationException NotFound(string entityName, int id)
        {
            return new InvalidOperationException($"{entityName} with Id {id} was not found.");
        }
    }
}
