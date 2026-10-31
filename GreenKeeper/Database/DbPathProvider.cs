using System.IO;

namespace GreenKeeper.Database
{
    public static class DbPathProvider
    {
        /// <summary>
        /// The path of the SQLite file. The app and the EF Core migration tooling both take it
        /// from here, so migrations reach the file the app uses. It lies under
        /// %LocalAppData%\GreenKeeper rather than next to the program: the user can write there
        /// wherever the app is installed, and the file survives updates and reinstalls.
        /// </summary>
        public static string GetDatabasePath()
        {
            return Path.Combine(EnsureAppDataFolder(), "greenkeeper.db");
        }

        /// <summary>
        /// The settings file lives next to the database, for the same reasons. It stays out of
        /// SQLite because it holds preferences of the installation, not the user's plant data.
        /// </summary>
        public static string GetSettingsPath()
        {
            return Path.Combine(EnsureAppDataFolder(), "settings.json");
        }

        private static string EnsureAppDataFolder()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GreenKeeper");

            Directory.CreateDirectory(folder);

            return folder;
        }
    }
}
