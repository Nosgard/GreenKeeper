using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Repositories
{
    public class PlantRepository : IPlantRepository
    {
        // This Repository consists of all the plants that will be depicted in the sidebar
        public IEnumerable<Plant> GetPlants()
        {
            return new List<Plant>();
        }
    }
}
