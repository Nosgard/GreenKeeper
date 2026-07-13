using GreenKeeper.Models;
using GreenKeeper.Repositories;
using GreenKeeper.Services;
using GreenKeeper.ViewModels;
using GreenKeeper.Views.Wizards.AddPlantWizard;
using GreenKeeper.Views.Wizards.AddScheduleWizard;
using GreenKeeper.Views.Notes;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GreenKeeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _mainViewModel;

        // One instance to be passed on from everywhere.
        // This DialogService is primarily served for Yes/No-Warnings
        private readonly IDialogService _dialogService = new MessageBoxDialogService();
        public MainWindow()
        {
            InitializeComponent();
            _mainViewModel = new MainViewModel(new PlantRepository(), _dialogService);
            _mainViewModel.AddPlantRequested += MainViewModel_AddPlantRequested;
            _mainViewModel.AddScheduleRequested += MainViewModel_AddScheduleRequested;

            // Subscription to the event that fires "OpenNotesCommand" in the MainViewModel.
            // Opening a new window for the notes (NotesView) does not happen in the ViewModel
            // so that it has no window-references and remains testable
            _mainViewModel.OpenNotesRequested += MainViewModel_OpenNotesRequested;
            this.DataContext = _mainViewModel;
        }

        // The actual reaction on OpenNotesRequested, caused by the OpenNotesCommand in the ViewModel.
        // It opens a new window and shows the notes of the given plant
        private void MainViewModel_OpenNotesRequested(object? sender, Plant plant)
        {
            var notesView = new NotesView(plant, _dialogService)
            {
                Owner = this
            };
            notesView.ShowDialog();
        }

        private void MainViewModel_AddPlantRequested(object? sender, EventArgs e)
        {
            var WizardView = new AddPlantWizardView
            {
                Owner = this
            };

            bool? hasResult = WizardView.ShowDialog();

            if (hasResult == true && WizardView.CreatedPlant != null)
            {
                _mainViewModel.AddPlant(WizardView.CreatedPlant);
            }
        }

        private void MainViewModel_AddScheduleRequested(object? sender, Plant plant)
        {
            var wizardView = new AddScheduleWizardView(plant, _dialogService)
            {
                Owner = this
            };

            bool? result = wizardView.ShowDialog();

            if (result == true)
            {
                _mainViewModel.RefreshCareStatuses();
            }
        }
    }
}