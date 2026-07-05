using GreenKeeper.Models;
using GreenKeeper.ViewModels.Notes;
using System;
using System.Collections.Generic;
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
        public NotesView(Plant plant)
        {
            InitializeComponent();
            _notesViewModel = new NotesViewModel(plant);
            _notesViewModel.RequestClose += NotesViewModel_RequestClose;
            this.DataContext = _notesViewModel;
        }

        // Unsubscribing before closing prevents double subscriptions of the Event-Handler
        // when opening the same window (view) multiple times
        private void NotesViewModel_RequestClose(object? sender, bool? dialogResult)
        {
            _notesViewModel.RequestClose -= NotesViewModel_RequestClose;
            DialogResult = dialogResult;
        }
    }
}
