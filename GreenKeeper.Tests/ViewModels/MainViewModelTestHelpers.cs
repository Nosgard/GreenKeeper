using GreenKeeper.Models;
using GreenKeeper.Tests.Fakes;
using GreenKeeper.ViewModels;
using GreenKeeper.ViewModels.Themes;
using System.Collections.Specialized;
using System.Windows.Data;

namespace GreenKeeper.Tests.ViewModels
{
    /// <summary>
    /// Builds the MainViewModel under test on fakes and reads what its sidebar shows and
    /// what it announces. Shared by the test classes of the main window.
    /// </summary>
    internal static class MainViewModelTestHelpers
    {
        // The clock every ViewModel under test runs on, so due dates are exact instead of "now".
        public static readonly DateTime Now = new(2025, 5, 1, 10, 30, 0);

        // How often the sidebar's view is rebuilt from now on. WPF reports
        // ICollectionView.Refresh() as a Reset, so that is the action counted.
        public static Func<int> SidebarRebuildsOf(MainViewModel viewModel)
        {
            int rebuilds = 0;
            ((INotifyCollectionChanged)CollectionViewSource.GetDefaultView(viewModel.PlantList.Plants)).CollectionChanged +=
                (_, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        rebuilds++;
                    }
                };

            return () => rebuilds;
        }

        // Everything the ViewModel announces from now on, in order.
        public static List<string> AnnouncementsOf(MainViewModel viewModel)
        {
            var announcements = new List<string>();
            viewModel.Announced += (_, text) => announcements.Add(text);
            return announcements;
        }

        // Builds a ViewModel on the given fakes; the ones a test does not mention are fresh defaults.
        public static MainViewModel CreateViewModel(
            FakePlantRepository? plantRepository = null,
            FakeDialogService? dialogService = null,
            FakeTimerService? timerService = null,
            FakeTimeProvider? timeProvider = null)
        {
            return new MainViewModel(
                plantRepository ?? new FakePlantRepository(),
                dialogService ?? new FakeDialogService(),
                timerService ?? new FakeTimerService(),
                timeProvider ?? new FakeTimeProvider(Now),
                new ThemeViewModel(new FakeThemeService(), new FakeSettingsService()));
        }

        // Same, but with the plants of the repository already loaded.
        public static async Task<MainViewModel> CreateInitializedViewModelAsync(
            FakePlantRepository? plantRepository = null,
            FakeDialogService? dialogService = null,
            FakeTimerService? timerService = null,
            FakeTimeProvider? timeProvider = null)
        {
            var viewModel = CreateViewModel(plantRepository, dialogService, timerService, timeProvider);
            await viewModel.PlantList.InitializeAsync();
            return viewModel;
        }

        // What the sidebar lists: the default view of Plants, which carries the search filter.
        public static List<string> VisiblePlantNames(MainViewModel viewModel)
        {
            return CollectionViewSource.GetDefaultView(viewModel.PlantList.Plants)
                .Cast<Plant>()
                .Select(plant => plant.Name)
                .ToList();
        }
    }
}
