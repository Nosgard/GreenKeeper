using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Services;
using GreenKeeper.ViewModels.Base;

namespace GreenKeeper.ViewModels.Notes
{
    public class NotesViewModel : ObservableObject, IDialogViewModel
    {
        private readonly Plant _plant;

        private readonly IDialogService _dialogService;

        /// <summary>
        /// Saves through DashboardViewModel.UpdatePlantNotesAsync, so this ViewModel knows
        /// neither the repository nor the database. It returns a Task, so a failed save can keep
        /// the dialog open.
        /// </summary>
        private readonly Func<string, Task> _saveNotesAsync;

        // The notes as last saved; IsDirty compares the edited text with them.
        private string _originalNotes;

        // The text being edited, kept apart from plant.Notes so Cancel leaves the plant as it was.
        private string _editableNotes;

        public NotesViewModel(Plant plant, IDialogService dialogService, Func<string, Task> saveNotesAsync)
        {
            _plant = plant;
            _dialogService = dialogService;
            _saveNotesAsync = saveNotesAsync;
            _originalNotes = plant.Notes ?? string.Empty;
            _editableNotes = _originalNotes;

            SaveCommand = new AsyncRelayCommand(
                execute: _ => SaveAsync(),
                canExecute: _ => IsDirty);

            CancelCommand = new AsyncRelayCommand(
                execute: _ => CancelAsync());
        }

        public string PlantName => _plant.Name;

        /// <summary>
        /// The text in the notes field. It starts as the plant's notes, empty if it has none.
        /// </summary>
        public string EditableNotes
        {
            get => _editableNotes;
            set
            {
                if (SetProperty(ref _editableNotes, value))
                {
                    OnPropertyChanged(nameof(IsDirty));
                }
            }
        }

        // Save works only while this is true.
        public bool IsDirty => _editableNotes != _originalNotes;

        public AsyncRelayCommand SaveCommand { get; }
        public AsyncRelayCommand CancelCommand { get; }

        /// <summary>
        /// Asks the window to close (see IDialogViewModel). Only CancelAsync raises it, after any
        /// question about unsaved changes.
        /// </summary>
        public event EventHandler<bool>? RequestClose;

        /// <summary>
        /// Raised when the Save button has saved the notes. The window stays open then,
        /// so NotesView uses it to keep the focus in the notes and to tell a screen reader.
        /// </summary>
        public event EventHandler? Saved;

        /// <summary>
        /// Returns whether the notes were saved. After a failure both commands keep the dialog
        /// open, with the entered text still in the field.
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

        private async Task SaveAsync()
        {
            if (await TrySaveAsync())
            {
                Saved?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called for the Cancel button and for every other way of closing the window (see
        /// NotesView).
        /// </summary>
        private async Task CancelAsync()
        {
            if (!IsDirty)
            {
                RequestClose?.Invoke(this, false);
                return;
            }

            // "No" would discard the changes, so here "Yes" is the safe answer.
            bool shouldSave = _dialogService.Confirm(
                "There are unsaved changes. Do you want to save?",
                "Unsaved Changes",
                safeAnswer: Answer.Yes);

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
