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
    /// Orchestrator for the edit dialog, analogous to the wizard ViewModels,
    /// but without step navigation - there is only ONE step, chosen
    /// once in the constructor based on the given care type.
    /// </summary>
    public class EditScheduleViewModel : INotifyPropertyChanged
    {
        private readonly Plant _plant;
        private readonly CareType _careType;

        public object CurrentStep { get; }

        // Provide the result, instead of mutating _plant
        public CareSchedule? EditedCareSchedule { get; private set; }
        public SunlightRequirement? EditedSunlightRequirement { get; private set; }

        // Either an active (Watering / Fertilizing) or passive (Sunlight) care type
        public EditScheduleViewModel(Plant plant, CareType careType)
        {
            _plant = plant;
            _careType = careType;

            if (careType == CareType.Sunlight)
            {
                // Pre-fill from the plant's existing sunlight requirement, if any
                var requirement = plant.SunlightRequirement;
                CurrentStep = new EditSunlightViewModel(
                    requirement?.Hours,
                    requirement?.Period ?? SunlightPeriod.Day);
            }
            else
            {
                // Pre-fill from the matching IntervalAmount/IntervalUnit, if one
                // already exists for this plant and care type
                var schedule = plant.CareSchedules.FirstOrDefault(s => s.Care == careType);
                string title = careType == CareType.Watering ? "Watering" : "Fertilizing";
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

        // Delegates the validity check to whichever concrete step type is currently active
        private bool IsCurrentStepValid() => CurrentStep switch
        {
            EditActiveScheduleViewModel active => active.HasValidAmount,
            EditSunlightViewModel sunlight => sunlight.HasValidAmount,
            _ => false
        };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool>? RequestClose;

        // Hand the entered values back as a new object instead of changing the plant object
        private void Save()
        {
            if (_careType == CareType.Sunlight)
            {
                // Always create a new sunlight requirement - the repository replaces the existing one
                var step = (EditSunlightViewModel)CurrentStep;
                EditedSunlightRequirement = new SunlightRequirement
                {
                    Hours = int.Parse(step.AmountText),
                    Period = step.SelectedPeriod
                };
            }
            else
            {
                var step = (EditActiveScheduleViewModel)CurrentStep;

                EditedCareSchedule = new CareSchedule
                {
                    Care = _careType,
                    IntervalAmount = int.Parse(step.AmountText),
                    IntervalUnit = step.SelectedUnit
                };
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
