using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.ViewModels.Base;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.RenamePlant
{
    /// <summary>
    /// Backs the rename dialog. PlantListViewModel saves the confirmed name.
    /// </summary>
    public class RenamePlantViewModel : ObservableObject, IDialogViewModel
    {
        private string _newName;

        // Holds the confirmed new name once the user has clicked Save.
        // Stays null if the dialog was canceled.
        public string? ConfirmedName { get; private set; }

        public RenamePlantViewModel(Plant plant)
        {
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
                    OnPropertyChanged(nameof(CharactersRemaining));
                    OnPropertyChanged(nameof(CharactersRemainingText));
                    OnPropertyChanged(nameof(HasValidName));
                }
            }
        }

        public int CharactersRemaining => Plant.MaxNameLength - _newName.Length;

        public string CharactersRemainingText => NameLengthCounter.TextFor(CharactersRemaining);

        public bool HasValidName => Plant.IsValidName(_newName);

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool>? RequestClose;

        private void Save()
        {
            ConfirmedName = Plant.NormalizeName(_newName);

            RequestClose?.Invoke(this, true);
        }
    }
}
