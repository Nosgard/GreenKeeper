using GreenKeeper.Models;
using GreenKeeper.Services;
using GreenKeeper.ViewModels.Notes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GreenKeeper.Views.Notes
{
    /// <summary>
    /// Interaction logic for NotesView.xaml
    /// </summary>
    public partial class NotesView : Window
    {
        private readonly NotesViewModel _notesViewModel;

        // Guards against re-entrancy: false as long as the close hasn't been confirmed yet.
        // Once set to true, the Closing handler lets the window close without intercepting it again -
        // otherwise the DialogResult assignment in ViewModel_RequestClose would immediately re-trigger
        // Closing and loop back into the confirmation flow a second time
        private bool _closeConfirmed;

        public NotesView(Plant plant, IDialogService dialogService, Func<string, Task> saveNotesAsync)
        {
            InitializeComponent();
            _notesViewModel = new NotesViewModel(plant, dialogService, saveNotesAsync);
            _notesViewModel.RequestClose += NotesViewModel_RequestClose;
            this.DataContext = _notesViewModel;
            Closing += NotesView_Closing;
        }

        // Unsubscribing before closing prevents double subscriptions of the Event-Handler
        // when opening the same window (view) multiple times
        private void NotesViewModel_RequestClose(object? sender, bool? dialogResult)
        {
            _notesViewModel.RequestClose -= NotesViewModel_RequestClose;
            _closeConfirmed = true;
            DialogResult = dialogResult;
        }

        /// <summary>
        /// Intercepts ANY way the window could close natively (X button, Alt+F4, taskbar close) -
        /// these all bypass CancelCommand entirely and would otherwise close the window without
        /// ever checking for unsaved changes.
        /// 
        /// Reuses the ViewModel's existing CancelCommand instead of duplicating the confirm/save logic here:
        /// clicking X behaves identically to clicking the Cancel-Button, using the exact same
        /// warning and save flow
        /// </summary>
        private void NotesView_Closing(object? sender, CancelEventArgs e)
        {
            if (_closeConfirmed)
            {
                return;
            }

            e.Cancel = true;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                _notesViewModel.CancelCommand.Execute(null);
            }));
        }
    }
}
