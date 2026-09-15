using GreenKeeper.Database;
using GreenKeeper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GreenKeeper.Services
{
    /// <summary>
    /// Stores the preferences as JSON next to the database, in
    /// %LocalAppData%\GreenKeeper. Deliberately not in the database: these are
    /// settings of the installation, not data the user works with, and they must
    /// be readable before the database has even been opened.
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,

            // Enums are written as text, not as numbers. There is no database
            // column behind them, so the numeric values carry no meaning - and
            // "Bright" keeps the file readable and survives a later reordering
            // of the enum, which a stored 0 or 1 would not.
            Converters = { new JsonStringEnumConverter() }
        };

        public AppSettings Load()
        {
            try
            {
                var path = DbPathProvider.GetSettingsPath();

                // First start on this machine - no file yet, and that is normal.
                if (!File.Exists(path))
                {
                    return new AppSettings();
                }

                var json = File.ReadAllText(path);

                // Deserialize returns null for a file whose whole content is "null".
                return JsonSerializer.Deserialize<AppSettings>(json, Options) ?? new AppSettings();
            }
            catch (Exception)
            {
                // Intentionally broad: a damaged file, a locked file, a path that
                // suddenly is not reachable - none of it is worth failing the
                // start over. Falling back to the defaults always leaves the app
                // in a usable state, and the next Save repairs the file.
                return new AppSettings();
            }
        }

        public void Save(AppSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, Options);
                File.WriteAllText(DbPathProvider.GetSettingsPath(), json);
            }
            catch (Exception)
            {
                // Same reasoning as in Load, and it matters even more here: the
                // user just clicked a toggle. Losing that choice on the next start
                // is a nuisance, crashing the app over it would not be.
            }
        }
    }
}
