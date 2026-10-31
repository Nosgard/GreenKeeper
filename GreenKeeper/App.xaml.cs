using GreenKeeper.Database;
using GreenKeeper.Repositories;
using GreenKeeper.Services;
using GreenKeeper.ViewModels;
using GreenKeeper.ViewModels.Themes;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace GreenKeeper
{
    /// <summary>
    /// The composition root: creates everything the main window needs and wires it up.
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var dialogService = new MessageDialogService();
            var contextFactory = new GreenKeeperDbContextFactory();

            if (!await TryPrepareDatabaseAsync(contextFactory, dialogService))
            {
                // Every feature needs the database, so the app cannot run without it.
                Shutdown();
                return;
            }

            // Restore the last theme before the window exists; applied later, the window would
            // show the default theme first.
            var settingsService = new SettingsService();
            var themeService = new ThemeService();
            themeService.ApplyTheme(settingsService.Load().Theme);

            var mainViewModel = new MainViewModel(
                new PlantRepository(contextFactory),
                dialogService,
                new DispatcherTimerService(),
                TimeProvider.System,
                new ThemeViewModel(themeService, settingsService));

            // The app creates the window here, not through StartupUri, which would show it before
            // the code above has run.
            var mainWindow = new MainWindow(mainViewModel, dialogService, TimeProvider.System, settingsService);
            mainWindow.Show();
        }

        /// <summary>
        /// Applies the pending migrations. Users of the release never run Update-Database, so
        /// without this the first query would fail with "no such table".
        /// </summary>
        private static async Task<bool> TryPrepareDatabaseAsync(GreenKeeperDbContextFactory contextFactory, IDialogService dialogService)
        {
            try
            {
                await using var context = contextFactory.CreateDbContext();
                await context.Database.MigrateAsync();
                return true;
            }
            catch (Exception ex)
            {
                dialogService.ShowError(
                    $"The database could not be prepared: \n\n{ex.Message}\n\nThe application will now close",
                    "Database Error");
                return false;
            }
        }
    }
}
