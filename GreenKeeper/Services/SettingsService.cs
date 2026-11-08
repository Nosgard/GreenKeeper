using GreenKeeper.Database;
using GreenKeeper.Models;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GreenKeeper.Services
{
    /// <summary>
    /// Stores the preferences as JSON next to the database, in %LocalAppData%\GreenKeeper
    /// (see DbPathProvider.GetSettingsPath).
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,

            // Enums as text: the file stays readable, and the enum can be reordered (see Theme).
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly Func<string> _settingsPath;

        public SettingsService()
            : this(DbPathProvider.GetSettingsPath)
        {
        }

        // The path is asked for on every Load and Save rather than once, so a folder
        // that cannot be created is handled there like any other failure.
        public SettingsService(Func<string> settingsPath)
        {
            _settingsPath = settingsPath;
        }

        public AppSettings Load()
        {
            try
            {
                var path = _settingsPath();

                // First start on this machine: no file yet.
                if (!File.Exists(path))
                {
                    return new AppSettings();
                }

                var json = File.ReadAllText(path);

                // A file that holds only "null" also gives the default settings.
                return JsonSerializer.Deserialize<AppSettings>(json, Options) ?? new AppSettings();
            }
            catch (Exception)
            {
                // Broad on purpose: a damaged or locked file or an unreachable path must not stop
                // the start. The next Save overwrites a damaged file.
                return new AppSettings();
            }
        }

        public void Save(AppSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, Options);
                File.WriteAllText(_settingsPath(), json);
            }
            catch (Exception)
            {
                // Broad on purpose, like in Load (see ISettingsService.Save).
            }
        }
    }
}
