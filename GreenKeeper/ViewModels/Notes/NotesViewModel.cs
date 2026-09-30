using GreenKeeper.Commands;
using GreenKeeper.Models;
using GreenKeeper.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GreenKeeper.ViewModels.Notes
{
    public class NotesViewModel : INotifyPropertyChanged
    {
        private readonly Plant _plant;

        // The dependency is passed in from outside (Constructor Injection),
        // instead of an explicit implementation.
        private readonly IDialogService _dialogService;

        /// <summary>
        /// Handles the actual persistence via MainViewModel.UpdatePlantNotesAsync -
        /// NotesViewModel itself still knows neither the repository nor the database.
        /// Func is used instead of Action because Save() needs to await the result
        /// to keep the dialog open on failure.
        /// </summary>
        private readonly Func<string, Task> _saveNotesAsync;

        // Value when opening the NotesView. Reference if changes to the notes are made.
        // Used to compare via IsDirty (for more info scroll down) with the edited notes.
        // After saving changes, IsDirty "forgets" the saved changes and treats the
        // changed notes as the original
        private string _originalNotes;

        // Takes the original notes so that changes can be recognized.
        // Separated from the notes in the plant object so that the
        // Cancel button can discard changes without changing the
        // plant object in the meantime
        private string _editableNotes;

        /// <summary>
        /// Important notes for SaveCommand and CancelCommand:
        /// SaveCommand in detail:
        /// 
        /// execute: Controls what happens when you click the Save button -> fire the Save() method.
        /// 
        /// canExecute: The Save button is only active when IsDirty is true.
        /// What does that mean? The notes are considered dirty if the notes differ from
        /// the original.
        /// 
        /// Important info for IsDirty!
        /// The assignment of IsDirty must be in the constructor; otherwise WPF ignores
        /// the click on a command set to 'null'.
        /// 
        /// CancelCommand in detail:
        /// 
        /// execute: Fires the Cancel() method
        /// Why no canExecute? Because Cancel must always be executable, even if there are
        /// no changes. This simply leads to closing the View without any warning.
        /// 
        /// </summary>
        /// <param name="plant"></param>
        public NotesViewModel(Plant plant, IDialogService dialogService, Func<string, Task> saveNotesAsync)
        {
            _plant = plant;
            _dialogService = dialogService;
            _saveNotesAsync = saveNotesAsync;
            _originalNotes = plant.Notes ?? string.Empty;
            _editableNotes = _originalNotes;


            SaveCommand = new RelayCommand(
                execute: _ => Save(),
                canExecute: _ => IsDirty);

            CancelCommand = new RelayCommand(
                execute: _ => Cancel());
        }

        public string PlantName => _plant.Name;

        /// <summary>
        /// Extract the Notes property from the plant model to present it in the related view
        /// and make it editable. Changes to the original notes will be recognized by OnPropertyChanged.
        /// The setter always fires on every button click.
        /// </summary>
        public string EditableNotes
        {
            get => _editableNotes;
            set
            {
                if (_editableNotes == value)
                {
                    return;
                }
                _editableNotes = value;
                OnPropertyChanged(nameof(EditableNotes));

                // IsDirty depends on _editableNotes.
                // Manually signal that IsDirty could change, and with that the Save button as well
                OnPropertyChanged(nameof(IsDirty));
            }
        }

        // True once the current notes differ from the original.
        // Controls CanExecute of SaveCommand (Save button enabled/disabled)
        public bool IsDirty => _editableNotes != _originalNotes;

        // Commands to be bound in NotesView.xaml
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Signals the NotesView that the window is about to be closed.
        /// This is necessary to warn the user about unsaved changes.
        /// NotesView.xaml.cs subscribes to it.
        /// Only fired by the Cancel() method.
        /// 
        /// bool? represents Window.DialogResult (true = save and close, false = discard and close).
        /// </summary>
        public event EventHandler<bool?>? RequestClose;


        // Sets the edited text in the plant object by calling TrySaveAsync.
        // Error handling happens inside TrySaveAsync
        private async void Save()
        {
            await TrySaveAsync();
        }

        /// <summary>
        /// Runs the actual save via the callback, updates _originalNotes on success (resets IsDirty)
        /// and returns whether it worked. Used by both Save() and Cancel() so both share the same
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
        /// Called when the Cancel button is clicked.
        /// A warning shows up if IsDirty recognizes changes to the notes.
        /// In case the user tries to cancel while there are unsaved changes,
        /// TrySaveAsync() will be awaited before actually closing.
        /// This is for error handling.
        /// </summary>
        private async void Cancel()
        {
            if (IsDirty)
            {
                // A cleaner alternative to MessageBox.Show() through
                // the injected abstraction. The ViewModel only knows that there
                // is a yes or a no.
                bool shouldSave = _dialogService.Confirm(
                    "There are unsaved changes. Do you want to save?",
                    "Unsaved Changes");

                // Act like Save + Close the View together
                if (shouldSave)
                {
                    bool saved = await TrySaveAsync();
                    if (saved)
                    {
                        RequestClose?.Invoke(this, true);
                    }
                    return;
                }

                // User chose "No"? Discard all changes and close
                RequestClose?.Invoke(this, false);
            }

            // No changes? Simply close
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
