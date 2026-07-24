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
    }
}
