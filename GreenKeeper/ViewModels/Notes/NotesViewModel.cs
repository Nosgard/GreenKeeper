using GreenKeeper.Commands;
using GreenKeeper.Models;
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

        // Value when opening the NotesView. Reference if changes on the notes are made.
        // Used to compare via IsDirty (for more info scroll down) with the edited notes.
        // After saving changes, IsDirty "forgets" the saved changes and treats the
        // the changed notes as the original
        private string _originalNotes;

        // Takes the original notes so that changes can be recognized.
        // Separated from the notes in the Plant-Object so that the
        // Cancel-Button can discard changes without changing the
        // Plant-Object in the meantime
        private string _editableNotes;

        /// <summary>
        /// Important notes for SaveCommand and CancelCommand:
        /// SaveCommand in detail:
        /// 
        /// execute: Controls what happens when you click the Save-Button -> Fire Save()-Method
        /// 
        /// canExecute: The Save-Button is only active, when IsDirty is true.
        /// What does that mean? The notes are considered dirty if the notes are deviant from
        /// the original.
        /// 
        /// Important info for IsDirty!
        /// The allocation of IsDirty must be in the constructor, otherwise WPF ignores
        /// the click on a Command set to 'null'.
        /// 
        /// CancelCommand in detail:
        /// 
        /// execute: Fires the Cancel()-Method
        /// Why no canExecute? Because Cancel must always be executable, even if there are
        /// no changes. This simply leads to closing the View without any warning
        /// 
        /// </summary>
        /// <param name="plant"></param>
        public NotesViewModel(Plant plant)
        {
            _plant = plant;
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
        /// Extract the Notes-property from the Plant-Model to present it in the related view
        /// and make it editable. Changes on the original notes will be recognized by OnPropertyChanged.
        /// The Setter always fires on every button click.
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
                // Manually tell, that IsDirty could change and with that the Save-Button as well
                OnPropertyChanged(nameof(IsDirty));
            }
        }

        // True, once the current notes are deviant from the original.
        // Controls CanExecute from SaveCommand (Save-Button enabled/disabled)
        public bool IsDirty => _editableNotes != _originalNotes;

        // Commands to be bound in NotesView.xaml
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Signalizes the NotesView, that the window is about to be closed.
        /// This is necessary to warn the user from unsaved changes.
        /// NotesView.xaml.cs subscribes to it.
        /// Only fired by the Cancel()-Method.
        /// 
        /// bool? depicts Window.DialogResult (true = save and close, false = discard and close)
        /// </summary>
        public event EventHandler<bool?>? RequestClose;


        // Sets the edited text in the Plant-Object.
        // After that, _originalNotes are set to a new original. IsDirty treats the notes as the original
        private void Save()
        {
            _plant.Notes = EditableNotes;
            _originalNotes = EditableNotes;
            OnPropertyChanged(nameof(IsDirty));
        }

        // Call, when the Cancel-Button is clicked.
        // A warning shows up, if IsDirty recognized changes on the notes
        private void Cancel()
        {
            if (IsDirty)
            {
                var result = MessageBox.Show(
                    "There are unsaved changes. Do you want to save?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                // Act like Save + Close the View together
                if (result == MessageBoxResult.Yes)
                {
                    Save();
                    RequestClose?.Invoke(this, true);
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
