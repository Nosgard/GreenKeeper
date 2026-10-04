using GreenKeeper.Layout;
using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Services;
using GreenKeeper.ViewModels;
using GreenKeeper.Views;
using GreenKeeper.Views.CareStatuses.EditOption;
using GreenKeeper.Views.Notes;
using GreenKeeper.Views.RenamePlant;
using GreenKeeper.Views.Wizards.AddPlantWizard;
using GreenKeeper.Views.Wizards.AddScheduleWizard;
using System.Windows;
using System.Windows.Controls;
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
        // How far the focus ring reaches beyond a control: 5 (Margin="-5" of FocusRing in
        // FocusStyles.xaml) and 3 of air.
        private const double FocusRingReach = 8;

        private readonly MainViewModel _mainViewModel;
        private readonly IDialogService _dialogService;
        private readonly TimeProvider _timeProvider;
        private readonly ISettingsService _settingsService;

        public MainWindow(
            MainViewModel mainViewModel,
            IDialogService dialogService,
            TimeProvider timeProvider,
            ISettingsService settingsService)
        {
            _mainViewModel = mainViewModel;
            _dialogService = dialogService;
            _timeProvider = timeProvider;
            _settingsService = settingsService;

            // In this order: the design size is read from Width and Height of the XAML,
            // which RestorePlacement then overwrites.
            InitializeComponent();
            InitializeResponsiveSize();
            RestorePlacement();

            FocusKeeper.Attach(this, PlantList, CareStatusList);
            _mainViewModel.Announced += MainViewModel_Announced;

            _mainViewModel.AddPlantRequested += MainViewModel_AddPlantRequested;
            _mainViewModel.AddScheduleRequested += MainViewModel_AddScheduleRequested;
            _mainViewModel.EditScheduleRequested += MainViewModel_EditScheduleRequested;
            _mainViewModel.OpenNotesRequested += MainViewModel_OpenNotesRequested;
            _mainViewModel.RenamePlantRequested += MainViewModel_RenamePlantRequested;

            // Stops the periodic status card refresh once this window (and therefore the application)
            // is closed, so the timer doesn't keep firing after the app is meant to shut down.
            Closed += (_, _) => _mainViewModel.StopCareStatusRefreshTimer();

            Closing += (_, _) => SavePlacement();

            PreviewMouseDown += Window_PreviewMouseDown;

            // The constructor cannot be async, so loading the plants will be fired
            // via the Loaded event once the window is ready.
            Loaded += MainWindow_Loaded;

            DataContext = _mainViewModel;
        }

        // -- Responsive Size Section --

        // The size in the XAML is the design size. The window opens at it, or at the size
        // that suits the screen, and draws its content at the scale of its current size.
        private void InitializeResponsiveSize()
        {
            var designSize = new Size(Width, Height);
            var referenceScreen = (Size)FindResource("ReferenceScreenSize");
            var scale = (FluidScale)FindResource("WindowScale");

            var startSize = StartSize.For(designSize, referenceScreen, SystemParameters.WorkArea.Size);
            Width = startSize.Width;
            Height = startSize.Height;

            // On a screen smaller than the minimum size, the window still has to fit.
            MinWidth = Math.Min(MinWidth, startSize.Width);
            MinHeight = Math.Min(MinHeight, startSize.Height);

            SizeChanged += (_, e) => UiScale.SetFactor(this, scale.FactorFor(designSize, e.NewSize));
        }

        // -- Window Placement Section --

        // The window opens where and how it was closed. Without a stored placement, as on
        // the first start, it opens at its start size in the middle of the screen.
        private void RestorePlacement()
        {
            var stored = _settingsService.Load().WindowPlacement;

            if (stored == null)
            {
                return;
            }

            // All screens together, because one of them may be gone since the window was closed.
            var screens = new Rect(
                SystemParameters.VirtualScreenLeft,
                SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth,
                SystemParameters.VirtualScreenHeight);
            var bounds = RestoredBounds.For(stored, screens);

            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = bounds.Left;
            Top = bounds.Top;
            Width = bounds.Width;
            Height = bounds.Height;

            if (stored.IsMaximized)
            {
                // Only once the window exists at its place. Maximized earlier, it would open on the
                // main screen instead of the one it was closed on.
                SourceInitialized += (_, _) => WindowState = WindowState.Maximized;
            }
        }

        // Read-modify-write like the theme toggle, so the other settings stay as they are.
        private void SavePlacement()
        {
            var settings = _settingsService.Load();

            // RestoreBounds is the window in its normal state, also while it is maximized.
            settings.WindowPlacement = new WindowPlacement
            {
                Left = RestoreBounds.Left,
                Top = RestoreBounds.Top,
                Width = RestoreBounds.Width,
                Height = RestoreBounds.Height,
                IsMaximized = WindowState == WindowState.Maximized
            };

            _settingsService.Save(settings);
        }

        // -- Screen Reader Section --

        // Said once the focus is back in place (see FocusKeeper).
        private void MainViewModel_Announced(object? sender, string text)
        {
            Announcer.AnnounceAfterFocusSettles(this, text);
        }

        // -- Dashboard Section --

        // The cards of another plant start at their top. A refresh of the plant in view,
        // after Complete for instance, leaves the cards where they are.
        private void PlantList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                StatusScrollViewer.ScrollToTop();
            }
        }

        // A card button the keyboard scrolls into view comes in only up to its own edge,
        // and the scroll area would cut its focus ring off. So the ring comes along.
        private void StatusPanel_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            // Requests that already name a rectangle, including the one raised below, and
            // those of the panel itself go on to the scroll area unchanged.
            if (!e.TargetRect.IsEmpty || e.TargetObject is not FrameworkElement target || ReferenceEquals(target, sender))
            {
                return;
            }

            e.Handled = true;
            target.BringIntoView(new Rect(
                -FocusRingReach,
                -FocusRingReach,
                target.ActualWidth + (2 * FocusRingReach),
                target.ActualHeight + (2 * FocusRingReach)));
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

            if (!wizardView.ShowDialog(this) || wizardView.CreatedPlant == null)
            {
                return;
            }

            await RunAsync(() => _mainViewModel.AddPlantAsync(wizardView.CreatedPlant), "Failed to save the plant", "Saving Error");
        }

        // Opens the Add Schedule wizard. If completed, its result (a care schedule or a sunlight requirement) is persisted.
        private async void MainViewModel_AddScheduleRequested(object? sender, Plant plant)
        {
            var wizardView = new AddScheduleWizardView(plant, _dialogService);

            if (!wizardView.ShowDialog(this) || wizardView.Result == null)
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

            if (!editView.ShowDialog(this) || editView.Result == null)
            {
                return;
            }

            await RunAsync(() => editView.Result.SaveAsync(_mainViewModel), "The change could not be saved", "Saving Error");
        }

        // Opens the notes window. It saves through the callback itself, so there is nothing to persist afterwards.
        private void MainViewModel_OpenNotesRequested(object? sender, Plant plant)
        {
            var notesView = new NotesView(plant, _dialogService, saveNotesAsync: text => _mainViewModel.UpdatePlantNotesAsync(plant, text));

            notesView.ShowDialog(this);
        }

        // Opens the rename dialog for the plant that was right-clicked in the sidebar.
        private async void MainViewModel_RenamePlantRequested(object? sender, Plant plant)
        {
            var renameView = new RenamePlantView(plant);

            if (!renameView.ShowDialog(this) || renameView.ConfirmedName == null)
            {
                return;
            }

            await RunAsync(
                () => _mainViewModel.RenamePlantAsync(plant, renameView.ConfirmedName),
                "The plant could not be renamed",
                "Renaming Error");
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
