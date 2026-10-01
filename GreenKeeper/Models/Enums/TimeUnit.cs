namespace GreenKeeper.Models.Enums
{
    /// <summary>
    /// The unit of a care interval. Stored as an integer by EF Core, so the
    /// numbers are part of the persisted data format - see CareType.
    /// </summary>
    public enum TimeUnit
    {
        Days = 0,
        Weeks = 1,
        Months = 2,
        Years = 3
    }
}
