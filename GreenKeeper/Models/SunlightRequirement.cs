using GreenKeeper.Models.Enums;

namespace GreenKeeper.Models
{
    public class SunlightRequirement : IPlantOwned
    {
        public int Id { get; set; }
        public int PlantId { get; set; }
        public int Hours { get; set; }
        public SunlightPeriod Period { get; set; }

        // EF Core navigation to the plant that owns this requirement; despite its name, it is
        // unrelated to the selection in the sidebar.
        public Plant SelectedPlant { get; set; } = null!;
    }
}
