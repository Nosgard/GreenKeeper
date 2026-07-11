using GreenKeeper.Commands;
using GreenKeeper.Converters;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.AddPlantWizard.Steps;
using GreenKeeper.ViewModels.AddPlantWizard.Steps.Active;
using GreenKeeper.ViewModels.AddPlantWizard.Steps.Passive;
using GreenKeeper.ViewModels.CareStatuses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GreenKeeper.ViewModels
{
    public class AddPlantWizardViewModel : INotifyPropertyChanged
    {
        // Collected data across all steps.
        // Will be turned into a Plant-Object by the end of the Wizard
        private readonly PlantNameStepViewModel _plantNameStepViewModel = new PlantNameStepViewModel();
        private readonly WateringStepViewModel _wateringStepViewModel = new WateringStepViewModel();
        private readonly FertilizingStepViewModel _fertilizingStepViewModel = new FertilizingStepViewModel();
        private readonly SunlightStepViewModel _sunlightStepViewModel = new SunlightStepViewModel();
        private readonly SummaryStepViewModel _summaryStepViewModel;

        private readonly List<IWizardStepViewModel> _steps;
        private int _currentStepIndex;

        public AddPlantWizardViewModel()
        {
            _summaryStepViewModel = new SummaryStepViewModel(
                _plantNameStepViewModel,
                _wateringStepViewModel,
                _fertilizingStepViewModel,
                _sunlightStepViewModel);

            // Order of the steps (ViewModels)
            _steps = new List<IWizardStepViewModel>
            {
                _plantNameStepViewModel,
                _wateringStepViewModel,
                _fertilizingStepViewModel,
                _sunlightStepViewModel,
                _summaryStepViewModel,
            };

            // Make the current step ready
            _currentStepIndex = 0;
            CurrentStep = _steps[_currentStepIndex];

            NextCommand = new RelayCommand(
                execute: _ => GoNext(),
                canExecute: _ => CurrentStep.CanProceed);

            BackCommand = new RelayCommand(
                execute: _ => GoBack(),
                canExecute: _ => _currentStepIndex > 0);

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

                // Once you reach the summary step, all values from the prior properties
                // must be read (e.g. in case the user presses the Back-Button an changes the values)
                if (value is SummaryStepViewModel summary)
                {
                    summary.Refresh();
                }
            }
        }

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand CancelCommand { get; }

        // Signalize the View, that the Wizard will be closed
        public event EventHandler<bool>? RequestClose;

        // After finishing the Wizard, the View reads the property, when RequestClose closed the window.
        // Only this ViewModel is allowed to set the created plant
        public Plant? CreatedPlant { get; private set; }

        private void GoNext()
        {
            if (_currentStepIndex < _steps.Count - 1)
            {
                _currentStepIndex++;
                CurrentStep = _steps[_currentStepIndex];
            }
            else
            {
                Finish();
            }
        }

        private void GoBack()
        {
            // Usually canExecute makes sure the Index never gets called at 0,
            // but once GoBack() will be called elsewhere in the future, cover this case
            if (_currentStepIndex > 0)
            {
                _currentStepIndex--;
                CurrentStep = _steps[_currentStepIndex];
            }
        }

        private void Finish()
        {
            CreatedPlant = BuildPlant();
            RequestClose?.Invoke(this, true);
        }

        private void Cancel()
        {
            RequestClose?.Invoke(this, false);
        }

        // Section for building a plant

        private Plant BuildPlant()
        {
            var plant = new Plant
            {
                Name = _plantNameStepViewModel.PlantName
            };
            
            // Watering: Mandatory field, so no further check is needed.
            // IntervalUnit: Saves the selected time unit for calculating the due date in hours later on (for more go to TimeUnitConverter -> ToDueDateText)
            // NextDueAt = now + time span calculated from the amount and unit in the related step
            plant.CareSchedules.Add(new CareSchedule
            {
                Care = CareType.Water,
                IntervalUnit = _wateringStepViewModel.SelectedUnit,
                NextDueAt = DateTime.Now.Add(TimeUnitConverter.ToTimeSpan(
                    int.Parse(_wateringStepViewModel.AmountText),
                    _wateringStepViewModel.SelectedUnit))
            });

            // Fertilizing: Optional, only add if the user didn't skip the step and entered a valid value
            if (_fertilizingStepViewModel.HasValidAmount)
            {
                plant.CareSchedules.Add(new CareSchedule
                {
                    Care = CareType.Nutrients,
                    IntervalUnit = _fertilizingStepViewModel.SelectedUnit,
                    NextDueAt = DateTime.Now.Add(TimeUnitConverter.ToTimeSpan(
                        int.Parse(_fertilizingStepViewModel.AmountText),
                        _fertilizingStepViewModel.SelectedUnit))
                });
            }

            // Sunlight: Optional. Unlike Watering/Fertilizing you don't need any calculation.
            // The Wizard already asks for values in the exact same structure (Hours + Period)
            if (_sunlightStepViewModel.HasValidAmount)
            {
                plant.SunlightRequirement = new SunlightRequirement
                {
                    Hours = int.Parse(_sunlightStepViewModel.AmountText),
                    Period = _sunlightStepViewModel.SelectedPeriod
                };
            }

            return plant;
        }

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
