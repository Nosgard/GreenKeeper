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
using GreenKeeper.Models.Enums;
using GreenKeeper.Views.CareStatuses.EditOption;

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

        // Concrete, implementation of ITimerService. Created and owned here and then injected
        // into the MainViewModel via its constructor
        private readonly ITimerService _timerService = new DispatcherTimerService();
        public MainWindow()
        {
            InitializeComponent();
            _mainViewModel = new MainViewModel(new PlantRepository(), _dialogService, _timerService);
            _mainViewModel.AddPlantRequested += MainViewModel_AddPlantRequested;
            _mainViewModel.AddScheduleRequested += MainViewModel_AddScheduleRequested;

            // Subscription to the event that fires "OpenNotesCommand" in the MainViewModel.
            // Opening a new window for the notes (NotesView) does not happen in the ViewModel
            // so that it has no window-references and remains testable
            _mainViewModel.OpenNotesRequested += MainViewModel_OpenNotesRequested;

            _mainViewModel.EditScheduleRequested += MainViewModel_EditScheduleRequested;

            // Stops the periodic Status-Card refresh once this window (and therefore the application)
            // is closed, so the timer doesn't keep firing after the app is meant to shut down
            Closed += (_, _) => _mainViewModel.StopCareStatusRefreshTimer();

            PreviewMouseDown += Window_PreviewMouseDown;
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

        private void MainViewModel_EditScheduleRequested(object? sender, (Plant plant, CareType care) e)
        {
            var editView = new EditScheduleView(e.plant, e.care)
            {
                Owner = this
            };

            bool? result = editView.ShowDialog();

            if (result == true)
            {
                _mainViewModel.RefreshCareStatuses();
            }
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!SearchTextBox.IsFocused)
            {
                return;
            }

            if (e.OriginalSource is DependencyObject clickedElement && IsDescendantOf(clickedElement, SearchTextBox))
            {
                return;
            }

            Keyboard.ClearFocus();
        }

        private static bool IsDescendantOf(DependencyObject child, DependencyObject parent)
        {
            DependencyObject? current = child;

            while (current != null)
            {
                if (current == parent)
                {
                    return true;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }
    }
}