using GreenKeeper.Commands;
using GreenKeeper.Models.Enums;
using GreenKeeper.Services;
using GreenKeeper.ViewModels.Base;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.Themes
{
    /// <summary>
    /// Backs the theme toggle in the sidebar header: switches between the two
    /// themes and remembers the choice for the next start.
    /// </summary>
    public class ThemeViewModel : ObservableObject
    {
        private readonly IThemeService _themeService;
        private readonly ISettingsService _settingsService;

        public ThemeViewModel(IThemeService themeService, ISettingsService settingsService)
        {
            _themeService = themeService;
            _settingsService = settingsService;

            ToggleCommand = new RelayCommand(_ => Toggle());
        }

        public ICommand ToggleCommand { get; }

        /// <summary>
        /// True while the dark theme is active. The toggle button binds its icon
        /// to this property - showing a sun (switch TO bright) while dark is
        /// active, and a moon (switch TO dark) while bright is active.
        /// </summary>
        public bool IsDarkTheme => _themeService.CurrentTheme == Theme.Dark;

        private void Toggle()
        {
            var newTheme = IsDarkTheme ? Theme.Bright : Theme.Dark;

            _themeService.ApplyTheme(newTheme);

            // Remember the choice for the next start. Read-modify-write rather than
            // writing a fresh object, so later settings are not dropped on the way.
            // Neither call throws - see SettingsService.
            var settings = _settingsService.Load();
            settings.Theme = newTheme;
            _settingsService.Save(settings);

            OnPropertyChanged(nameof(IsDarkTheme));
        }
    }
}
