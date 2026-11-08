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

            var mainWindow = new MainWindow(mainViewModel, dialogService, TimeProvider.System, settingsService);
            mainWindow.Show();

            // OnStartup subscribes only after Show. An exception before this line ends the app,
            // also one thrown while the "Database Error" dialog is open.
            DispatcherUnhandledException += (_, args) => ReportUnexpectedError(args, mainWindow, dialogService);
        }

        /// <summary>
        /// Applies the pending migrations on every start, because users of the release never
        /// update the database themselves. Without them the first query would fail with
        /// "no such table".
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
        /// Shows the dialog "Unexpected Error" for each UI-thread exception that no helper
        /// caught (see IDialogService.ShowError), and the app keeps running.
        /// </summary>
        private void ReportUnexpectedError(DispatcherUnhandledExceptionEventArgs e, Window mainWindow, IDialogService dialogService)
        {
            // After the main window has closed, the exception ends the app without a dialog.
            if (!mainWindow.IsVisible)
            {
                return;
            }

            // An exception that comes back on every redraw would otherwise stack dialog on
            // dialog, so an exception while this dialog is open ends the app.
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
