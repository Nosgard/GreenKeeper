using GreenKeeper.Models.Enums;

namespace GreenKeeper.Models
{
    public class CareSchedule : IPlantOwned
    {
        public int Id { get; set; }
        public int PlantId { get; set; }
        public CareType Care { get; set; }
        public DateTime? LastCaredAt { get; set; }
        public DateTime? NextDueAt { get; set; }
        public TimeUnit? IntervalUnit { get; set; }
        public int? IntervalAmount { get; set; }

        // Navigation property
        public Plant SelectedPlant { get; set; } = null!;
    }
}
