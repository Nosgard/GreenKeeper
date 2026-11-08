using GreenKeeper.Models;

namespace GreenKeeper.Services
{
    public interface ISettingsService
    {
        /// <summary>
        /// Reads the stored preferences. Never throws: a missing, empty or damaged
        /// file yields the defaults.
        /// </summary>
        AppSettings Load();

        /// <summary>
        /// Writes the preferences to disk. Never throws either: losing a preference on the next
        /// start is better than a crash.
        /// </summary>
        void Save(AppSettings settings);
    }
}
