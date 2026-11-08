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
    /// </summary>
    public class DashboardViewModel : ObservableObject, IPlantCareEditor
    {
        private readonly IPlantRepository _plantRepository;
        private readonly IDialogService _dialogService;
        private readonly TimeProvider _timeProvider;
        private readonly PlantListViewModel _plantList;
        private readonly Action<string> _announce;

        // The cards of the selected plant. The periodic refresh updates them in place, so a
        // screen reader stays on its element (see MainViewModel.RefreshDueStatus).
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

            AddScheduleCommand = new RelayCommand(_ => AddScheduleRequested?.Invoke(this, SelectedPlant!), _ => IsPlantSelected);
            OpenNotesCommand = new RelayCommand(_ => OpenNotesRequested?.Invoke(this, SelectedPlant!), _ => IsPlantSelected);
            DeletePlantCommand = new AsyncRelayCommand(_ => DeleteSelectedPlantAsync(), _ => IsPlantSelected);
        }

        private Plant? SelectedPlant => _plantList.SelectedPlant;

        public bool IsPlantSelected => SelectedPlant != null;

        /// <summary>
        /// Shows the cards of the plant now selected in the sidebar. The SelectedPlant setter
        /// of PlantListViewModel calls it through onSelectionChanged.
        /// </summary>
        public void ShowSelectedPlant()
        {
            RebuildCareStatuses();
            OnPropertyChanged(nameof(IsPlantSelected));
        }

        // What the card of the care type reads at this moment, the way a screen reader says it.
        private string SpokenStatusOf(CareType careType)
        {
            return CareStatuses.First(card => card.Care == careType).SpokenStatusText;
        }

        // -- Care Status Section --

        /// <summary>
        /// The cards of the selected plant: Watering always, since it is mandatory, Fertilizing
        /// and Sunlight only when the plant has them.
        /// </summary>
        public IEnumerable<CareStatusViewModel> CareStatuses => _careStatuses;

        // Builds the cards of the selected plant. Call it after the plant or its data changed,
        // because a card holds the care schedule or sunlight requirement it was built on, and
        // AddOrReplace puts a new object in its place.
        private void RebuildCareStatuses()
        {
            _careStatuses.Clear();

            if (SelectedPlant == null)
            {
                return;
            }

            var plant = SelectedPlant;
            CareSchedule? ScheduleFor(CareType type) => plant.CareSchedules.FirstOrDefault(s => s.Care == type);

            _careStatuses.Add(WateringCard(ScheduleFor(CareType.Watering)));

            var fertilizingSchedule = ScheduleFor(CareType.Fertilizing);
            if (fertilizingSchedule != null)
            {
                _careStatuses.Add(FertilizingCard(fertilizingSchedule));
            }

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
        /// Rebuilds the cards and the sidebar after a change to a plant's care. The status dot
        /// and the spoken name follow only through RefreshSidebar, because Plant and
        /// CareSchedule raise no PropertyChanged.
        /// </summary>
        public void RefreshCareStatuses()
        {
            RebuildCareStatuses();
            _plantList.RefreshSidebar();
        }

        // -- Window Requests Section --

        public ICommand AddScheduleCommand { get; }
        public ICommand OpenNotesCommand { get; }

        // MainWindow handles these and opens the window each one asks for.
        public event EventHandler<Plant>? AddScheduleRequested;
        public event EventHandler<Plant>? OpenNotesRequested;
        public event EventHandler<(Plant plant, CareType care)>? EditScheduleRequested;

        // -- Plant Changes Section --
        // Called with a dialog's result. MainWindow reports a failure through RunAsync; for
        // UpdatePlantNotesAsync the notes window does that.

        /// <summary>
        /// Saves the schedule with its interval starting now and puts the saved one into the
        /// plant's CareSchedules, so the card shows it without reloading the plants.
        /// </summary>
        public async Task AddOrReplaceCareScheduleAsync(CareSchedule careSchedule)
        {
            if (SelectedPlant == null || !careSchedule.HasInterval())
            {
                return;
            }

            careSchedule.StartIntervalAt(Now);

            var saved = await _plantRepository.AddOrReplaceCareScheduleAsync(SelectedPlant.Id, careSchedule);

            SelectedPlant.CareSchedules.RemoveAll(s => s.Care == saved.Care);
            SelectedPlant.CareSchedules.Add(saved);

            RefreshCareStatuses();
            _announce($"{saved.Care.DisplayName()} schedule saved. {SpokenStatusOf(saved.Care)}");
        }

        /// <summary>
        /// Saves a new or replacing sunlight requirement for the selected plant, like
        /// AddOrReplaceCareScheduleAsync but without a due date to calculate.
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

        public async Task UpdatePlantNotesAsync(Plant plant, string notes)
        {
            await _plantRepository.UpdatePlantNotesAsync(plant.Id, notes);
            plant.Notes = notes;
        }

        // -- Card and Button Actions Section --
        // Bound to commands, so nobody awaits them: a failure ends in an error dialog here.

        public AsyncRelayCommand DeletePlantCommand { get; }

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

            _plantList.SelectedPlant = null;

            _announce($"{plant.Name} deleted");
        }

        /// <summary>
        /// Completes an active care schedule (Watering, Fertilizing): the next due date counts
        /// from the moment of the click.
        /// </summary>
        private async Task CompleteCareScheduleAsync(CareType careType)
        {
            var schedule = SelectedPlant?.CareSchedules.FirstOrDefault(s => s.Care == careType);

            if (schedule == null || !schedule.HasInterval())
            {
                return;
            }

            // Calculated once, so the database and the local object afterwards get the same values.
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

            RefreshCareStatuses();
            _announce($"{careType.DisplayName()} schedule removed");
        }

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
