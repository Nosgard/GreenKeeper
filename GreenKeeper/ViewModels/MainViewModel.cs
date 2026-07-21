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
        private ObservableCollection<Plant> _plants;

        // Set all plants for the ListView
        /// <summary>
        /// Important notes for OpenNotesCommand:
        /// Commands always need to be initialized in the constructor.
        /// The OpenNotesCommand is get-only, which means that if there
        /// is no allocation it remains 'null', the compiler will
        /// cause no alarm and WPF ignores the click on a bound command
        /// set to 'null'.
        /// 
        /// execute: Rules what happens after a click.
        /// You only fire an event (OpenNotesRequested), instead of opening
        /// a new window directly.
        /// -> Why? Because the ViewModel should not know anything about the view
        /// 
        /// canExecute: Controls the bound button whether to be enabled or disabled.
        /// If there is no plant selected, the button remains deactivated.
        /// </summary>
        /// <param name="plantRepository"></param>
        public MainViewModel(IPlantRepository plantRepository, IDialogService dialogService, ITimerService timerService)
        {
            _plantRepository = plantRepository;
            _dialogService = dialogService;
            _timerService = timerService;
            _plants = new ObservableCollection<Plant>(_plantRepository.GetPlants());


            // Register FilterPlants as the filter predicate for the default view of plants.
            // Since both the ListView and the operation are on the same underlying collection instance,
            // the ListView picks up this filter automatically, without needing to be changed itself
            CollectionViewSource.GetDefaultView(Plants).Filter = FilterPlants;

            // Periodically refreshes the Status-Cards, so due date texts and Complete-Button's
            // enabled state (IsCompletable) stay up to date automatically
            _timerService.Start(TimeSpan.FromMinutes(5), RefreshCareStatuses);

            // Add Plant Wizard related Command
            AddPlantCommand = new RelayCommand(
                execute: _ => AddPlantRequested?.Invoke(this, EventArgs.Empty));

            // Add Schedule Wizard related Command
            AddScheduleCommand = new RelayCommand(
                execute: _ => AddScheduleRequested?.Invoke(this, SelectedPlant!),
                canExecute: _ => SelectedPlant != null);

            // Delete Plant Button related Command
            DeletePlantCommand = new RelayCommand(
                execute: _ => DeleteSelectedPlant(),
                canExecute: _ => SelectedPlant != null);

            // Notes related Command
            OpenNotesCommand = new RelayCommand(
                execute: _ => OpenNotesRequested?.Invoke(this, SelectedPlant!),
                canExecute: _ => SelectedPlant != null);



            // -- Debug-Section --

#if DEBUG
            SimulateTimePassingCommand = new RelayCommand(
                execute: _ => SimulateTimePassing(),
                canExecute: _ => SelectedPlant != null);
#endif
        }

        // All available plants for the ListView (currently only dummy entries)
        public ObservableCollection<Plant> Plants
        {
            get { return _plants; }
            set { _plants = value; OnPropertyChanged(nameof(Plants)); }
        }

        /// <summary>
        /// One card per defined CareType. Depending on the set care types,
        /// only the cards of the allocated care types will appear in the status.
        /// 
        /// Important!
        /// The watering schedule is mandatory for all plants, which means that
        /// it's status card is always visible.
        /// The fertilizing schedule and the separate sunlight requirement are optional
        /// and will not show up if they are not set to a plant
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

                // Watering: mandatory for every plant
                yield return new WateringStatusViewModel(
                    ScheduleFor(CareType.Water),
                    onComplete: () => CompleteCareSchedule(CareType.Water),
                    onEdit: () => EditScheduleRequested?.Invoke(this, (SelectedPlant, CareType.Water)));

                // Fertilizing optional, only show the status if set to a plant
                var fertilizingSchedule = ScheduleFor(CareType.Nutrients);
                if (fertilizingSchedule != null)
                {
                    yield return new FertilizingStatusViewModel(
                        fertilizingSchedule,
                        onComplete: () => CompleteCareSchedule(CareType.Nutrients),
                        onEdit: () => EditScheduleRequested?.Invoke(this, (SelectedPlant, CareType.Nutrients)),
                        onRemove: () => RemoveCareSchedule(CareType.Nutrients, "fertilizing schedule"));
                }

                // Sunlight: optional
                if (SelectedPlant.SunlightRequirement != null)
                {
                    yield return new SunlightStatusViewModel(
                        SelectedPlant.SunlightRequirement,
                        onEdit: () => EditScheduleRequested?.Invoke(this, (SelectedPlant, CareType.Sunlight)),
                        onRemove: RemoveSunlightRequirement);
                }
            }
        }

        // Selected plant in the ListView (Currently only dummy entries)
        /// <summary>
        /// Reminder:
        /// An explicit "Requery" of OpenNotesCommand is not necessary.
        /// RelayCommand is hanging on CommandManager.RequerySuggested,
        /// which automatically requests CanExecute on most UI-interactions
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
        // Useful for Visibility-Bindings like the Status-Title in the Dashboard
        public bool IsPlantSelected => SelectedPlant != null;

        // Essential command to be bound to the Notes-Button in MainWIndow.xaml.
        // It is ICommand so that the View only binds the interface and
        // the explicit implementation remains exchangeable
        public ICommand OpenNotesCommand { get; }

        // Notify the View, that a new Notes-Window for the given plant should be opened.
        // Will be subscribed by MainWindow.xaml.cs (for more information, go there).
        // Code-Behind opens the window (View), while the ViewModel does not know any Window-Class
        public event EventHandler<Plant>? OpenNotesRequested;


        // Search Plant Section

        private string _searchText = string.Empty;

        /// <summary>
        /// Bound to the search option (TextBox in MainWindow.xaml).
        /// The setter fires on every single keystroke - not just when the
        /// TextBox loses focus. Combined with the Refresh() call, it produces
        /// the live search behavior (similar to a search engine).
        /// If a plant was selected, the selected plant will be set to null
        /// as no selected plant that doesn't appear in the ListView during
        /// the search should keep it's selected state
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

                // Re-evaluates FilterPlants for every item in Plants using the NOW-updated SearchText
                CollectionViewSource.GetDefaultView(Plants).Refresh();
            }
        }

        /// <summary>
        /// Filter predicate for the Plants CollectionView (see constructor above).
        /// Called once per plant every time Refresh() runs - returning true keeps
        /// the plant visible in the ListView, false hides it.
        /// 
        /// Empty/whitespace-only search text -> every plant is shown
        /// (no filtering applied)
        /// Text entered: case-insensitive substring match against the plant's name
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

        // Add Plant Wizard Section
        public ICommand AddPlantCommand { get; }
        public event EventHandler? AddPlantRequested;

        // Add a new plant to the ListView that was created in the Wizard.
        // No manual OnPropertyChanged needed because it's being added to the ObservableCollection
        public void AddPlant(Plant plant)
        {
            Plants.Add(plant);
        }

        // Add Schedule Wizard Section
        public ICommand AddScheduleCommand { get; }
        public event EventHandler<Plant>? AddScheduleRequested;

        // Delete Plant Button Section
        public ICommand DeletePlantCommand { get; }

        private void DeleteSelectedPlant()
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

            Plants.Remove(SelectedPlant);

            // After removing a plant, there is no "selected" plant.
            // Prevent the Dashboard from presenting non-existing data
            SelectedPlant = null;
        }

        // Care-Status related section

        /// <summary>
        /// Will be called after the AddScheduleWizard successfully applied a Care Schedule / Sunlight Requirement
        /// on the selected Plant-Object. Because the Change doesn't apply via the ObservableCollection
        /// (Plant was changed, not swapped) the UI doesn't need to be notified to show the new Status-Card
        /// </summary>
        public void RefreshCareStatuses()
        {
            OnPropertyChanged(nameof(CareStatuses));
        }

        // Care-Status Complete-Option
        // Marks an active Care-Schedule (Watering / Fertilizing) as "done now" and recalculates NextDueAt based on the exact moment of the click
        private void CompleteCareSchedule(CareType careType)
        {
            if (SelectedPlant == null)
            {
                return;
            }

            var schedule = SelectedPlant.CareSchedules.FirstOrDefault(s => s.Care == careType);
            if (schedule?.IntervalAmount == null || schedule.IntervalUnit == null)
            {
                // No saved amount or unit -> No calculation of a new interval
                return;
            }

            schedule.NextDueAt = TimeUnitConverter.ToDueDate(DateTime.Now, schedule.IntervalAmount.Value, schedule.IntervalUnit.Value);
            schedule.LastCaredAt = DateTime.Now;

            OnPropertyChanged(nameof(CareStatuses));
        }

        // Care-Status Edit-Option

        // Notify the View that the Edit-Dialog (EditScheduleView) must be opened for a specific Care-Type of the selected plant
        public event EventHandler<(Plant plant, CareType care)>? EditScheduleRequested;


        // Care-Status Remove-Option

        // Removes the optional care schedule from the selected plant once the user confirmed
        private void RemoveCareSchedule(CareType careType, string displayName)
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
            if (schedule != null)
            {
                SelectedPlant.CareSchedules.Remove(schedule);
            }

            // CareStatuses doesn't have any Backing-Field and reads from the selected plant (SelectedPlant).
            // OnProperty is enough to let the removed Status-Card disappear from the ItemsControl
            OnPropertyChanged(nameof(CareStatuses));
        }

        // Removes the sunlight requirement from the selected plant once the user confirmed
        private void RemoveSunlightRequirement()
        {
            if (SelectedPlant == null)
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

            SelectedPlant.SunlightRequirement = null;

            OnPropertyChanged(nameof(CareStatuses));
        }

        /// <summary>
        /// Stop the periodic Status-Card refresh (see _timerService.Start in the constructor).
        /// Called by MainWindow.xaml.cs when the main window is closed, so the timer doesn't
        /// keep running (and referencing this ViewModel) after the application would otherwise
        /// be shutting down
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

        // -- Debug-Section --

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
        /// Pulls the Time-Span of NextDueAt (and optionally LastCaredAt, if set)
        /// for ALL Care-Schedules of the selected plant except the Sunlight-Requirement
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

            OnPropertyChanged(nameof(CareStatuses));
        }
#endif
    }
}
