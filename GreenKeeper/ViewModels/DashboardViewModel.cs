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
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GreenKeeper.ViewModels
{
    /// <summary>
    /// Backs the dashboard of the main window: the status cards of the plant selected in
    /// the sidebar and every change to that plant's care, its notes and its deletion.
    /// Windows are only requested through events and MainWindow opens them, so this class
    /// knows no window class and stays testable.
    /// </summary>
    public class DashboardViewModel : ObservableObject, IPlantCareEditor
    {
        private readonly IPlantRepository _plantRepository;
        private readonly IDialogService _dialogService;
        private readonly TimeProvider _timeProvider;
        private readonly PlantListViewModel _plantList;
        private readonly Action<string> _announce;

        // The cards of the selected plant. One kept collection, so a card that is still
        // correct keeps its object, and with it the element a screen reader is on.
        private readonly ObservableCollection<CareStatusViewModel> _careStatuses = new();

        // plantList: owns the selection and the sidebar, which a change to a plant's care refreshes.
        // announce: passes a sentence on to MainViewModel.Announced.
        public DashboardViewModel(
            IPlantRepository plantRepository,
            IDialogService dialogService,
            TimeProvider timeProvider,
            PlantListViewModel plantList,
            Action<string> announce)
        {
            _plantRepository = plantRepository;
            _dialogService = dialogService;
            _timeProvider = timeProvider;
            _plantList = plantList;
            _announce = announce;

            // The commands only signal the intent to open a window; MainWindow subscribes
            // to the events and opens it, so the ViewModel never sees a window class.
            AddScheduleCommand = new RelayCommand(_ => AddScheduleRequested?.Invoke(this, SelectedPlant!), _ => IsPlantSelected);
            OpenNotesCommand = new RelayCommand(_ => OpenNotesRequested?.Invoke(this, SelectedPlant!), _ => IsPlantSelected);
            DeletePlantCommand = new AsyncRelayCommand(_ => DeleteSelectedPlantAsync(), _ => IsPlantSelected);
        }

        // The sidebar owns the selection, the dashboard only reads it.
        private Plant? SelectedPlant => _plantList.SelectedPlant;

        // Check if a plant is selected.
        // Useful for visibility bindings like the status title in the dashboard.
        public bool IsPlantSelected => SelectedPlant != null;

        /// <summary>
        /// Shows the cards of the plant just selected in the sidebar. MainViewModel hands
        /// this to PlantListViewModel, whose SelectedPlant setter calls it after reporting
        /// the new selection.
        /// </summary>
        public void ShowSelectedPlant()
        {
            RebuildCareStatuses();
            OnPropertyChanged(nameof(CareStatuses));
            OnPropertyChanged(nameof(IsPlantSelected));
        }

        // What the card of the care type reads at this moment, the way a screen reader says it.
        private string SpokenStatusOf(CareType careType)
        {
            return CareStatuses.First(card => card.Care == careType).SpokenStatusText;
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
        public IEnumerable<CareStatusViewModel> CareStatuses => _careStatuses;

        // Builds the cards of the selected plant, only when the plant or its data changed:
        // a card holds the schedule it was built on, and AddOrReplace puts a new schedule
        // object in its place. The periodic refresh keeps the cards (see MainViewModel.RefreshDueStatus).
        private void RebuildCareStatuses()
        {
            _careStatuses.Clear();

            if (SelectedPlant == null)
            {
                return;
            }

            var plant = SelectedPlant;
            CareSchedule? ScheduleFor(CareType type) => plant.CareSchedules.FirstOrDefault(s => s.Care == type);

            // Watering: mandatory for every plant.
            _careStatuses.Add(WateringCard(ScheduleFor(CareType.Watering)));

            // Fertilizing: optional, only show the status if set for a plant.
            var fertilizingSchedule = ScheduleFor(CareType.Fertilizing);
            if (fertilizingSchedule != null)
            {
                _careStatuses.Add(FertilizingCard(fertilizingSchedule));
            }

            // Sunlight: optional.
            if (plant.SunlightRequirement != null)
            {
                _careStatuses.Add(SunlightCard(plant.SunlightRequirement));
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
        /// and PlantAccessibleNameConverter for each plant's status dot and spoken name - without this, both would only ever update
        /// by coincidence (e.g. when the ListView happens to redraw for an unrelated reason).
        /// </summary>
        public void RefreshCareStatuses()
        {
            RebuildCareStatuses();
            OnPropertyChanged(nameof(CareStatuses));
            _plantList.RefreshSidebar();
        }

        // -- Window Requests Section --

        public ICommand AddScheduleCommand { get; }
        public ICommand OpenNotesCommand { get; }

        // MainWindow.xaml.cs subscribes to these and opens the respective window (View).
        public event EventHandler<Plant>? AddScheduleRequested;
        public event EventHandler<Plant>? OpenNotesRequested;
        public event EventHandler<(Plant plant, CareType care)>? EditScheduleRequested;

        // -- Plant Changes Section --
        // Called by MainWindow with the result of a dialog, so the callers await them
        // in their own try/catch; the ViewModel only persists and updates the local objects.

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
            _announce($"{saved.Care.DisplayName()} schedule saved. {SpokenStatusOf(saved.Care)}");
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
            _announce($"Sunlight requirement saved. {SpokenStatusOf(CareType.Sunlight)}");
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
                "Delete Plant",
                safeAnswer: Answer.No);

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

            _plantList.Plants.Remove(plant);

            // After removing a plant, there is no "selected" plant.
            // Prevent the dashboard from presenting non-existent data.
            _plantList.SelectedPlant = null;

            _announce($"{plant.Name} deleted");
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
            _announce($"{careType.DisplayName()} completed. {SpokenStatusOf(careType)}");
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
                $"Remove {careType.DisplayName()} Schedule",
                safeAnswer: Answer.No);

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

            // The refresh rebuilds the cards of the selected plant, so the removed card disappears.
            RefreshCareStatuses();
            _announce($"{careType.DisplayName()} schedule removed");
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
                "Remove Sunlight Requirement",
                safeAnswer: Answer.No);

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
            _announce("Sunlight requirement removed");
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
