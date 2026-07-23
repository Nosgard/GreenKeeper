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
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GreenKeeper");

            Directory.CreateDirectory(folder);

            return Path.Combine(folder, "greenkeeper.db");
        }
    }
}
