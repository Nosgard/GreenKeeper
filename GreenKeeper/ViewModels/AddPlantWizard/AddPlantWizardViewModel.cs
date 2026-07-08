using GreenKeeper.Commands;
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

        private readonly List<IWizardStepViewModel> _steps;
        private int _currentStepIndex;

        public AddPlantWizardViewModel()
        {
            // Order of the steps (ViewModels)
            _steps = new List<IWizardStepViewModel>
            {
                _plantNameStepViewModel,
                _wateringStepViewModel,
                _fertilizingStepViewModel,
                _sunlightStepViewModel,
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
            }
        }

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand CancelCommand { get; }

        // Signalize the View, that the Wizard will be closed
        public event EventHandler<bool>? RequestClose;

        private void GoNext()
        {
            if (_currentStepIndex < _steps.Count - 1)
            {
                _currentStepIndex++;
                CurrentStep = _steps[_currentStepIndex];
            }
            else
            {
                // Last step completed? Close the Wizard
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
            // Method currently under construction.
            // This method will take all collected data and create
            // a new Plant-Object that will be added to the ListView
            RequestClose?.Invoke(this, true);
        }

        private void Cancel()
        {
            RequestClose?.Invoke(this, false);
        }

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
