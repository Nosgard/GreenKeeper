using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Repositories;
using GreenKeeper.Scheduling;
using GreenKeeper.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;

namespace GreenKeeper.ViewModels
{
    /// <summary>
    /// Backs the sidebar of the main window: the plant list with its search and the
    /// selection, and adding and renaming a plant. It remembers which state each entry
    /// shows, so the periodic refresh rebuilds the list only when a state changed.
    /// </summary>
    public class PlantListViewModel : ObservableObject
    {
        private readonly IPlantRepository _plantRepository;
        private readonly Action<string> _announce;
        private readonly Action _onSelectionChanged;

        // The state each plant's entry shows at the moment, so the periodic refresh can
        // tell whether rebuilding the sidebar would change anything at all.
        private readonly Dictionary<int, PlantStatus> _shownPlantStatus = new();
        private readonly TimeProvider _timeProvider;
        private Plant? _selectedPlant;
        private string _searchText = string.Empty;
        private bool _hasLoadedPlants;

        // announce: passes a sentence on to MainViewModel.Announced.
        // onSelectionChanged: lets the dashboard show the cards of the new selection.
        public PlantListViewModel(
            IPlantRepository plantRepository,
            TimeProvider timeProvider,
            Action<string> announce,
            Action onSelectionChanged)
        {
            _plantRepository = plantRepository;
            _timeProvider = timeProvider;
            _announce = announce;
            _onSelectionChanged = onSelectionChanged;

            // Register FilterPlants as the filter predicate for the default view of plants.
            // Since both the ListView and the operation are on the same underlying collection instance,
            // the ListView picks up this filter automatically, without needing to be changed itself.
            CollectionViewSource.GetDefaultView(Plants).Filter = FilterPlants;

            // The commands only signal the intent to open a window; MainWindow subscribes
            // to the events and opens it, so the ViewModel never sees a window class.
            AddPlantCommand = new RelayCommand(_ => AddPlantRequested?.Invoke(this, EventArgs.Empty));
            RenamePlantCommand = new RelayCommand(RequestRename, parameter => parameter is Plant);
        }

        /// <summary>
        /// Loads all plants from the database and fills the Plants collection with them.
        /// It needs to be called once after the constructor has been called
        /// (the constructor cannot use await).
        /// </summary>
        public async Task InitializeAsync()
        {
            var plants = await _plantRepository.GetPlantsAsync();
            foreach (var plant in plants)
            {
                Plants.Add(plant);
            }

            RememberPlantStatus();
            HasLoadedPlants = true;
        }

        /// <summary>
        /// True once InitializeAsync has filled Plants. Until then an empty list only means
        /// the plants are still being loaded, not that there are none.
        /// </summary>
        public bool HasLoadedPlants
        {
            get => _hasLoadedPlants;
            private set => SetProperty(ref _hasLoadedPlants, value);
        }

        // -- Plant List Section --

        // All available plants for the ListView.
        public ObservableCollection<Plant> Plants { get; } = new();

        // Selected plant in the ListView.
        public Plant? SelectedPlant
        {
            get => _selectedPlant;
            set
            {
                if (SetProperty(ref _selectedPlant, value))
                {
                    _onSelectionChanged();
                }
            }
        }

        /// <summary>
        /// Bound to the search option (TextBox in MainWindow.xaml).
        /// The setter fires on every single keystroke - not just when the
        /// TextBox loses focus. Combined with the Refresh() call, it produces
        /// the live search behavior (similar to a search engine).
        /// If a plant was selected, the selected plant will be set to null,
        /// as no selected plant that doesn't appear in the ListView during
        /// the search should keep its selected state.
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    SelectedPlant = null;

                    // Re-evaluates FilterPlants for every item in Plants using the NOW-updated SearchText.
                    CollectionViewSource.GetDefaultView(Plants).Refresh();

                    AnnounceSearchResult();
                }
            }
        }

        /// <summary>
        /// Filter predicate for the Plants CollectionView (see constructor above).
        /// Called once per plant every time Refresh() runs - returning true keeps
        /// the plant visible in the ListView, false hides it.
        ///
        /// Empty/whitespace-only search text -> every plant is shown
        /// (no filtering applied).
        /// Text entered: case-insensitive substring match against the plant's name.
        /// </summary>
        private bool FilterPlants(object item)
        {
            if (item is not Plant plant)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                return true;
            }

            return plant.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }

        // The list changes while the focus stays in the search box, so a screen reader
        // hears how many plants match.
        private void AnnounceSearchResult()
        {
            // An emptied search brings back the full list; nothing to report.
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                return;
            }

            int matchCount = Plants.Count(FilterPlants);

            _announce(matchCount switch
            {
                0 => "No plants found",
                1 => "1 plant found",
                _ => $"{matchCount} plants found"
            });
        }

        // Rebuilding the view re-runs PlantStatusDotConverter and PlantAccessibleNameConverter
        // for every entry, so the remembered states are the ones on screen afterwards.
        public void RefreshSidebar()
        {
            RememberPlantStatus();
            CollectionViewSource.GetDefaultView(Plants).Refresh();
        }

        private void RememberPlantStatus()
        {
            _shownPlantStatus.Clear();

            foreach (var plant in Plants)
            {
                _shownPlantStatus[plant.Id] = PlantStatusRules.For(plant, Now);
            }
        }

        public bool HasAnyPlantStatusChanged()
        {
            return Plants.Count != _shownPlantStatus.Count
                || Plants.Any(plant => !_shownPlantStatus.TryGetValue(plant.Id, out var shown)
                    || shown != PlantStatusRules.For(plant, Now));
        }

        // -- Window Requests Section --

        public ICommand AddPlantCommand { get; }
        public ICommand RenamePlantCommand { get; }

        // MainWindow.xaml.cs subscribes to these and opens the respective window (View).
        public event EventHandler? AddPlantRequested;
        public event EventHandler<Plant>? RenamePlantRequested;

        // The plant comes from the context menu of the sidebar entry (CommandParameter), not from the selection.
        private void RequestRename(object? parameter)
        {
            if (parameter is Plant plant)
            {
                RenamePlantRequested?.Invoke(this, plant);
            }
        }

        // -- Plant Changes Section --
        // Called by MainWindow with the result of a dialog, so the callers await them
        // in their own try/catch; the ViewModel only persists and updates the local objects.

        /// <summary>
        /// Persists a newly created plant (built by the Add Plant wizard)
        /// to the database via the repository, and only THEN adds it to
        /// the ObservableCollection that the sidebar's ListView is bound to.
        ///
        /// Doing it in this order matters: if AddPlantAsync (the repository call)
        /// were to fail - e.g. a database error - the new plant would never reach
        /// the UI either. This avoids a situation where the UI shows a plant that,
        /// in reality, was never actually saved.
        /// </summary>
        public async Task AddPlantAsync(Plant plant)
        {
            // The wizard only collects the intervals; the countdowns start now, at the moment of saving.
            var now = Now;
            foreach (var schedule in plant.CareSchedules.Where(s => s.HasInterval()))
            {
                schedule.StartIntervalAt(now);
            }

            var savedPlant = await _plantRepository.AddPlantAsync(plant);
            Plants.Add(savedPlant);

            _announce($"{savedPlant.Name} added");
        }

        /// <summary>
        /// Persists a plant's new name and updates the in-memory object so the
        /// sidebar reflects the change right away.
        ///
        /// Deliberately touches nothing but the name: care schedules, the sunlight requirement
        /// and all due dates stay exactly as they are. Renaming is purely cosmetic - it must
        /// never restart a countdown or otherwise disturb the plant's care state.
        /// </summary>
        public async Task RenamePlantAsync(Plant plant, string newName)
        {
            await _plantRepository.RenamePlantAsync(plant.Id, newName);
            plant.Name = newName;

            // Plant implements no INotifyPropertyChanged, so the ListView would
            // never notice the changed name on its own - same reason as for RefreshCareStatuses.
            CollectionViewSource.GetDefaultView(Plants).Refresh();

            // The dashboard header binds to SelectedPlant.Name, so it needs an
            // explicit nudge as well if the renamed plant happens to be selected.
            OnPropertyChanged(nameof(SelectedPlant));

            _announce($"Renamed to {newName}");
        }

        private DateTime Now => _timeProvider.GetLocalNow().DateTime;
    }
}
