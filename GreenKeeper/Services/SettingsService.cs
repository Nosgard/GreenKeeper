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

                // Deserialize returns null for a file whose whole content is "null".
                return JsonSerializer.Deserialize<AppSettings>(json, Options) ?? new AppSettings();
            }
            catch (Exception)
            {
                // Broad on purpose: a damaged or locked file or an unreachable path is no reason to fail
                // the start. The defaults keep the app usable, and the next Save repairs the file.
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
                // Same reason as in Load (see ISettingsService.Save).
            }
        }
    }
}
