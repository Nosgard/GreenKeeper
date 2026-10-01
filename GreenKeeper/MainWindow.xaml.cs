using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Services;
using GreenKeeper.ViewModels;
using GreenKeeper.Views.CareStatuses.EditOption;
using GreenKeeper.Views.Notes;
using GreenKeeper.Views.RenamePlant;
using GreenKeeper.Views.Wizards.AddPlantWizard;
using GreenKeeper.Views.Wizards.AddScheduleWizard;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GreenKeeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml. Opens the dialogs the ViewModel asks
    /// for through its events and hands their results back to it - the only place
    /// where ViewModel and windows meet.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IDialogService _dialogService;
        private readonly TimeProvider _timeProvider;

        public MainWindow(MainViewModel mainViewModel, IDialogService dialogService, TimeProvider timeProvider)
        {
            _mainViewModel = mainViewModel;
            _dialogService = dialogService;
            _timeProvider = timeProvider;

            InitializeComponent();

            _mainViewModel.AddPlantRequested += MainViewModel_AddPlantRequested;
            _mainViewModel.AddScheduleRequested += MainViewModel_AddScheduleRequested;
            _mainViewModel.EditScheduleRequested += MainViewModel_EditScheduleRequested;
            _mainViewModel.OpenNotesRequested += MainViewModel_OpenNotesRequested;
            _mainViewModel.RenamePlantRequested += MainViewModel_RenamePlantRequested;

            // Stops the periodic status card refresh once this window (and therefore the application)
            // is closed, so the timer doesn't keep firing after the app is meant to shut down.
            Closed += (_, _) => _mainViewModel.StopCareStatusRefreshTimer();

            PreviewMouseDown += Window_PreviewMouseDown;

            // The constructor cannot be async, so loading the plants will be fired
            // via the Loaded event once the window is ready.
            Loaded += MainWindow_Loaded;

            DataContext = _mainViewModel;
        }

        // -- Dialogs Section --
        // Event handlers have to be "async void", so nobody can await them: every database
        // call goes through RunAsync, which reports a failure as an error dialog.

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await RunAsync(() => _mainViewModel.InitializeAsync(), "Failed to load the plant", "Loading Error");
        }

        // Opens the Add Plant wizard. If the user completed it, the built plant is handed off to be persisted.
        private async void MainViewModel_AddPlantRequested(object? sender, EventArgs e)
        {
            var wizardView = new AddPlantWizardView();

            if (!ShowDialog(wizardView) || wizardView.CreatedPlant == null)
            {
                return;
            }

            await RunAsync(() => _mainViewModel.AddPlantAsync(wizardView.CreatedPlant), "Failed to save the plant", "Saving Error");
        }

        // Opens the Add Schedule wizard. If completed, its result (a care schedule or a sunlight requirement) is persisted.
        private async void MainViewModel_AddScheduleRequested(object? sender, Plant plant)
        {
            var wizardView = new AddScheduleWizardView(plant, _dialogService);

            if (!ShowDialog(wizardView) || wizardView.Result == null)
            {
                return;
            }

            await RunAsync(() => wizardView.Result.SaveAsync(_mainViewModel), "Schedule could not be saved", "Saving Error");
        }

        // Opens the edit dialog. If saved, its result is persisted the exact same way as a wizard result -
        // editing a schedule and replacing it via the wizard are, from the database's point of view, the identical operation.
        private async void MainViewModel_EditScheduleRequested(object? sender, (Plant plant, CareType care) e)
        {
            var editView = new EditScheduleView(e.plant, e.care, _timeProvider);

            if (!ShowDialog(editView) || editView.Result == null)
            {
                return;
            }

            await RunAsync(() => editView.Result.SaveAsync(_mainViewModel), "The change could not be saved", "Saving Error");
        }

        // Opens the notes window. It saves through the callback itself, so there is nothing to persist afterwards.
        private void MainViewModel_OpenNotesRequested(object? sender, Plant plant)
        {
            ShowDialog(new NotesView(plant, _dialogService, saveNotesAsync: text => _mainViewModel.UpdatePlantNotesAsync(plant, text)));
        }

        // Opens the rename dialog for the plant that was right-clicked in the sidebar.
        private async void MainViewModel_RenamePlantRequested(object? sender, Plant plant)
        {
            var renameView = new RenamePlantView(plant);

            if (!ShowDialog(renameView) || renameView.ConfirmedName == null)
            {
                return;
            }

            await RunAsync(
                () => _mainViewModel.RenamePlantAsync(plant, renameView.ConfirmedName),
                "The plant could not be renamed",
                "Renaming Error");
        }

        // Shows the window as a modal dialog owned by the main window; true if it was confirmed.
        private bool ShowDialog(Window dialog)
        {
            dialog.Owner = this;
            return dialog.ShowDialog() == true;
        }

        private async Task RunAsync(Func<Task> operation, string failureText, string title)
        {
            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"{failureText}:\n{ex.Message}", title);
            }
        }

        // -- Search Box Focus Section --

        // Clicking anywhere outside the search box takes the keyboard focus away from it.
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
