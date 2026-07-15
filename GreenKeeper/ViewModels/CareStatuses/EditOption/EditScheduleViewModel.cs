using GreenKeeper.Commands;
using GreenKeeper.Converters;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.CareStatuses.EditOption
{
    /// <summary>
    /// Orchestrator for the Edit-Dialog, analogous to the Wizard-ViewModels,
    /// but without a step navigation - there is only ONE step, chosen
    /// once in the constructor based on the given Care-Type
    /// </summary>
    public class EditScheduleViewModel : INotifyPropertyChanged
    {
        private readonly Plant _plant;
        private readonly CareType _careType;

        public object CurrentStep { get; }

        // Either an active (Watering /Fertilizing) or passive (Sunlight) Care-Type
        public EditScheduleViewModel(Plant plant, CareType careType)
        {
            _plant = plant;
            _careType = careType;

            if (careType == CareType.Sunlight)
            {
                // Pre-fill from the plant's existing Sunlight-Requirement, if any
                var requirement = plant.SunlightRequirement;
                CurrentStep = new EditSunlightViewModel(
                    requirement?.Hours,
                    requirement?.Period ?? SunlightPeriod.Day);
            }
            else
            {
                // Pre-fill from the matching IntervalAmount/IntervalUnit, if one
                // already exists for this plant and Care-Type
                var schedule = plant.CareSchedules.FirstOrDefault(s => s.Care == careType);
                string title = careType == CareType.Water ? "Watering" : "Fertilizing";
                CurrentStep = new EditActiveScheduleViewModel(
                    title,
                    schedule?.IntervalAmount,
                    schedule?.IntervalUnit ?? TimeUnit.Days);
            }

            SaveCommand = new RelayCommand(
                execute: _ => Save(),
                canExecute: _ => IsCurrentStepValid());

            CancelCommand = new RelayCommand(
                execute: _ => RequestClose?.Invoke(this, false));
        }

        // Delegates validity-check to whichever concrete step type is currently active
        private bool IsCurrentStepValid() => CurrentStep switch
        {
            EditActiveScheduleViewModel active => active.HasValidAmount,
            EditSunlightViewModel sunlight => sunlight.HasValidAmount,
            _ => false
        };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool>? RequestClose;

        // Apply the entered values directly onto the Plant-Object
        private void Save()
        {
            if (_careType == CareType.Sunlight)
            {
                // Create a new Sunlight-Requirement if none exists yet, otherwise update the existing one in place
                var step = (EditSunlightViewModel)CurrentStep;
                _plant.SunlightRequirement ??= new SunlightRequirement();
                _plant.SunlightRequirement.Hours = int.Parse(step.AmountText);
                _plant.SunlightRequirement.Period = step.SelectedPeriod;
            }
            else
            {
                var step = (EditActiveScheduleViewModel)CurrentStep;
                var schedule = _plant.CareSchedules.FirstOrDefault(s => s.Care == _careType);

                // Create a new Care-Schedule if none exists yet for this Care-Type
                if (schedule == null)
                {
                    schedule = new CareSchedule { Care = _careType };
                    _plant.CareSchedules.Add(schedule);
                }

                int amount = int.Parse(step.AmountText);
                schedule.IntervalAmount = amount;
                schedule.IntervalUnit = step.SelectedUnit;

                // The due date is recalculated starting from NOW, as agreed.
                // Editing resets the countdown, it does not just change the interval length while keeping the old due date - LastCaredAt as well
                schedule.NextDueAt = DateTime.Now.Add(TimeUnitConverter.ToTimeSpan(amount, step.SelectedUnit));
                schedule.LastCaredAt = DateTime.Now;
            }

            RequestClose?.Invoke(this, true);
        }

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
