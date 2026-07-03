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
        // This Repository consists of all the plants that will be depicted in the sidebar. Currently contains dummy plants
        public IEnumerable<Plant> GetPlants()
        {
            return new List<Plant>
            {
                new Plant {
                    Name = "Aloe Vera",

                    CareSchedules = new List<CareSchedule>
                    {
                        new CareSchedule { Care = CareType.Water, NextDueAt = DateTime.Now.AddDays(3)},
                        new CareSchedule { Care = CareType.Nutrients, NextDueAt = DateTime.Now.AddDays(30)},
                    },
                    SunlightRequirement = new SunlightRequirement {Hours = 6, Period = SunlightPeriod.Day}
                },
                new Plant {
                    Name = "Snake Plant",

                    CareSchedules = new List<CareSchedule>
                    {
                        new CareSchedule { Care = CareType.Water, NextDueAt = DateTime.Now.AddDays(0)}
                    }
                },
                new Plant {
                    Name = "Cactus",

                    CareSchedules = new List<CareSchedule>
                    {
                        new CareSchedule { Care = CareType.Water, NextDueAt = DateTime.Now.AddDays(-14)}
                    }
                },
            };
        }
    }
}
