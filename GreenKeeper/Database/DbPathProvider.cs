using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Database
{
    public static class DbPathProvider
    {
        /// <summary>
        /// Determines the single, consistent path for the SQLite database file.
        /// Used both by the runtime DbContextFactory and by the design-time factory
        /// that the EF Core migration tooling needs - both must always agree on the
        /// same file, otherwise migrations could end up targeting a different
        /// database than the one the app actually uses.
        /// 
        /// Stored under %LocalAppData%\GreenKeeper, not the program directory:
        /// this location is always writable by the current user regardless
        /// of install location or permissions and survives app updates/reinstalls
        /// since it's independent of where the executable itself lives
        /// </summary>
        /// <returns></returns>
        public static string GetDatabasePath()
        {
            return Path.Combine(GetAppDataFolder(), "greenkeeper.db");
        }

        /// <summary>
        /// The settings file lives next to the database, for the same reasons.
        /// It deliberately stays out of SQLite: these are preferences of the
        /// installation rather than data, and they have to be readable before the
        /// database has even been opened.
        /// </summary>
        public static string GetSettingsPath()
        {
            return Path.Combine(GetAppDataFolder(), "settings.json");
        }

        /// <summary>
        /// Creates the folder if it is not there yet and returns it. Both paths go
        /// through here, so the location is defined in exactly one place.
        /// </summary>
        private static string GetAppDataFolder()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GreenKeeper");

            Directory.CreateDirectory(folder);

            return folder;
        }
    }
}
