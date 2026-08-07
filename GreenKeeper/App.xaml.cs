using GreenKeeper.Database;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Data;
using System.Windows;

namespace GreenKeeper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Runs before any window is shown and prepares the SQLite database.
        /// 
        /// Why this exists: An end user who downloads the released application never executes "Update-Database"
        /// manually - therefore their machine would have no tables at all, and the app would crash on the
        /// first data access with a "no such table" error. Applying migrations here, at startup, makes the app
        /// self-sufficient on any machine
        /// </summary>
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var contextFactory = new GreenKeeperDbContextFactory();
                await using var context = contextFactory.CreateDbContext();

                // Applies every migration that hasn't run on this machine yet.
                // On a first launch, this creates the entire schema from scratch
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                // In case the preparing of the database fails, the app cannot work in any meaningful way.
                // Every feature depends on the database so the app must shutdown
                MessageBox.Show(
                    $"The database could not be prepared: \n\n{ex.Message}Theapplication will now close",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
                return;
            }

            // Create the main window manually. StartupUri would show the window immediately
            // before the code above had any chance to run
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }

}
