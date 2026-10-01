namespace GreenKeeper.Models
{
    /// <summary>
    /// A record that belongs to exactly one plant (care schedules and the
    /// sunlight requirement). Lets the repository treat both alike when
    /// attaching or replacing them.
    /// </summary>
    public interface IPlantOwned
    {
        int PlantId { get; set; }
    }
}
