using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Models
{
    public class SunlightRequirement
    {
        public int Id { get; set; }
        public int PlantId { get; set; }
        public int Hours { get; set; }
        public SunlightPeriod Period { get; set; }

        public Plant SelectedPlant { get; set; } = null!;
    }
}
