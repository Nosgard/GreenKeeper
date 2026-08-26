using GreenKeeper.Models;
using GreenKeeper.ViewModels.RenamePlant;
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

namespace GreenKeeper.Views.RenamePlant
{
    /// <summary>
    /// Interaction logic for RenamePlantView.xaml
    /// </summary>
    public partial class RenamePlantView : Window
    {
        private readonly RenamePlantViewModel _renamePlantViewModel;
        public RenamePlantView(Plant plant)
        {
            InitializeComponent();
            _renamePlantViewModel = new RenamePlantViewModel(plant);
            _renamePlantViewModel.RequestClose += ViewModel_RequestClose;
            this.DataContext = _renamePlantViewModel;

            // Puts the cursor straight into the input field and selects the existing name,
            // so the user can just start typing to replace it
            Loaded += (_, _) =>
            {
                NameTextBox.Focus();
                NameTextBox.SelectAll();
            };
        }

        // Exposes the name to the caller (MainWindow), analogous to
        // EditScheduleView.EditCareSchedule - the actual persistence happens in the MainViewModel
        public string? ConfirmedName => _renamePlantViewModel.ConfirmedName;

        // Unsubscribing before closing prevents double subscriptions of the
        // Event-Handler when opening the same window (view) multiple times
        private void ViewModel_RequestClose(object? sender, bool dialogResult)
        {
            _renamePlantViewModel.RequestClose -= ViewModel_RequestClose;
            DialogResult = dialogResult;
        }
    }
}
