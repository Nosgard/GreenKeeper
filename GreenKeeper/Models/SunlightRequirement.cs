using GreenKeeper.Models.Enums;

namespace GreenKeeper.Models
{
    public class SunlightRequirement : IPlantOwned
    {
        public int Id { get; set; }
        public int PlantId { get; set; }
        public int Hours { get; set; }
        public SunlightPeriod Period { get; set; }

        public Plant SelectedPlant { get; set; } = null!;
    }
}
