#if DEBUG
using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using GreenKeeper.ViewModels.Base;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.TimeSimulation
{
    /// <summary>
    /// Backs the debug panel of the dashboard: fast-forwards the due dates of
    /// the selected plant by a number of days or weeks, so overdue states can
    /// be tried out without waiting. Only exists in DEBUG builds.
    /// </summary>
    public class TimeSimulationViewModel : ObservableObject
    {
        private readonly Func<Plant?> _selectedPlant;
        private readonly Action _onSimulated;
        private TimeUnit _unit = TimeUnit.Days;
        private string _amountText = "1";

        // selectedPlant: the plant to fast-forward, read at the time of the click.
        // onSimulated: lets the dashboard refresh its status cards afterwards.
        public TimeSimulationViewModel(Func<Plant?> selectedPlant, Action onSimulated)
        {
            _selectedPlant = selectedPlant;
            _onSimulated = onSimulated;

            FastForwardCommand = new RelayCommand(
                execute: _ => FastForward(),
                canExecute: _ => _selectedPlant() != null);
        }

        public ICommand FastForwardCommand { get; }

        // Available jumps in the UI.
        public IReadOnlyList<KeyValuePair<TimeUnit, string>> AvailableUnits { get; } =
            new List<KeyValuePair<TimeUnit, string>>
            {
                new(TimeUnit.Days, "Days"),
                new(TimeUnit.Weeks, "Weeks"),
            };

        public TimeUnit Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        public string AmountText
        {
            get => _amountText;
            set => SetProperty(ref _amountText, value);
        }

        /// <summary>
        /// Subtracts the time span from NextDueAt (and optionally LastCaredAt, if set)
        /// for ALL care schedules of the selected plant except the sunlight requirement.
        /// </summary>
        private void FastForward()
        {
            var plant = _selectedPlant();

            if (plant == null || !int.TryParse(AmountText, out int amount) || amount <= 0)
            {
                return;
            }

            var span = DueDateCalculator.ToTimeSpan(amount, Unit);

            foreach (var schedule in plant.CareSchedules)
            {
                schedule.NextDueAt = schedule.NextDueAt?.Subtract(span);
                schedule.LastCaredAt = schedule.LastCaredAt?.Subtract(span);
            }

            _onSimulated();
        }
    }
}
#endif
