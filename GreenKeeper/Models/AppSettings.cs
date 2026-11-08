using GreenKeeper.Models.Enums;

namespace GreenKeeper.Models
{
    /// <summary>
    /// The preferences SettingsService keeps between sessions, outside the database (see
    /// DbPathProvider.GetSettingsPath). A property missing from an older file keeps its default.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// The theme to restore on the next start.
        /// </summary>
        public Theme Theme { get; set; } = Theme.Bright;

        /// <summary>
        /// Where the main window was closed, to open it there again. Null until the first
        /// close, and in a settings file of an earlier version.
        /// </summary>
        public WindowPlacement? WindowPlacement { get; set; }
    }
}
