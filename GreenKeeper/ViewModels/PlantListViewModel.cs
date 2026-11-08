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
    /// selection, and adding and renaming a plant.
    /// </summary>
    public class PlantListViewModel : ObservableObject
    {
        private readonly IPlantRepository _plantRepository;
        private readonly Action<string> _announce;
        private readonly Action _onSelectionChanged;

        // The state each plant's entry shows at the moment, so the periodic refresh can tell
        // whether rebuilding the sidebar would change anything.
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

            // The plant list in the sidebar picks up this filter.
            CollectionViewSource.GetDefaultView(Plants).Filter = FilterPlants;

            AddPlantCommand = new RelayCommand(_ => AddPlantRequested?.Invoke(this, EventArgs.Empty));
            RenamePlantCommand = new RelayCommand(RequestRename, parameter => parameter is Plant);
        }

        /// <summary>
        /// Loads the plants from the database into Plants. MainWindow calls it once, when the
        /// window has loaded.
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

        public ObservableCollection<Plant> Plants { get; } = new();

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
        /// The text of the search box in the sidebar. The list filters with every keystroke, and
        /// the selection is cleared, since the selected plant may no longer be in the filtered list.
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    SelectedPlant = null;

                    CollectionViewSource.GetDefaultView(Plants).Refresh();

                    AnnounceSearchResult();
                }
            }
        }

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

        // The rebuild runs PlantStatusDotConverter and PlantAccessibleNameConverter again for
        // every entry, so the remembered states match the screen afterwards.
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

        // MainWindow handles these and opens the window each one asks for.
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
        // MainWindow calls these with a dialog's result and reports a failure through its RunAsync.

        /// <summary>
        /// Saves a plant from the Add Plant wizard and adds it to Plants only once the database
        /// has taken it, so the sidebar never shows a plant that was not saved.
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
        /// Saves the new name and updates the plant in memory, so the sidebar shows it at once.
        /// </summary>
        public async Task RenamePlantAsync(Plant plant, string newName)
        {
            await _plantRepository.RenamePlantAsync(plant.Id, newName);
            plant.Name = newName;

            // Plant raises no PropertyChanged, so without this the sidebar would show the old name.
            CollectionViewSource.GetDefaultView(Plants).Refresh();

            // The dashboard header binds to SelectedPlant.Name, and the renamed plant may be the
            // selected one.
            OnPropertyChanged(nameof(SelectedPlant));

            _announce($"Renamed to {newName}");
        }

        private DateTime Now => _timeProvider.GetLocalNow().DateTime;
    }
}
