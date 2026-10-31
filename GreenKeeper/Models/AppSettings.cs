using GreenKeeper.Models.Enums;

namespace GreenKeeper.Models
{
    /// <summary>
    /// The preferences the app keeps between sessions, outside the database (see
    /// DbPathProvider.GetSettingsPath). Every property has a default, so a file written by
    /// an older version stays readable: a property it lacks keeps its default.
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
