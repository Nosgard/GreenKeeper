using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Models
{
    public class CareSchedule
    {
        public int Id { get; set; }
        public int PlantId { get; set; }
        public CareType Care { get; set; }
        public DateTime? LastCaredAt { get; set; }
        public DateTime? NextDueAt { get; set; }

        // Navigation property
        public Plant SelectedPlant { get; set; } = null!;
    }
}
