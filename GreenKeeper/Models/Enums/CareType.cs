namespace GreenKeeper.Models.Enums
{
    /// <summary>
    /// The types of care schedules. EF Core stores enums as integers, so the numbers are
    /// part of the saved data: changing a member's number would map existing records to the
    /// wrong care type. Moving or renaming a member is safe while its number stays the same.
    /// </summary>
    public enum CareType
    {
        Watering = 0,
        Fertilizing = 1,
        Sunlight = 2
    }
}
