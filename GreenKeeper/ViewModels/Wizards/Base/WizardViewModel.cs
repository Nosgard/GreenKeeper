using GreenKeeper.Commands;
using GreenKeeper.ViewModels.Base;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.Wizards.Base
{
    /// <summary>
    /// The step navigation shared by both wizards: the current step, the three
    /// buttons and the close signal. Which step comes next is up to the subclass.
    /// </summary>
    public abstract class WizardViewModel : ObservableObject, IDialogViewModel
    {
        private IWizardStepViewModel _currentStep = null!;

        protected WizardViewModel()
        {
            NextCommand = new RelayCommand(
                execute: _ => GoNext(),
                canExecute: _ => CurrentStep.CanProceed);

            BackCommand = new RelayCommand(
                execute: _ => GoBack(),
                canExecute: _ => CanGoBack);

            CancelCommand = new RelayCommand(
                execute: _ => Close(false));
        }

        public IWizardStepViewModel CurrentStep
        {
            get => _currentStep;
            protected set
            {
                if (SetProperty(ref _currentStep, value))
                {
                    // The buttons depend on the step, and no UI interaction announces the switch.
                    RelayCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand CancelCommand { get; }

        // Signals the View that the wizard will be closed: true = finished, false = canceled.
        public event EventHandler<bool>? RequestClose;

        protected abstract bool CanGoBack { get; }

        protected abstract void GoNext();

        protected abstract void GoBack();

        protected void Close(bool finished)
        {
            RequestClose?.Invoke(this, finished);
        }
    }
}
