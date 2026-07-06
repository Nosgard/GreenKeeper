using GreenKeeper.Commands;
using GreenKeeper.ViewModels.AddPlantWizard.Steps;
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

        public AddPlantWizardViewModel()
        {
            CurrentStep = _plantNameStepViewModel;

            NextCommand = new RelayCommand(
                execute: _ => GoNext(),
                canExecute: _ => CurrentStep.CanProceed);

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
            }
        }

        public ICommand NextCommand { get; }
        public ICommand CancelCommand { get; }

        // Signalize the View, that the Wizard will be closed
        public event EventHandler<bool>? RequestClose;

        private void GoNext()
        {
            // Placeholder for the next step
        }

        private void Cancel()
        {
            RequestClose?.Invoke(this, false);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
