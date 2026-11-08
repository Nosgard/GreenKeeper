namespace GreenKeeper.Models.Enums
{
    /// <summary>
    /// The care types: Watering and Fertilizing for care schedules, Sunlight for the sunlight
    /// requirement. A care schedule saves its type as a number: changing one maps existing
    /// care schedules to the wrong care type. Moving or renaming a member is safe while its
    /// number stays the same.
    /// </summary>
    public enum CareType
    {
        Watering = 0,
        Fertilizing = 1,
        Sunlight = 2
    }
}
