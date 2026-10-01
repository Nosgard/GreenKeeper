namespace GreenKeeper.Models.Enums
{
    /// <summary>
    /// The period the hours of sunlight refer to. Stored as an integer by
    /// EF Core, so the numbers are part of the persisted data format - see CareType.
    /// </summary>
    public enum SunlightPeriod
    {
        Day = 0,
        Week = 1,
        Month = 2,
        Year = 3
    }
}
