using GreenKeeper.Database;
using GreenKeeper.Repositories;
using GreenKeeper.Services;
using GreenKeeper.ViewModels;
using GreenKeeper.ViewModels.Themes;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Threading;

namespace GreenKeeper
{
    /// <summary>
    /// The composition root: creates everything the main window needs and wires it up.
    /// </summary>
    public partial class App : Application
    {
        // True while ReportUnexpectedError shows its dialog.
        private bool _isReportingUnexpectedError;

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

            // The app subscribes only after Show, so an exception before this line ends the app,
            // also one while the "Database Error" dialog is open.
            DispatcherUnhandledException += (_, args) => ReportUnexpectedError(args, mainWindow, dialogService);
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

        /// <summary>
        /// Once OnStartup has shown the main window, this method receives each exception
        /// on the UI thread that no helper caught (see IDialogService.ShowError). It shows
        /// the dialog "Unexpected Error", and the app keeps running. After the main window
        /// has closed, or while that dialog is already open, this method does nothing and
        /// the exception ends the app.
        /// </summary>
        private void ReportUnexpectedError(DispatcherUnhandledExceptionEventArgs e, Window mainWindow, IDialogService dialogService)
        {
            // IsVisible turns false when the main window closes.
            if (!mainWindow.IsVisible)
            {
                return;
            }

            // A second exception while the dialog is open ends the app. Without this check,
            // this method would stack dialog on dialog for a layout pass that fails every time.
            if (_isReportingUnexpectedError)
            {
                return;
            }

            _isReportingUnexpectedError = true;
            dialogService.ShowError(
                $"Something unexpected went wrong:\n{e.Exception.Message}\n\n" +
                "If anything looks wrong now, restart the application to see what is saved",
                "Unexpected Error");
            _isReportingUnexpectedError = false;

            e.Handled = true;
        }
    }
}
