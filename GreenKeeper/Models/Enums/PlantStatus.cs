namespace GreenKeeper.Models.Enums
{
    /// <summary>
    /// The most urgent state among the cares of a plant that have a due date. The
    /// sidebar shows it as the color of the status dot and names it for screen readers.
    /// </summary>
    public enum PlantStatus
    {
        UpToDate,
        DueToday,
        Overdue
    }
}
