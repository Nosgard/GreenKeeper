using GreenKeeper.Commands;
using GreenKeeper.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.RenamePlant
{
    /// <summary>
    /// Backs the Rename-Dialog. Like the Wizard- and Edit-ViewModels, it only
    /// prepares the new value and signals the result - the actual persistence
    /// happens in the MainViewModel via the repository, so this class knows
    /// nothing about databases or windows
    /// </summary>
    public class RenamePlantViewModel : INotifyPropertyChanged
    {

        // Same limit as the Add-Plant-Wizard's name step - it would be
        // inconsistent to allow longer names when renaming
        public const int MaxNameLength = 50;

        // Holds the confirmed new name once the user clicked Save.
        // Stays null if the dialog was cancelled
        public string? ConfirmedName { get; private set; }

        public RenamePlantViewModel(Plant plant)
        {
            // Pre-fill with the current name, so the user sees what they're
            // changing and can make small corrections without retyping
            _newName = plant.Name;

            SaveCommand = new RelayCommand(
                execute: _ => Save(),
                canExecute: _ => HasValidName);

            CancelCommand = new RelayCommand(
                execute: _ => RequestClose?.Invoke(this, false));
        }

        private string _newName;
        public string NewName
        {
            get => _newName;
            set
            {
                if (_newName == value)
                {
                    return;
                }

                _newName = value;
                OnPropertyChanged(nameof(NewName));

                // Both depend on _newName and must be re-evaluated whenever
                // the text changes
                OnPropertyChanged(nameof(CharactersRemaining));
                OnPropertyChanged(nameof(HasValidName));
            }
        }

        // Shown below the input field
        public int CharactersRemaining => MaxNameLength - (_newName?.Length ?? 0);

        // A name consisting only of spaces would look empty in the sidebar
        // but still pass a simple null/empty check
        public bool HasValidName => !string.IsNullOrWhiteSpace(_newName);

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool>? RequestClose;

        private void Save()
        {
            // Trim so leading/trailing spaces don't end up in the database -
            // they'd be invisible in the UI but affect sorting and searching
            ConfirmedName = _newName.Trim();

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
