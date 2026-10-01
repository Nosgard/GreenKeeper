using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.ViewModels.Base;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.RenamePlant
{
    /// <summary>
    /// Backs the rename dialog. Like the wizard and edit ViewModels, it only
    /// prepares the new value and signals the result - the actual persistence
    /// happens in the MainViewModel via the repository, so this class knows
    /// nothing about databases or windows.
    /// </summary>
    public class RenamePlantViewModel : ObservableObject, IDialogViewModel
    {
        private string _newName;

        // Holds the confirmed new name once the user has clicked Save.
        // Stays null if the dialog was canceled.
        public string? ConfirmedName { get; private set; }

        public RenamePlantViewModel(Plant plant)
        {
            // Pre-fill with the current name, so the user sees what they're
            // changing and can make small corrections without retyping.
            _newName = plant.Name;

            SaveCommand = new RelayCommand(
                execute: _ => Save(),
                canExecute: _ => HasValidName);

            CancelCommand = new RelayCommand(
                execute: _ => RequestClose?.Invoke(this, false));
        }

        public string NewName
        {
            get => _newName;
            set
            {
                if (SetProperty(ref _newName, value))
                {
                    // Both depend on _newName and must be re-evaluated whenever
                    // the text changes.
                    OnPropertyChanged(nameof(CharactersRemaining));
                    OnPropertyChanged(nameof(HasValidName));
                }
            }
        }

        // Shown below the input field.
        public int CharactersRemaining => Plant.MaxNameLength - _newName.Length;

        // Same rule as the Add Plant wizard's name step - it would be
        // inconsistent to accept other names when renaming.
        public bool HasValidName => Plant.IsValidName(_newName);

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool>? RequestClose;

        private void Save()
        {
            // Trim so leading/trailing spaces don't end up in the database -
            // they'd be invisible in the UI but affect sorting and searching.
            ConfirmedName = _newName.Trim();

            RequestClose?.Invoke(this, true);
        }
    }
}
