using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Services;
using GreenKeeper.ViewModels.Base;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.Notes
{
    public class NotesViewModel : ObservableObject, IDialogViewModel
    {
        private readonly Plant _plant;

        // The dependency is passed in from outside (Constructor Injection),
        // instead of an explicit implementation.
        private readonly IDialogService _dialogService;

        /// <summary>
        /// Handles the actual persistence via MainViewModel.UpdatePlantNotesAsync -
        /// NotesViewModel itself still knows neither the repository nor the database.
        /// Func is used instead of Action because saving needs to await the result
        /// to keep the dialog open on failure.
        /// </summary>
        private readonly Func<string, Task> _saveNotesAsync;

        // Value when opening the NotesView. Reference if changes to the notes are made.
        // Used to compare via IsDirty (for more info scroll down) with the edited notes.
        // After saving changes, IsDirty "forgets" the saved changes and treats the
        // changed notes as the original.
        private string _originalNotes;

        // Takes the original notes so that changes can be recognized.
        // Separated from the notes in the plant object so that the
        // Cancel button can discard changes without changing the
        // plant object in the meantime.
        private string _editableNotes;

        /// <summary>
        /// SaveCommand: saves via the callback; only active while IsDirty is true,
        /// i.e. while the notes differ from the original.
        ///
        /// CancelCommand: always executable, even if there are no changes - then it
        /// simply closes the View without any warning.
        /// </summary>
        public NotesViewModel(Plant plant, IDialogService dialogService, Func<string, Task> saveNotesAsync)
        {
            _plant = plant;
            _dialogService = dialogService;
            _saveNotesAsync = saveNotesAsync;
            _originalNotes = plant.Notes ?? string.Empty;
            _editableNotes = _originalNotes;

            SaveCommand = new AsyncRelayCommand(
                execute: _ => TrySaveAsync(),
                canExecute: _ => IsDirty);

            CancelCommand = new AsyncRelayCommand(
                execute: _ => CancelAsync());
        }

        public string PlantName => _plant.Name;

        /// <summary>
        /// Extract the Notes property from the plant model to present it in the related view
        /// and make it editable. Changes to the original notes will be recognized by OnPropertyChanged.
        /// </summary>
        public string EditableNotes
        {
            get => _editableNotes;
            set
            {
                if (SetProperty(ref _editableNotes, value))
                {
                    // IsDirty depends on _editableNotes.
                    // Manually signal that IsDirty could change, and with that the Save button as well.
                    OnPropertyChanged(nameof(IsDirty));
                }
            }
        }

        // True once the current notes differ from the original.
        // Controls CanExecute of SaveCommand (Save button enabled/disabled).
        public bool IsDirty => _editableNotes != _originalNotes;

        // Commands to be bound in NotesView.xaml
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Signals the NotesView that the window is about to be closed.
        /// This is necessary to warn the user about unsaved changes.
        /// NotesView.xaml.cs subscribes to it.
        /// Only fired by CancelAsync: true = saved and close, false = discard and close.
        /// </summary>
        public event EventHandler<bool>? RequestClose;

        /// <summary>
        /// Runs the actual save via the callback, updates _originalNotes on success (resets IsDirty)
        /// and returns whether it worked. Used by both commands so both share the same
        /// error behavior - on failure the entered text stays untouched instead of being lost.
        /// </summary>
        private async Task<bool> TrySaveAsync()
        {
            try
            {
                await _saveNotesAsync(EditableNotes);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"The notes could not be saved:\n{ex.Message}",
                    "Saving Error");
                return false;
            }

            _originalNotes = EditableNotes;
            OnPropertyChanged(nameof(IsDirty));
            return true;
        }

        /// <summary>
        /// Called when the Cancel button is clicked (or the window is closed otherwise).
        /// Without changes the window simply closes. With unsaved changes the user is
        /// asked: "Yes" saves first and closes only if that worked, "No" discards.
        /// </summary>
        private async Task CancelAsync()
        {
            if (!IsDirty)
            {
                RequestClose?.Invoke(this, false);
                return;
            }

            bool shouldSave = _dialogService.Confirm(
                "There are unsaved changes. Do you want to save?",
                "Unsaved Changes");

            if (!shouldSave)
            {
                RequestClose?.Invoke(this, false);
                return;
            }

            if (await TrySaveAsync())
            {
                RequestClose?.Invoke(this, true);
            }
        }
    }
}
