using GreenKeeper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Repositories
{
    public class PlantRepository : IPlantRepository
    {
        public IEnumerable<Plant> GetPlants()
        {
            return new List<Plant>
            {
                new Plant { Name = "Aloe Vera" },
                new Plant { Name = "Snake Plant" },
                new Plant { Name = "Cactus" },
            };
        }
    }
}
