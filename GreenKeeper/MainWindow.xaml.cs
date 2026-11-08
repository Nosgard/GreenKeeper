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

namespace GreenKeeper
{
    /// <summary>
    /// Opens the windows that PlantListViewModel and DashboardViewModel ask for through their
    /// events and hands the results back to them, so those ViewModels never see a window class.
    /// </summary>
    public partial class MainWindow : Window
    {
        // How far the focus ring reaches beyond a control (Margin="-5" of FocusRing in
        // FocusStyles.xaml), plus some air.
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

            TitleBarTheme.Attach(this);
            FocusKeeper.Attach(this, PlantList, CareStatusList);
            FocusCue.Attach(this);

            // F6 switches between the regions, the arrow keys step inside each of them.
            FocusRegion[] regions =
            [
                new FocusRegion(Sidebar, start: SidebarStart),
                new FocusRegion(Dashboard, emptyHint: () => EmptyDashboardHint.Text)
            ];
            FocusRegions.Attach(this, regions);
            ArrowNavigation.Attach(this, regions.Select(region => region.Root).ToArray());

            _mainViewModel.Announced += MainViewModel_Announced;

            _mainViewModel.PlantList.AddPlantRequested += MainViewModel_AddPlantRequested;
            _mainViewModel.Dashboard.AddScheduleRequested += MainViewModel_AddScheduleRequested;
            _mainViewModel.Dashboard.EditScheduleRequested += MainViewModel_EditScheduleRequested;
            _mainViewModel.Dashboard.OpenNotesRequested += MainViewModel_OpenNotesRequested;
            _mainViewModel.PlantList.RenamePlantRequested += MainViewModel_RenamePlantRequested;

            // Otherwise the refresh timer keeps firing while the app shuts down.
            Closed += (_, _) => _mainViewModel.StopCareStatusRefreshTimer();

            Closing += (_, _) => SavePlacement();

            PreviewMouseDown += Window_PreviewMouseDown;

            Loaded += MainWindow_Loaded;

            DataContext = _mainViewModel;
        }

        // -- Keyboard Section --

        // Tab focuses an entry without choosing it, and the list chooses only with Space
        // or the arrow keys. While no plant is chosen, Enter opens the entry as users
        // expect. With another plant chosen, Enter only adds the entry to the selection and
        // opens nothing (see FocusStops.Focus).
        private void PlantEntry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || sender is not ListViewItem entry)
            {
                return;
            }

            entry.IsSelected = true;
            e.Handled = true;
        }

        // F6 into the sidebar lands on the chosen plant or the first one, and on the
        // search box while the list is empty.
        private UIElement? SidebarStart()
        {
            object? plant = PlantList.SelectedItem ?? PlantList.Items.Cast<object>().FirstOrDefault();

            return plant == null ? SearchTextBox : VisualTree.EntryOf(PlantList, plant);
        }

        // -- Responsive Size Section --

        // Sets the start size that suits the screen (see StartSize). The window opens at it
        // only without a saved placement, since RestorePlacement overwrites it. The content
        // is drawn at the scale of the window's current size.
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

            // RestoreBounds, so a maximized window keeps its normal position and size.
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

        // The keyboard scrolls a card button into view only up to the button's edge, so the
        // scroll area would cut off its focus ring. This handler asks again with FocusRingReach
        // added on every side.
        private void StatusPanel_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            // Requests with a rectangle, like the one raised below, and those of the panel
            // itself go on unchanged.
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
        // An exception from these handlers would reach App.ReportUnexpectedError, which shows
        // only a general message, so loading the plants and saving a dialog's result go through
        // RunAsync, which names the failed step in its error dialog. The notes window handles
        // its own failures.

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await RunAsync(() => _mainViewModel.PlantList.InitializeAsync(), "Failed to load the plant", "Loading Error");
        }

        private async void MainViewModel_AddPlantRequested(object? sender, EventArgs e)
        {
            var wizardView = new AddPlantWizardView();

            if (!wizardView.ShowDialog(this) || wizardView.CreatedPlant == null)
            {
                return;
            }

            await RunAsync(() => _mainViewModel.PlantList.AddPlantAsync(wizardView.CreatedPlant), "Failed to save the plant", "Saving Error");
        }

        private async void MainViewModel_AddScheduleRequested(object? sender, Plant plant)
        {
            var wizardView = new AddScheduleWizardView(plant, _dialogService);

            if (!wizardView.ShowDialog(this) || wizardView.Result == null)
            {
                return;
            }

            await RunAsync(() => wizardView.Result.SaveAsync(_mainViewModel.Dashboard), "Schedule could not be saved", "Saving Error");
        }

        // The edit dialog's result is saved like a result of the Add Schedule wizard: for the
        // database, editing a care schedule or the sunlight requirement and replacing it through
        // that wizard are the same operation.
        private async void MainViewModel_EditScheduleRequested(object? sender, (Plant plant, CareType care) e)
        {
            var editView = new EditScheduleView(e.plant, e.care, _timeProvider);

            if (!editView.ShowDialog(this) || editView.Result == null)
            {
                return;
            }

            await RunAsync(() => editView.Result.SaveAsync(_mainViewModel.Dashboard), "The change could not be saved", "Saving Error");
        }

        // The notes window saves through the callback itself, so nothing is left to persist here.
        private void MainViewModel_OpenNotesRequested(object? sender, Plant plant)
        {
            var notesView = new NotesView(plant, _dialogService, saveNotesAsync: text => _mainViewModel.Dashboard.UpdatePlantNotesAsync(plant, text));

            notesView.ShowDialog(this);
        }

        // Opens the rename dialog for the plant whose context menu was used, which need not be the
        // selected one.
        private async void MainViewModel_RenamePlantRequested(object? sender, Plant plant)
        {
            var renameView = new RenamePlantView(plant);

            if (!renameView.ShowDialog(this) || renameView.ConfirmedName == null)
            {
                return;
            }

            await RunAsync(
                () => _mainViewModel.PlantList.RenamePlantAsync(plant, renameView.ConfirmedName),
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

        // Clicking anywhere outside the search box takes the keyboard focus away from it, so an
        // empty box shows "Search..." again. A click inside the box keeps the focus there.
        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!SearchTextBox.IsFocused)
            {
                return;
            }

            if (e.OriginalSource is DependencyObject clickedElement && SearchTextBox.IsAncestorOf(clickedElement))
            {
                return;
            }

            Keyboard.ClearFocus();
        }
    }
}
