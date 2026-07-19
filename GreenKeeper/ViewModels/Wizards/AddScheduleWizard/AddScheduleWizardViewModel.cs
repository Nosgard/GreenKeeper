using GreenKeeper.Commands;
using GreenKeeper.Converters;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Services;
using GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps.Active;
using GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps.Base;
using GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps.Passive;
using GreenKeeper.ViewModels.Wizards.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.Wizards.AddScheduleWizard
{
    public class AddScheduleWizardViewModel : INotifyPropertyChanged
    {
        private readonly Plant _plant;
        private readonly IDialogService _dialogService;

        private readonly CareTypeSelectionStepViewModel _selectionStep = new();

        // Will be instantiated, once the user made his decision on the first step.
        // Unlike the AddPlantWizard there is no explicit order because the next step will be set dynamically
        private IWizardStepViewModel? _detailStep;

        public AddScheduleWizardViewModel(Plant plant, IDialogService dialogService)
        {
            _plant = plant;
            _dialogService = dialogService;

            CurrentStep = _selectionStep;

            NextCommand = new RelayCommand(
                execute: _ => GoNext(),
                canExecute: _ => CurrentStep.CanProceed);

            BackCommand = new RelayCommand(
                execute: _ => GoBack(),
                canExecute: _ => CurrentStep != _selectionStep);

            CancelCommand = new RelayCommand(
                execute: _ => Cancel());
        }

        private IWizardStepViewModel _currentStep = null!;
        public IWizardStepViewModel CurrentStep
        {
            get => _currentStep;
            private set
            {
                _currentStep = value;
                OnPropertyChanged(nameof(CurrentStep));
                (NextCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (BackCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool>? RequestClose;

        private void GoNext()
        {
            if (CurrentStep == _selectionStep)
            {
                // Set the next step depending on the selection in the previous step
                _detailStep = _selectionStep.SelectedCareType switch
                {
                    CareType.Water => new ScheduleActiveStepViewModel("Watering"),
                    CareType.Nutrients => new ScheduleActiveStepViewModel("Fertilizing"),
                    CareType.Sunlight => new ScheduleSunlightStepViewModel(),
                    _ => throw new ArgumentOutOfRangeException()
                };
                CurrentStep = _detailStep;
            }
            else
            {
                // "Finish" is trying to apply the status, but can abort
                // without closing the Wizard e.g. when the user enters "No" in the warning to overwrite
                if (CanApply())
                {
                    RequestClose?.Invoke(this, true);
                }
            }
        }

        private void GoBack()
        {
            // Back to the first step: The Detail-Step will be discarded.
            // If a value was entered, it's not going to be cached as it's not necessary for only two steps
            CurrentStep = _selectionStep;
        }

        private void Cancel()
        {
            RequestClose?.Invoke(this, false);
        }

        // Handle the entry in the Detail-Step.
        // In case the user refused to overwrite the Wizard remains open, otherwise it will be closed
        private bool CanApply()
        {
            return _selectionStep.SelectedCareType switch
            {
                CareType.Water => ApplyActiveCareSchedule(CareType.Water, (ScheduleActiveStepViewModel)_detailStep!),
                CareType.Nutrients => ApplyActiveCareSchedule(CareType.Nutrients, (ScheduleActiveStepViewModel)_detailStep!),
                CareType.Sunlight => ApplySunlightRequirement((ScheduleSunlightStepViewModel)_detailStep!),
                _ => false
            };
        }

        private bool ApplyActiveCareSchedule(CareType careType, ScheduleActiveStepViewModel step)
        {
            var existing = _plant.CareSchedules.FirstOrDefault(s => s.Care == careType);

            if (existing != null)
            {
                bool shouldReplace = _dialogService.Confirm(
                    $"There is already a {step.Title} schedule for this plant. Do you want to replace it?",
                    "Schedule already exists");

                if (!shouldReplace)
                {
                    // Keep the Wizard open
                    return false;
                }

                _plant.CareSchedules.Remove(existing);
            }

            int amount = int.Parse(step.AmountText);
            _plant.CareSchedules.Add(new CareSchedule
            {
                Care = careType,
                IntervalAmount = int.Parse(step.AmountText),
                IntervalUnit = step.SelectedUnit,
                NextDueAt = TimeUnitConverter.ToDueDate(DateTime.Now, amount, step.SelectedUnit)
            });

            return true;
        }

        private bool ApplySunlightRequirement(ScheduleSunlightStepViewModel step)
        {
            if (_plant.SunlightRequirement != null)
            {
                bool shouldReplace = _dialogService.Confirm(
                    "There is already a sunlight requirement for this plant. Do you want to replace it?",
                    "Sunlight requirement already exists");

                if (!shouldReplace)
                {
                    // Keep the Wizard open
                    return false;
                }
            }

            _plant.SunlightRequirement = new SunlightRequirement
            {
                Hours = int.Parse(step.AmountText),
                Period = step.SelectedPeriod
            };

            return true;
        }


        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
