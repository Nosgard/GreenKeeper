using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using GreenKeeper.ViewModels.Base;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.CareStatuses.EditOption
{
    /// <summary>
    /// Orchestrator for the edit dialog, analogous to the wizard ViewModels,
    /// but without step navigation - there is only ONE step, chosen
    /// once in the constructor based on the given care type.
    /// </summary>
    public class EditScheduleViewModel : ObservableObject, IDialogViewModel
    {
        public IScheduleInputStep CurrentStep { get; }

        // The entered values as a new object, instead of changing the plant. Null until saved.
        public ScheduleInput? Result { get; private set; }

        // Either an active (Watering / Fertilizing) or passive (Sunlight) care type.
        public EditScheduleViewModel(Plant plant, CareType careType, TimeProvider timeProvider)
        {
            CurrentStep = CreateStep(plant, careType, timeProvider);

            SaveCommand = new RelayCommand(
                execute: _ => Save(),
                canExecute: _ => CurrentStep.HasValidAmount);

            CancelCommand = new RelayCommand(
                execute: _ => RequestClose?.Invoke(this, false));
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool>? RequestClose;

        // Pre-fills the step from the plant's existing entry of that care type, if any.
        private static IScheduleInputStep CreateStep(Plant plant, CareType careType, TimeProvider timeProvider)
        {
            if (careType == CareType.Sunlight)
            {
                var requirement = plant.SunlightRequirement;
                return new EditSunlightViewModel(requirement?.Hours, requirement?.Period ?? SunlightPeriod.Day);
            }

            var schedule = plant.CareSchedules.FirstOrDefault(s => s.Care == careType);
            return new EditActiveScheduleViewModel(
                careType, schedule?.IntervalAmount, schedule?.IntervalUnit ?? TimeUnit.Days, timeProvider);
        }

        private void Save()
        {
            Result = CurrentStep.CreateInput();
            RequestClose?.Invoke(this, true);
        }
    }
}
