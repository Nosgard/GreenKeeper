using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Repositories;
using GreenKeeper.Scheduling;
using GreenKeeper.Services;
using GreenKeeper.ViewModels.Base;
using GreenKeeper.ViewModels.CareStatuses.Abstract;
using GreenKeeper.ViewModels.CareStatuses.Active;
using GreenKeeper.ViewModels.CareStatuses.Passive;
using GreenKeeper.ViewModels.Themes;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
#if DEBUG
using GreenKeeper.ViewModels.TimeSimulation;
#endif

namespace GreenKeeper.ViewModels
{
    /// <summary>
    /// Backs the main window: the plant list with its search, the selected plant
    /// with its status cards, and every change to a plant's care. Windows are only
    /// requested through events - MainWindow opens them - so this class knows no
    /// window class and stays testable.
    /// </summary>
    public class MainViewModel : ObservableObject, IPlantCareEditor
    {
        // Keeps the due date texts and the Complete button's enabled state (IsCompletable)
        // up to date on their own.
        private static readonly TimeSpan CareStatusRefreshInterval = TimeSpan.FromMinutes(5);

        private readonly IPlantRepository _plantRepository;
        private readonly IDialogService _dialogService;
        private readonly ITimerService _timerService;
        private readonly TimeProvider _timeProvider;
        private Plant? _selectedPlant;
        private string _searchText = string.Empty;

        public MainViewModel(
            IPlantRepository plantRepository,
            IDialogService dialogService,
            ITimerService timerService,
            TimeProvider timeProvider,
            ThemeViewModel theme)
        {
            _plantRepository = plantRepository;
            _dialogService = dialogService;
            _timerService = timerService;
            _timeProvider = timeProvider;
            Theme = theme;

            // Register FilterPlants as the filter predicate for the default view of plants.
            // Since both the ListView and the operation are on the same underlying collection instance,
            // the ListView picks up this filter automatically, without needing to be changed itself.
            CollectionViewSource.GetDefaultView(Plants).Filter = FilterPlants;

            _timerService.Start(CareStatusRefreshInterval, RefreshCareStatuses);

            // The commands only signal the intent to open a window; MainWindow subscribes
            // to the events and opens it, so the ViewModel never sees a window class.
            AddPlantCommand = new RelayCommand(_ => AddPlantRequested?.Invoke(this, EventArgs.Empty));
            AddScheduleCommand = new RelayCommand(_ => AddScheduleRequested?.Invoke(this, SelectedPlant!), _ => IsPlantSelected);
            OpenNotesCommand = new RelayCommand(_ => OpenNotesRequested?.Invoke(this, SelectedPlant!), _ => IsPlantSelected);
            RenamePlantCommand = new RelayCommand(RequestRename, parameter => parameter is Plant);
            DeletePlantCommand = new AsyncRelayCommand(_ => DeleteSelectedPlantAsync(), _ => IsPlantSelected);

#if DEBUG
            TimeSimulation = new TimeSimulationViewModel(() => SelectedPlant, RefreshCareStatuses);
#endif
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
                    OnPropertyChanged(nameof(CareStatuses));
                    OnPropertyChanged(nameof(IsPlantSelected));
                }
            }
        }

        // Check if a plant is selected.
        // Useful for visibility bindings like the status title in the dashboard.
        public bool IsPlantSelected => SelectedPlant != null;

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

        // -- Care Status Section --

        /// <summary>
        /// One card per defined CareType. Depending on which care types are set,
        /// only the cards of the assigned care types will appear in the status.
        ///
        /// Important!
        /// The watering schedule is mandatory for all plants, which means that
        /// its status card is always visible.
        /// The fertilizing schedule and the separate sunlight requirement are optional
        /// and will not show up if they are not set for a plant.
        /// </summary>
        public IEnumerable<CareStatusViewModel> CareStatuses
        {
            get
            {
                if (SelectedPlant == null)
                {
                    yield break;
                }

                var plant = SelectedPlant;
                CareSchedule? ScheduleFor(CareType type) => plant.CareSchedules.FirstOrDefault(s => s.Care == type);

                // Watering: mandatory for every plant.
                yield return WateringCard(ScheduleFor(CareType.Watering));

                // Fertilizing: optional, only show the status if set for a plant.
                var fertilizingSchedule = ScheduleFor(CareType.Fertilizing);
                if (fertilizingSchedule != null)
                {
                    yield return FertilizingCard(fertilizingSchedule);
                }

                // Sunlight: optional.
                if (plant.SunlightRequirement != null)
                {
                    yield return SunlightCard(plant.SunlightRequirement);
                }
            }
        }

        // The cards get callbacks instead of the plant, so they know neither the plant nor any window.
        private WateringStatusViewModel WateringCard(CareSchedule? schedule)
        {
            return new WateringStatusViewModel(
                schedule,
                onComplete: () => CompleteCareScheduleAsync(CareType.Watering),
                onEdit: () => RequestEdit(CareType.Watering),
                _timeProvider);
        }

        private FertilizingStatusViewModel FertilizingCard(CareSchedule schedule)
        {
            return new FertilizingStatusViewModel(
                schedule,
                onComplete: () => CompleteCareScheduleAsync(CareType.Fertilizing),
                onEdit: () => RequestEdit(CareType.Fertilizing),
                onRemove: () => RemoveCareScheduleAsync(CareType.Fertilizing),
                _timeProvider);
        }

        private SunlightStatusViewModel SunlightCard(SunlightRequirement requirement)
        {
            return new SunlightStatusViewModel(
                requirement,
                onEdit: () => RequestEdit(CareType.Sunlight),
                onRemove: RemoveSunlightRequirementAsync);
        }

        private void RequestEdit(CareType careType)
        {
            EditScheduleRequested?.Invoke(this, (SelectedPlant!, careType));
        }

        /// <summary>
        /// Refreshes both the dashboard and the sidebar after any change to a plant's care schedules or sunlight requirement,
        /// e.g. after completing, adding, replacing or removing a schedule.
        ///
        /// Called from multiple places rather than relying on ObservableCollection notifications, because the change happens
        /// directly on an already-loaded plant object (Plant/CareSchedule don't implement INotifyPropertyChanged).
        /// Plants itself is never swapped or re-added to, so the UI would otherwise never learn that something changed.
        ///
        /// What both things do:
        ///
        /// OnPropertyChanged: updates the dashboard's status cards for the currently selected plant.
        ///
        /// CollectionViewSource: forces the sidebar's ListView to re-evaluate every item, which in turn re-runs PlantStatusDotConverter
        /// for each plant's status dot - without this, the dot would only ever update by coincidence (e.g. when the ListView happens
        /// to redraw for an unrelated reason).
        /// </summary>
        public void RefreshCareStatuses()
        {
            OnPropertyChanged(nameof(CareStatuses));
            CollectionViewSource.GetDefaultView(Plants).Refresh();
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

        // -- Window Requests Section --

        public ICommand AddPlantCommand { get; }
        public ICommand AddScheduleCommand { get; }
        public ICommand OpenNotesCommand { get; }
        public ICommand RenamePlantCommand { get; }

        // MainWindow.xaml.cs subscribes to these and opens the respective window (View).
        public event EventHandler? AddPlantRequested;
        public event EventHandler<Plant>? AddScheduleRequested;
        public event EventHandler<Plant>? OpenNotesRequested;
        public event EventHandler<Plant>? RenamePlantRequested;
        public event EventHandler<(Plant plant, CareType care)>? EditScheduleRequested;

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
        }

        /// <summary>
        /// Calculates NextDueAt/LastCaredAt for a new or replacing care schedule,
        /// persists it via the repository, then updates the local CareSchedules
        /// list so the status card appears immediately without a DB reload.
        /// </summary>
        public async Task AddOrReplaceCareScheduleAsync(CareSchedule careSchedule)
        {
            if (SelectedPlant == null || !careSchedule.HasInterval())
            {
                return;
            }

            careSchedule.StartIntervalAt(Now);

            var saved = await _plantRepository.AddOrReplaceCareScheduleAsync(SelectedPlant.Id, careSchedule);

            // Swap out any old local entry of the same care type for the saved one.
            SelectedPlant.CareSchedules.RemoveAll(s => s.Care == saved.Care);
            SelectedPlant.CareSchedules.Add(saved);

            RefreshCareStatuses();
        }

        /// <summary>
        /// Persists a new/replacing sunlight requirement for the selected plant -
        /// same principle as AddOrReplaceCareScheduleAsync, just no date calculation needed.
        /// </summary>
        public async Task AddOrReplaceSunlightRequirementAsync(SunlightRequirement sunlightRequirement)
        {
            if (SelectedPlant == null)
            {
                return;
            }

            var saved = await _plantRepository.AddOrReplaceSunlightRequirementAsync(SelectedPlant.Id, sunlightRequirement);
            SelectedPlant.SunlightRequirement = saved;

            RefreshCareStatuses();
        }

        /// <summary>
        /// Persists new notes text for the given plant, then updates the local,
        /// in-memory plant object so it reflects the saved state.
        /// </summary>
        public async Task UpdatePlantNotesAsync(Plant plant, string notes)
        {
            await _plantRepository.UpdatePlantNotesAsync(plant.Id, notes);
            plant.Notes = notes;
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
        }

        // -- Card and Button Actions Section --
        // Bound to commands, so nobody awaits them: a failure ends in an error dialog here.

        public ICommand DeletePlantCommand { get; }

        // Deletes the currently selected plant, after the user confirms.
        private async Task DeleteSelectedPlantAsync()
        {
            if (SelectedPlant == null)
            {
                return;
            }

            var plant = SelectedPlant;

            bool isConfirmed = _dialogService.Confirm(
                $"Are you sure you want to delete \"{plant.Name}\"? This cannot be undone.",
                "Delete Plant");

            if (!isConfirmed)
            {
                return;
            }

            bool isDeleted = await TryPersistAsync(
                () => _plantRepository.DeletePlantAsync(plant.Id),
                "The plant could not be deleted",
                "Delete Error");

            if (!isDeleted)
            {
                return;
            }

            Plants.Remove(plant);

            // After removing a plant, there is no "selected" plant.
            // Prevent the dashboard from presenting non-existent data.
            SelectedPlant = null;
        }

        /// <summary>
        /// Marks an active care schedule (Watering/Fertilizing) as "done now":
        /// calculates a new due date starting from the exact moment of the click,
        /// persists that new due date to the database, and then updates the local,
        /// in-memory copy so the status card reflects the change immediately.
        /// </summary>
        private async Task CompleteCareScheduleAsync(CareType careType)
        {
            var schedule = SelectedPlant?.CareSchedules.FirstOrDefault(s => s.Care == careType);

            // No saved amount or unit -> No calculation of a new interval.
            if (schedule == null || !schedule.HasInterval())
            {
                return;
            }

            // Calculated here, so the exact same values that get persisted to the database are
            // also the ones applied to the local object afterwards.
            var now = Now;
            var nextDueAt = schedule.NextDueDateFrom(now);

            bool isCompleted = await TryPersistAsync(
                () => _plantRepository.CompleteCareScheduleAsync(schedule.Id, nextDueAt, now),
                "The care schedule could not be updated");

            if (!isCompleted)
            {
                return;
            }

            schedule.NextDueAt = nextDueAt;
            schedule.LastCaredAt = now;

            RefreshCareStatuses();
        }

        // Removes an optional care schedule (Fertilizing) from the selected plant once the user has confirmed.
        private async Task RemoveCareScheduleAsync(CareType careType)
        {
            if (SelectedPlant == null)
            {
                return;
            }

            string scheduleName = careType.ScheduleNameInSentence();

            bool isConfirmed = _dialogService.Confirm(
                $"Are you sure you want to remove the {scheduleName} for \"{SelectedPlant.Name}\"?",
                $"Remove {careType.DisplayName()} Schedule");

            var schedule = SelectedPlant.CareSchedules.FirstOrDefault(s => s.Care == careType);

            if (!isConfirmed || schedule == null)
            {
                return;
            }

            bool isRemoved = await TryPersistAsync(
                () => _plantRepository.RemoveCareScheduleAsync(schedule.Id),
                $"The {scheduleName} could not be removed");

            if (!isRemoved)
            {
                return;
            }

            SelectedPlant.CareSchedules.Remove(schedule);

            // CareStatuses has no backing field and reads from the selected plant, so the
            // refresh is enough to let the removed status card disappear from the ItemsControl.
            RefreshCareStatuses();
        }

        // Removes the sunlight requirement from the selected plant once the user has confirmed.
        private async Task RemoveSunlightRequirementAsync()
        {
            if (SelectedPlant?.SunlightRequirement == null)
            {
                return;
            }

            var requirement = SelectedPlant.SunlightRequirement;

            bool isConfirmed = _dialogService.Confirm(
                $"Are you sure you want to remove the sunlight requirement for \"{SelectedPlant.Name}\"?",
                "Remove Sunlight Requirement");

            if (!isConfirmed)
            {
                return;
            }

            bool isRemoved = await TryPersistAsync(
                () => _plantRepository.RemoveSunlightRequirementAsync(requirement.Id),
                "The sunlight requirement could not be removed");

            if (!isRemoved)
            {
                return;
            }

            SelectedPlant.SunlightRequirement = null;

            RefreshCareStatuses();
        }

        // Runs a repository call and turns a failure into an error dialog. Returns whether it succeeded,
        // so the caller only touches the local objects after the database has taken the change.
        private async Task<bool> TryPersistAsync(Func<Task> operation, string failureText, string title = "Error")
        {
            try
            {
                await operation();
                return true;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"{failureText}:\n{ex.Message}", title);
                return false;
            }
        }

        private DateTime Now => _timeProvider.GetLocalNow().DateTime;
    }
}
