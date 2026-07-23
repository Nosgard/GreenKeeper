using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Models
{
    public class Plant
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public List<CareSchedule> CareSchedules { get; set; } = new List<CareSchedule>();
        public SunlightRequirement? SunlightRequirement { get; set; }
    }
}
