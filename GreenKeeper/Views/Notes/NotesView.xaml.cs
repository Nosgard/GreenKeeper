using GreenKeeper.Models;
using GreenKeeper.Services;
using GreenKeeper.ViewModels.Notes;
using System.ComponentModel;

namespace GreenKeeper.Views.Notes
{
    /// <summary>
    /// Interaction logic for NotesView.xaml.
    /// </summary>
    public partial class NotesView : DialogWindow
    {
        private readonly NotesViewModel _notesViewModel;

        // Guards against re-entrancy: false as long as the close hasn't been confirmed yet.
        // Once set to true, the Closing handler lets the window close without intercepting it again -
        // otherwise the DialogResult assignment in CloseWithResult would immediately re-trigger
        // Closing and loop back into the confirmation flow a second time.
        private bool _closeConfirmed;

        public NotesView(Plant plant, IDialogService dialogService, Func<string, Task> saveNotesAsync)
        {
            InitializeComponent();
            _notesViewModel = new NotesViewModel(plant, dialogService, saveNotesAsync);
            Attach(_notesViewModel);
            Closing += NotesView_Closing;
        }

        protected override void CloseWithResult(bool dialogResult)
        {
            _closeConfirmed = true;
            base.CloseWithResult(dialogResult);
        }

        /// <summary>
        /// Intercepts ANY way the window could close natively (X button, Alt+F4, taskbar close) -
        /// these all bypass CancelCommand entirely and would otherwise close the window without
        /// ever checking for unsaved changes.
        ///
        /// Reuses the ViewModel's existing CancelCommand instead of duplicating the confirm/save logic here:
        /// clicking X behaves identically to clicking the Cancel button, using the exact same
        /// warning and save flow.
        /// </summary>
        private void NotesView_Closing(object? sender, CancelEventArgs e)
        {
            if (_closeConfirmed)
            {
                return;
            }

            e.Cancel = true;

            Dispatcher.BeginInvoke(() => _notesViewModel.CancelCommand.Execute(null));
        }
    }
}
