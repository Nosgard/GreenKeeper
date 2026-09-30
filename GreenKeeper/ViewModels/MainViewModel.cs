using GreenKeeper.Commands;
using GreenKeeper.Converters;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Repositories;
using GreenKeeper.Services;
using GreenKeeper.ViewModels.CareStatuses;
using GreenKeeper.ViewModels.CareStatuses.Abstract;
using GreenKeeper.ViewModels.CareStatuses.Active;
using GreenKeeper.ViewModels.CareStatuses.Passive;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace GreenKeeper.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IPlantRepository _plantRepository;
        private readonly IDialogService _dialogService;
        private readonly ITimerService _timerService;
        private readonly IThemeService _themeService;
        private readonly ISettingsService _settingsService;
        private ObservableCollection<Plant> _plants;

        // Set all plants for the ListView.
        /// <summary>
        /// Important notes for OpenNotesCommand:
        /// Commands always need to be initialized in the constructor.
        /// The OpenNotesCommand is get-only, which means that if there
        /// is no assignment it remains 'null', the compiler will
        /// raise no warning and WPF ignores the click on a bound command
        /// set to 'null'.
        /// 
        /// execute: Defines what happens after a click.
        /// You only fire an event (OpenNotesRequested), instead of opening
        /// a new window directly.
        /// -> Why? Because the ViewModel should not know anything about the view.
        /// 
        /// canExecute: Controls whether the bound button is enabled or disabled.
        /// If there is no plant selected, the button remains deactivated.
        /// </summary>
        /// <param name="plantRepository"></param>
        public MainViewModel(IPlantRepository plantRepository, IDialogService dialogService, ITimerService timerService, IThemeService themeService, ISettingsService settingsService)
        {
            _plantRepository = plantRepository;
            _dialogService = dialogService;
            _timerService = timerService;
            _themeService = themeService;
            _settingsService = settingsService;
            _plants = new ObservableCollection<Plant>();


            // Register FilterPlants as the filter predicate for the default view of plants.
            // Since both the ListView and the operation are on the same underlying collection instance,
            // the ListView picks up this filter automatically, without needing to be changed itself.
            CollectionViewSource.GetDefaultView(Plants).Filter = FilterPlants;

            // Periodically refreshes the status cards, so due date texts and the Complete button's
            // enabled state (IsCompletable) stay up to date automatically.
            _timerService.Start(TimeSpan.FromMinutes(5), RefreshCareStatuses);

            // Command related to the theme
            ToggleThemeCommand = new RelayCommand(
                execute: _ => ToggleTheme());

            // Command related to the Add Plant wizard
            AddPlantCommand = new RelayCommand(
                execute: _ => AddPlantRequested?.Invoke(this, EventArgs.Empty));

            // Command related to the Add Schedule wizard
            AddScheduleCommand = new RelayCommand(
                execute: _ => AddScheduleRequested?.Invoke(this, SelectedPlant!),
                canExecute: _ => SelectedPlant != null);

            // Command related to the Delete Plant button
            DeletePlantCommand = new RelayCommand(
                execute: _ => DeleteSelectedPlant(),
                canExecute: _ => SelectedPlant != null);

            // Command related to the notes
            OpenNotesCommand = new RelayCommand(
                execute: _ => OpenNotesRequested?.Invoke(this, SelectedPlant!),
                canExecute: _ => SelectedPlant != null);

            // Command related to renaming a plant
            RenamePlantCommand = new RelayCommand(
                execute: parameter =>
                {
                    if (parameter is Plant plant)
                    {
                        RenamePlantRequested?.Invoke(this, plant);
                    }
                },
                canExecute: parameter => parameter is Plant);

            // -- Debug Section --

#if DEBUG
            SimulateTimePassingCommand = new RelayCommand(
                execute: _ => SimulateTimePassing(),
                canExecute: _ => SelectedPlant != null);
#endif
        }

        /// <summary>
        /// Loads all plants from the database and fills the Plants collection with them.
        /// It needs to be called once after the constructor has been called
        /// (The constructor cannot use await.)
        /// </summary>
        public async Task InitializeAsync()
        {
            var plants = await _plantRepository.GetPlantsAsync();
            foreach (var plant in plants)
            {
                Plants.Add(plant);
            }
        }

        // All available plants for the ListView.
        public ObservableCollection<Plant> Plants
        {
            get { return _plants; }
            set { _plants = value; OnPropertyChanged(nameof(Plants)); }
        }

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

                CareSchedule? ScheduleFor(CareType type) =>
                    SelectedPlant.CareSchedules.FirstOrDefault(s => s.Care == type);

                // Watering: mandatory for every plant.
                yield return new WateringStatusViewModel(
                    ScheduleFor(CareType.Watering),
                    onComplete: () => CompleteCareSchedule(CareType.Watering),
                    onEdit: () => EditScheduleRequested?.Invoke(this, (SelectedPlant, CareType.Watering)));

                // Fertilizing: optional, only show the status if set for a plant.
                var fertilizingSchedule = ScheduleFor(CareType.Fertilizing);
                if (fertilizingSchedule != null)
                {
                    yield return new FertilizingStatusViewModel(
                        fertilizingSchedule,
                        onComplete: () => CompleteCareSchedule(CareType.Fertilizing),
                        onEdit: () => EditScheduleRequested?.Invoke(this, (SelectedPlant, CareType.Fertilizing)),
                        onRemove: () => RemoveCareSchedule(CareType.Fertilizing, "fertilizing schedule"));
                }

                // Sunlight: optional.
                if (SelectedPlant.SunlightRequirement != null)
                {
                    yield return new SunlightStatusViewModel(
                        SelectedPlant.SunlightRequirement,
                        onEdit: () => EditScheduleRequested?.Invoke(this, (SelectedPlant, CareType.Sunlight)),
                        onRemove: RemoveSunlightRequirement);
                }
            }
        }

        // Selected plant in the ListView.
        /// <summary>
        /// Reminder:
        /// An explicit "Requery" of OpenNotesCommand is not necessary.
        /// RelayCommand is hooked into CommandManager.RequerySuggested,
        /// which automatically requests CanExecute on most UI interactions.
        /// </summary>
        private Plant? _SelectedPlant;
        public Plant? SelectedPlant
        {
            get { return _SelectedPlant; }
            set
            {
                _SelectedPlant = value;
                OnPropertyChanged(nameof(SelectedPlant));
                OnPropertyChanged(nameof(CareStatuses));
                OnPropertyChanged(nameof(IsPlantSelected));
            }
        }

        // Check if a plant is selected.
        // Useful for visibility bindings like the status title in the dashboard.
        public bool IsPlantSelected => SelectedPlant != null;

        // -- Theme Section --

        public ICommand ToggleThemeCommand { get; }

        /// <summary>
        /// True while the dark theme is active. The toggle button binds its icon
        /// to this property - showing a sun (switch TO bright) while dark is
        /// active, and a moon (switch TO dark) while bright is active.
        /// </summary>
        public bool IsDarkTheme => _themeService.CurrentTheme == Theme.Dark;

        private void ToggleTheme()
        {
            var newTheme = _themeService.CurrentTheme == Theme.Dark
                ? Theme.Bright
                : Theme.Dark;

            _themeService.ApplyTheme(newTheme);

            // Remember the choice for the next start. Read-modify-write rather than
            // writing a fresh object, so later settings are not dropped on the way.
            // Neither call throws - see SettingsService.
            var settings = _settingsService.Load();
            settings.Theme = newTheme;
            _settingsService.Save(settings);

            OnPropertyChanged(nameof(IsDarkTheme));
        }

        // -- Notes Section --

        // Essential command to be bound to the Notes button in MainWindow.xaml.
        // It is ICommand so that the View only binds the interface and
        // the explicit implementation remains exchangeable.
        public ICommand OpenNotesCommand { get; }

        // Notify the View that a new notes window for the given plant should be opened.
        // Will be subscribed to by MainWindow.xaml.cs (for more information, go there).
        // The code-behind opens the window (View), while the ViewModel does not know any window class.
        public event EventHandler<Plant>? OpenNotesRequested;

        /// <summary>
        /// Persists new notes text for the given plant, then updates the local,
        /// in-memory plant object so it reflects the saved state. Called via the
        /// callback that NotesViewModel receives (see NotesView/MainWindow) -
        /// not directly bound to a command, so this stays a plain async Task.
        /// </summary>
        public async Task UpdatePlantNotesAsync(Plant plant, string notes)
        {
            await _plantRepository.UpdatePlantNotesAsync(plant.Id, notes);
            plant.Notes = notes;
        }

        // -- Search Plant Section --

        private string _searchText = string.Empty;

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
                if (_searchText == value)
                {
                    return;
                }

                _searchText = value;
                SelectedPlant = null;
                OnPropertyChanged(nameof(SearchText));

                // Re-evaluates FilterPlants for every item in Plants using the NOW-updated SearchText.
                CollectionViewSource.GetDefaultView(Plants).Refresh();
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

        // -- Rename Plant Section --

        public ICommand RenamePlantCommand { get; }

        // Notifies the View that the rename dialog should be opened for a specific plant.
        // Follows the same pattern as EditScheduleRequested: the ViewModel only signals the
        // intent, while MainWindow.xaml.cs actually opens the window.
        public event EventHandler<Plant>? RenamePlantRequested;

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
        
        // -- Add Plant Wizard Section --
        public ICommand AddPlantCommand { get; }
        public event EventHandler? AddPlantRequested;

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
            var savedPlant = await _plantRepository.AddPlantAsync(plant);
            Plants.Add(savedPlant);
        }

        // -- Add Schedule Wizard Section --
        public ICommand AddScheduleCommand { get; }
        public event EventHandler<Plant>? AddScheduleRequested;

        /// <summary>
        /// Calculates NextDueAt/LastCaredAt for a new or replacing care schedule,
        /// persists it via the repository, then updates the local CareSchedules
        /// list so the status card appears immediately without a DB reload.
        /// Called directly from MainWindow (not via a command callback), so this
        /// stays a plain async Task - the caller awaits it in its own try/catch.
        /// </summary>
        public async Task AddOrReplaceCareScheduleAsync(CareSchedule newCareSchedule)
        {
            if ( (SelectedPlant == null || newCareSchedule.IntervalAmount == null || newCareSchedule.IntervalUnit == null))
            {
                return;
            }

            newCareSchedule.NextDueAt = TimeUnitConverter.ToDueDate(DateTime.Now, newCareSchedule.IntervalAmount.Value, newCareSchedule.IntervalUnit.Value);
            newCareSchedule.LastCaredAt = DateTime.Now;

            var saved = await _plantRepository.AddOrReplaceCareScheduleAsync(SelectedPlant.Id, newCareSchedule);

            // Swap out any old local entry of the same care type for the saved one.
            var existingLocal = SelectedPlant.CareSchedules.FirstOrDefault(s => s.Care == saved.Care);
            if (existingLocal != null)
            {
                SelectedPlant.CareSchedules.Remove(existingLocal);
            }
            SelectedPlant.CareSchedules.Add(saved);

            RefreshCareStatuses();
        }

        /// <summary>
        /// Persists a new/replacing sunlight requirement for the selected plant -
        /// same principle as AddOrReplaceCareScheduleAsync, just no date calculation needed.
        /// </summary>
        public async Task AddOrReplaceSunlightRequirementAsync(SunlightRequirement newSunlightRequirement)
        {
            if (SelectedPlant == null)
            {
                return;
            }

            var saved = await _plantRepository.AddOrReplaceSunlightRequirementAsync(SelectedPlant.Id, newSunlightRequirement);
            SelectedPlant.SunlightRequirement = saved;

            RefreshCareStatuses();
        }

        // -- Delete Plant Button Section --
        public ICommand DeletePlantCommand { get; }

        
        // Deletes the currently selected plant, after the user confirms.
        private async void DeleteSelectedPlant()
        {
            if (SelectedPlant == null)
            {
                return;
            }

            bool isConfirmed = _dialogService.Confirm(
                $"Are you sure you want to delete \"{SelectedPlant.Name}\"? This cannot be undone.",
                "Delete Plant");

            if (!isConfirmed)
            {
                return;
            }

            try
            {
                await _plantRepository.DeletePlantAsync(SelectedPlant.Id);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"The plant could not be deleted:\n{ex.Message}",
                    "Delete Error");
                return;
            }

            Plants.Remove(SelectedPlant);

            // After removing a plant, there is no "selected" plant.
            // Prevent the dashboard from presenting non-existent data.
            SelectedPlant = null;
        }

        // -- Care Status Section --

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
        /// Marks an active care schedule (Watering/Fertilizing) as "done now":
        /// calculates a new due date starting from the exact moment of the click,
        /// persists that new due date to the database, and then updates the local,
        /// in-memory copy so the status card reflects the change immediately.
        /// </summary>
        private async void CompleteCareSchedule(CareType careType)
        {
            if (SelectedPlant == null)
            {
                return;
            }

            var schedule = SelectedPlant.CareSchedules.FirstOrDefault(s => s.Care == careType);
            if (schedule?.IntervalAmount == null || schedule.IntervalUnit == null)
            {
                // No saved amount or unit -> No calculation of a new interval.
                return;
            }

            // Calculated here, so the exact same values that get persisted to the database are
            // also the ones applied to the local object afterwards.
            var newNextDueAt = TimeUnitConverter.ToDueDate(DateTime.Now, schedule.IntervalAmount.Value, schedule.IntervalUnit.Value);
            var newLastCaredAt = DateTime.Now;

            try
            {
                await _plantRepository.CompleteCareScheduleAsync(schedule.Id, newNextDueAt, newLastCaredAt);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"The Care-Schedule could not be updated:\n{ex.Message}",
                    "Error");
                return;
            }

            schedule.NextDueAt = newNextDueAt;
            schedule.LastCaredAt = newLastCaredAt;

            RefreshCareStatuses();
        }

        // Care Status Edit Option

        // Notify the View that the edit dialog (EditScheduleView) must be opened for a specific care type of the selected plant.
        public event EventHandler<(Plant plant, CareType care)>? EditScheduleRequested;


        // Care Status Remove Option

        /// <summary>
        /// Removes the optional care schedule from the selected plant once the user has confirmed.
        /// "async void" because onRemove is wired up via a synchronous Action delegate
        /// in CareStatuses, so error handling must happen entirely within this method via try/catch,
        /// since the caller of an async void method cannot catch exceptions from it.
        /// </summary>
        private async void RemoveCareSchedule(CareType careType, string displayName)
        {
            if (SelectedPlant == null)
            {
                return;
            }

            bool isConfirmed = _dialogService.Confirm(
                $"Are you sure you want to remove the {displayName} for \"{SelectedPlant.Name}\"?",
                "Remove Schedule");

            if (!isConfirmed)
            {
                return;
            }

            var schedule = SelectedPlant.CareSchedules.FirstOrDefault(s => s.Care == careType);
            if (schedule == null)
            {
                return;
            }

            try
            {
                await _plantRepository.RemoveCareScheduleAsync(schedule.Id);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"The {displayName} could not be removed:\n{ex.Message}",
                    "Error");
                return;
            }

            SelectedPlant.CareSchedules.Remove(schedule);

            // CareStatuses doesn't have any backing field and reads from the selected plant (SelectedPlant).
            // The call to RefreshCareStatuses is enough to let the removed status card disappear from the ItemsControl.
            RefreshCareStatuses();
        }

        /// <summary>
        /// Removes the sunlight requirement from the selected plant once the user has confirmed.
        /// Same "async void" reasoning as RemoveCareSchedule.
        /// </summary>
        private async void RemoveSunlightRequirement()
        {
            if (SelectedPlant?.SunlightRequirement == null)
            {
                return;
            }

            bool isConfirmed = _dialogService.Confirm(
                $"Are you sure you want to remove the sunlight requirement for \"{SelectedPlant.Name}\"?",
                "Remove Sunlight Requirement");

            if (!isConfirmed)
            {
                return;
            }

            try
            {
                await _plantRepository.RemoveSunlightRequirementAsync(SelectedPlant.SunlightRequirement.Id);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"The Sunlight-Requirement could not be removed:\n{ex.Message}",
                    "Error");
                return;
            }

            SelectedPlant.SunlightRequirement = null;

            RefreshCareStatuses();
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

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // -- Debug Section --

#if DEBUG
        public bool IsDebugBuild => true;
#else
        public bool IsDebugBuild => false;
#endif

#if DEBUG
        public ICommand SimulateTimePassingCommand { get; }

        // Available jumps in the UI
        public IReadOnlyList<KeyValuePair<TimeUnit, string>> AvailableSimulationUnits { get; } =
            new List<KeyValuePair<TimeUnit, string>>
            {
                new(TimeUnit.Days, "Days"),
                new(TimeUnit.Weeks, "Weeks"),
            };

        private TimeUnit _simulationUnit = TimeUnit.Days;
        public TimeUnit SimulationUnit
        {
            get => _simulationUnit;
            set {
                _simulationUnit = value;
                OnPropertyChanged(nameof(SimulationUnit));
            }
        }

        private string _simulationAmountText = "1";
        public string SimulationAmountText
        {
            get => _simulationAmountText;
            set {
                _simulationAmountText = value;
                OnPropertyChanged(nameof(SimulationAmountText));
            }
        }

        /// <summary>
        /// Subtracts the time span from NextDueAt (and optionally LastCaredAt, if set)
        /// for ALL care schedules of the selected plant except the sunlight requirement.
        /// </summary>
        private void SimulateTimePassing()
        {
            if (SelectedPlant == null)
            {
                return;
            }

            if (!int.TryParse(SimulationAmountText, out int amount) || amount <= 0)
            {
                return;
            }

            var span = TimeUnitConverter.ToTimeSpan(amount, SimulationUnit);

            foreach (var schedule in SelectedPlant.CareSchedules)
            {
                if (schedule.NextDueAt.HasValue)
                {
                    schedule.NextDueAt = schedule.NextDueAt.Value.Subtract(span);
                }
                if (schedule.LastCaredAt.HasValue)
                {
                    schedule.LastCaredAt = schedule.LastCaredAt.Value.Subtract(span);
                }
            }

            RefreshCareStatuses();
        }
#endif
    }
}
