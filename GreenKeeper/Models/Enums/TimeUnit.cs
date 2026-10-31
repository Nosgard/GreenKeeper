namespace GreenKeeper.Models.Enums
{
    /// <summary>
    /// The unit of a care interval. EF Core stores it as an integer, so the numbers are part
    /// of the saved data (see CareType).
    /// </summary>
    public enum TimeUnit
    {
        Days = 0,
        Weeks = 1,
        Months = 2,
        Years = 3
    }
}
