using GreenKeeper.Repositories;
using GreenKeeper.Services;
using GreenKeeper.ViewModels.Base;
using GreenKeeper.ViewModels.Themes;
#if DEBUG
using GreenKeeper.ViewModels.TimeSimulation;
#endif

namespace GreenKeeper.ViewModels
{
    /// <summary>
    /// Backs the main window and puts its two parts together: PlantList for the sidebar
    /// and Dashboard for the cards of the selected plant. It keeps what both need, the
    /// announcements, the periodic refresh, the theme and the debug panel.
    /// </summary>
    public class MainViewModel : ObservableObject
    {
        // Keeps the due date texts and the Complete button's enabled state (IsCompletable)
        // up to date on their own.
        private static readonly TimeSpan CareStatusRefreshInterval = TimeSpan.FromMinutes(5);

        private readonly ITimerService _timerService;

        public MainViewModel(
            IPlantRepository plantRepository,
            IDialogService dialogService,
            ITimerService timerService,
            TimeProvider timeProvider,
            ThemeViewModel theme)
        {
            _timerService = timerService;
            Theme = theme;

            // Dashboard is only set on the next line, but no plant can be selected before that.
            PlantList = new PlantListViewModel(plantRepository, timeProvider, Announce, onSelectionChanged: () => Dashboard!.ShowSelectedPlant());
            Dashboard = new DashboardViewModel(plantRepository, dialogService, timeProvider, PlantList, Announce);

            _timerService.Start(CareStatusRefreshInterval, RefreshDueStatus);

#if DEBUG
            TimeSimulation = new TimeSimulationViewModel(() => PlantList.SelectedPlant, RefreshDueStatus);
#endif
        }

        // The sidebar: the plant list with its search and the selection.
        public PlantListViewModel PlantList { get; }

        // The dashboard: the cards of the selected plant and the actions on them.
        public DashboardViewModel Dashboard { get; }

        // -- Announcements Section --

        /// <summary>
        /// What an action did, in a short sentence. Sighted users see the result on screen;
        /// MainWindow hands this text to a screen reader, which stays silent otherwise
        /// because the focus does not move.
        /// </summary>
        public event EventHandler<string>? Announced;

        private void Announce(string text)
        {
            Announced?.Invoke(this, text);
        }

        /// <summary>
        /// The periodic refresh. Only time has passed, so only what depends on the current
        /// day is re-read. The cards keep their objects, and the sidebar is rebuilt only
        /// once a plant shows a different state, since a screen reader announces every
        /// rebuilt element again.
        /// </summary>
        private void RefreshDueStatus()
        {
            foreach (var card in Dashboard.CareStatuses)
            {
                card.NotifyDueStatusChanged();
            }

            if (PlantList.HasAnyPlantStatusChanged())
            {
                PlantList.RefreshSidebar();
            }
        }

        /// <summary>
        /// Stop the periodic status card refresh (see _timerService.Start in the constructor).
        /// Called by MainWindow.xaml.cs when the main window is closed, so the timer doesn't
        /// keep running (and referencing this ViewModel) after the application would otherwise
        /// have shut down.
        /// </summary>
        public void StopCareStatusRefreshTimer()
        {
            _timerService.Stop();
        }

        // -- Theme and Debug Section --

        public ThemeViewModel Theme { get; }

#if DEBUG
        public bool IsDebugBuild => true;

        public TimeSimulationViewModel TimeSimulation { get; }
#else
        public bool IsDebugBuild => false;
#endif
    }
}
