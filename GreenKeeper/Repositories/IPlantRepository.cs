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
    }
}
