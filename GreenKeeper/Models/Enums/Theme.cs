namespace GreenKeeper.Models.Enums
{
    /// <summary>
    /// The color themes. A theme is saved by its name in the settings file (see
    /// SettingsService), so the members can be reordered, but renaming one makes an older
    /// settings file load as the defaults.
    /// </summary>
    public enum Theme
    {
        Dark,
        Bright
    }
}
