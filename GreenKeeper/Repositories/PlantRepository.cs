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
    }
}
