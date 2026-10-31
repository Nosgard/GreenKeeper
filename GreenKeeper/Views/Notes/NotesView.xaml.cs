using GreenKeeper.Models;
using GreenKeeper.Services;
using GreenKeeper.ViewModels.Notes;
using System.ComponentModel;

namespace GreenKeeper.Views.Notes
{
    public partial class NotesView : DialogWindow
    {
        private readonly NotesViewModel _notesViewModel;

        // Set once the close is confirmed, so the Closing handler lets the window close.
        // Without it, setting DialogResult in CloseWithResult would raise Closing again and
        // run the confirmation a second time.
        private bool _closeConfirmed;

        public NotesView(Plant plant, IDialogService dialogService, Func<string, Task> saveNotesAsync)
        {
            InitializeComponent();
            _notesViewModel = new NotesViewModel(plant, dialogService, saveNotesAsync);
            Attach(_notesViewModel);
            Closing += NotesView_Closing;
            _notesViewModel.Saved += NotesViewModel_Saved;
        }

        // Save disables itself once the notes are saved, and the focus on it would fall
        // back to the window. The focus goes back to the notes instead, and a screen
        // reader hears the result.
        private void NotesViewModel_Saved(object? sender, EventArgs e)
        {
            NotesBox.Focus();
            Announcer.AnnounceAfterFocusSettles(NotesBox, "Notes saved");
        }

        protected override void CloseWithResult(bool dialogResult)
        {
            _closeConfirmed = true;
            base.CloseWithResult(dialogResult);
        }

        /// <summary>
        /// Sends every other way of closing (the X button, Alt+F4, the taskbar) through
        /// CancelCommand, so it asks about unsaved changes like the Cancel button.
        /// </summary>
        private void NotesView_Closing(object? sender, CancelEventArgs e)
        {
            if (_closeConfirmed)
            {
                return;
            }

            e.Cancel = true;

            // Runs after this handler, once the close is canceled. CancelCommand then asks about
            // unsaved changes, if there are any, and closes the window itself unless a save fails.
            Dispatcher.BeginInvoke(() => _notesViewModel.CancelCommand.Execute(null));
        }
    }
}
