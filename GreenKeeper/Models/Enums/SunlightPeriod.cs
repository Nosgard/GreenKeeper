namespace GreenKeeper.Models.Enums
{
    /// <summary>
    /// The period the hours of sunlight refer to. EF Core stores it as an integer, so the
    /// numbers are part of the saved data (see CareType).
    /// </summary>
    public enum SunlightPeriod
    {
        Day = 0,
        Week = 1,
        Month = 2,
        Year = 3
    }
}
