using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Services;
using System.IO;

namespace GreenKeeper.Tests.Services
{
    /// <summary>
    /// Runs SettingsService against a settings file in a temporary folder of its own,
    /// so the tests never touch the user's real settings.json. xUnit creates a new
    /// instance for every test, so each test starts without a settings file.
    /// </summary>
    public class SettingsServiceTests : IDisposable
    {
        private readonly DirectoryInfo _settingsFolder = Directory.CreateTempSubdirectory("GreenKeeper.Tests-");
        private readonly string _settingsPath;
        private readonly SettingsService _settingsService;

        public SettingsServiceTests()
        {
            _settingsPath = Path.Combine(_settingsFolder.FullName, "settings.json");
            _settingsService = new SettingsService(() => _settingsPath);
        }

        public void Dispose()
        {
            _settingsFolder.Delete(recursive: true);
        }

        // -- Load Tests --

        [Fact]
        public void Load_GivenNoSettingsFileYet_ReturnsTheDefaults()
        {
            // Given: the first start on this machine - the folder holds no settings file yet

            // When: the settings are loaded
            var settings = _settingsService.Load();

            // Then: the app starts in the bright theme
            Assert.Equal(Theme.Bright, settings.Theme);
        }

        [Fact]
        public void Load_GivenThemeSavedBefore_RestoresIt()
        {
            // Given: the dark theme, saved when the user last switched to it
            _settingsService.Save(new AppSettings { Theme = Theme.Dark });

            // When: the settings are loaded on the next start
            var settings = _settingsService.Load();

            // Then: the dark theme is restored
            Assert.Equal(Theme.Dark, settings.Theme);
        }

        /// <summary>
        /// The theme is stored by its name. A file written by an earlier version has to
        /// stay readable, so that format must not change unnoticed.
        /// </summary>
        [Fact]
        public void Load_GivenFileNamingTheTheme_ReadsTheTheme()
        {
            // Given: a settings file as the app writes it, with the theme as text
            File.WriteAllText(_settingsPath, """{ "Theme": "Dark" }""");

            // When: the settings are loaded
            var settings = _settingsService.Load();

            // Then: the theme is recognized by its name
            Assert.Equal(Theme.Dark, settings.Theme);
        }

        [Fact]
        public void Load_GivenDamagedFile_ReturnsTheDefaults()
        {
            // Given: a settings file that breaks off in the middle
            File.WriteAllText(_settingsPath, """{ "Theme": """);

            // When: the settings are loaded
            var settings = _settingsService.Load();

            // Then: the app starts in the bright theme instead of failing
            Assert.Equal(Theme.Bright, settings.Theme);
        }

        // -- Save Tests --

        [Fact]
        public void Save_GivenTheFileCannotBeWritten_DoesNotThrow()
        {
            // Given: a service whose settings file lies in a folder that does not exist
            var unreachablePath = Path.Combine(_settingsFolder.FullName, "missing", "settings.json");
            var settingsService = new SettingsService(() => unreachablePath);

            // When: the settings are saved
            var exception = Record.Exception(() => settingsService.Save(new AppSettings { Theme = Theme.Dark }));

            // Then: the failure stays inside - losing the preference beats taking the app down
            Assert.Null(exception);
        }
    }
}
