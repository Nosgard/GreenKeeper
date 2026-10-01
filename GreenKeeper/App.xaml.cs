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
    /// Interaction logic for App.xaml. The composition root: everything the main
    /// window needs is created and wired up here, in one place.
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var dialogService = new MessageBoxDialogService();
            var contextFactory = new GreenKeeperDbContextFactory();

            if (!await TryPrepareDatabaseAsync(contextFactory, dialogService))
            {
                // If preparing the database fails, the app cannot work in any meaningful way.
                // Every feature depends on the database, so the app must shut down.
                Shutdown();
                return;
            }

            // Restore the theme the user picked last time. This has to happen before
            // the window is created - applying it afterwards would show the window in
            // the default theme first and only then switch. Load never throws, so with a
            // missing or damaged settings file the app simply starts in the default theme.
            var settingsService = new SettingsService();
            var themeService = new ThemeService();
            themeService.ApplyTheme(settingsService.Load().Theme);

            var mainViewModel = new MainViewModel(
                new PlantRepository(contextFactory),
                dialogService,
                new DispatcherTimerService(),
                TimeProvider.System,
                new ThemeViewModel(themeService, settingsService));

            // Create the main window manually. StartupUri would show the window immediately
            // before the code above had any chance to run.
            var mainWindow = new MainWindow(mainViewModel, dialogService, TimeProvider.System);
            mainWindow.Show();
        }

        /// <summary>
        /// Applies every migration that hasn't run on this machine yet; on a first
        /// launch, this creates the entire schema from scratch.
        ///
        /// Why this exists: An end user who downloads the released application never executes "Update-Database"
        /// manually - therefore their machine would have no tables at all, and the app would crash on the
        /// first data access with a "no such table" error. Applying migrations here, at startup, makes the app
        /// self-sufficient on any machine.
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
