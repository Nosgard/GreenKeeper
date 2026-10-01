using GreenKeeper.Models.Enums;
using GreenKeeper.Tests.Fakes;
using GreenKeeper.ViewModels.Themes;

namespace GreenKeeper.Tests.ViewModels
{
    public class ThemeViewModelTests
    {
        [Fact]
        public void IsDarkTheme_GivenBrightThemeIsActive_ReturnsFalse()
        {
            // Given: a ViewModel whose theme service reports the bright theme
            var viewModel = new ThemeViewModel(new FakeThemeService(), new FakeSettingsService());

            // When: IsDarkTheme is read
            var isDarkTheme = viewModel.IsDarkTheme;

            // Then: it should be false, so the toggle button shows the moon
            Assert.False(isDarkTheme);
        }

        [Fact]
        public void IsDarkTheme_GivenDarkThemeIsActive_ReturnsTrue()
        {
            // Given: a theme service switched to dark before the ViewModel is built.
            // The fake has no separate seeding method, so its own ApplyTheme sets
            // up the starting state - exactly what the real service would have
            // done when App restored a stored theme at startup
            var themeService = new FakeThemeService();
            themeService.ApplyTheme(Theme.Dark);

            var viewModel = new ThemeViewModel(themeService, new FakeSettingsService());

            // When: IsDarkTheme is read
            var isDarkTheme = viewModel.IsDarkTheme;

            // Then: it should be true, so the toggle button shows the sun
            Assert.True(isDarkTheme);
        }

        [Fact]
        public void ToggleCommand_GivenBrightIsActive_AppliesDark()
        {
            // Given: a ViewModel running on the bright theme
            var themeService = new FakeThemeService();
            var viewModel = new ThemeViewModel(themeService, new FakeSettingsService());

            // When: the toggle command is executed
            viewModel.ToggleCommand.Execute(null);

            // Then: the dark theme should have been applied
            Assert.Equal(Theme.Dark, themeService.CurrentTheme);
            Assert.True(viewModel.IsDarkTheme);
        }

        [Fact]
        public void ToggleCommand_GivenDarkIsActive_AppliesBright()
        {
            // Given: a ViewModel running on the dark theme
            var themeService = new FakeThemeService();
            themeService.ApplyTheme(Theme.Dark);
            var viewModel = new ThemeViewModel(themeService, new FakeSettingsService());

            // When: the toggle command is executed
            viewModel.ToggleCommand.Execute(null);

            // Then: the bright theme should have been applied - the toggle works
            // in both directions, not just away from the default
            Assert.Equal(Theme.Bright, themeService.CurrentTheme);
            Assert.False(viewModel.IsDarkTheme);
        }

        [Fact]
        public void ToggleCommand_WhenExecuted_AppliesThemeExactlyOnce()
        {
            // Given: a fresh theme service that has not been asked to apply anything
            var themeService = new FakeThemeService();
            var viewModel = new ThemeViewModel(themeService, new FakeSettingsService());

            // When: the toggle command is executed once
            viewModel.ToggleCommand.Execute(null);

            // Then: ApplyTheme ran exactly once. Applying twice would be invisible
            // in the end state but would restart the color transition mid-flight
            Assert.Equal(1, themeService.ApplyThemeCallCount);
        }

        [Fact]
        public void ToggleCommand_WhenExecuted_RaisesPropertyChangedForIsDarkTheme()
        {
            // Given: a ViewModel whose PropertyChanged events are recorded
            var viewModel = new ThemeViewModel(new FakeThemeService(), new FakeSettingsService());

            var raisedProperties = new List<string>();
            viewModel.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // When: the toggle command is executed
            viewModel.ToggleCommand.Execute(null);

            // Then: IsDarkTheme was announced. Without this the colors would change
            // but the toggle button would keep showing the icon of the old theme
            Assert.Contains(nameof(ThemeViewModel.IsDarkTheme), raisedProperties);
        }

        [Fact]
        public void ToggleCommand_WhenExecuted_PersistsNewThemeToSettings()
        {
            // Given: a ViewModel on the bright theme with an empty settings store
            var settingsService = new FakeSettingsService();
            var viewModel = new ThemeViewModel(new FakeThemeService(), settingsService);

            // When: the toggle command is executed
            viewModel.ToggleCommand.Execute(null);

            // Then: the new theme was written once, so it survives the next start
            Assert.Equal(1, settingsService.SaveCallCount);
            Assert.Equal(Theme.Dark, settingsService.Settings.Theme);
        }

        [Fact]
        public void ToggleCommand_WhenExecutedTwice_ReturnsToOriginalThemeAndPersistsIt()
        {
            // Given: a ViewModel on the bright theme
            var themeService = new FakeThemeService();
            var settingsService = new FakeSettingsService();
            var viewModel = new ThemeViewModel(themeService, settingsService);

            // When: the toggle command is executed twice
            viewModel.ToggleCommand.Execute(null);
            viewModel.ToggleCommand.Execute(null);

            // Then: both the applied theme and the stored one are back where they
            // started - the stored value follows every switch, not just the first
            Assert.Equal(Theme.Bright, themeService.CurrentTheme);
            Assert.Equal(Theme.Bright, settingsService.Settings.Theme);
            Assert.Equal(2, settingsService.SaveCallCount);
        }

        [Fact]
        public void ToggleCommand_CanExecute_IsAlwaysTrue()
        {
            // Given: a fresh ViewModel
            var viewModel = new ThemeViewModel(new FakeThemeService(), new FakeSettingsService());

            // When: CanExecute is evaluated
            var canExecute = viewModel.ToggleCommand.CanExecute(null);

            // Then: switching the theme never depends on anything
            Assert.True(canExecute);
        }
    }
}
