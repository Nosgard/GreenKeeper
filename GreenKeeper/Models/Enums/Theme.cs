namespace GreenKeeper.Models.Enums
{
    /// <summary>
    /// The color themes. Unlike CareType, a theme is saved as text in the settings file (see
    /// SettingsService), so the numbers carry no meaning and the members can be reordered.
    /// </summary>
    public enum Theme
    {
        Dark,
        Bright
    }
}
