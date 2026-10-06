using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace GreenKeeper.Views
{
    /// <summary>
    /// F6 moves the keyboard focus to the next region of a window, Shift+F6 back to the
    /// previous one: in the main window between the sidebar and the dashboard, so a
    /// keyboard user does not have to Tab through the one to reach the other. A region
    /// gets the focus back where the user left it; on the first visit, or when that
    /// element is gone, the focus goes to where the region starts.
    /// </summary>
    public sealed class FocusRegions
    {
        private readonly Window _window;
        private readonly List<FocusRegion> _regions;
        private readonly Dictionary<FocusRegion, UIElement> _lastFocused = new();

        private FocusRegions(Window window, List<FocusRegion> regions)
        {
            _window = window;
            _regions = regions;
        }

        /// <summary>
        /// Lets F6 and Shift+F6 move through the regions, in the order given. The events
        /// of the window hold the regions, so they live as long as the window.
        /// </summary>
        public static void Attach(Window window, params FocusRegion[] regions)
        {
            var focusRegions = new FocusRegions(window, regions.ToList());
            window.GotKeyboardFocus += focusRegions.Window_GotKeyboardFocus;
            window.PreviewKeyDown += focusRegions.Window_PreviewKeyDown;
        }

        private void Window_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus is not UIElement element)
            {
                return;
            }

            foreach (var region in _regions.Where(region => region.Root.IsAncestorOf(element)))
            {
                _lastFocused[region] = element;
            }
        }

        // F6 and Shift+F6 only: Ctrl+F6 and Alt+F6 belong to other conventions.
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.F6 || (Keyboard.Modifiers & ~ModifierKeys.Shift) != ModifierKeys.None)
            {
                return;
            }

            e.Handled = true;
            bool backwards = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

            // WPF asks a command whether it can run only after the next key, with low
            // priority, after this handler. Until then a button whose command cannot run any
            // more still looks enabled; F6 would focus it and lose the focus again at once.
            // So F6 waits for the answer.
            CommandManager.InvalidateRequerySuggested();
            _window.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, () => MoveFocus(backwards));
        }

        private void MoveFocus(bool backwards)
        {
            int? current = RegionWithFocus();

            // The region with the focus is never the target, so its own start, which may
            // scroll, is not even looked for.
            var targets = _regions.Select((region, index) => index == current ? null : TargetIn(region)).ToList();
            int? next = FocusRegionOrder.Next(current, targets.Select(target => target != null).ToList(), backwards);

            if (next is int index)
            {
                FocusStops.Focus(targets[index]!);
                return;
            }

            // Nothing moves, so a screen reader is told why; a sighted user sees the hint in
            // the empty region already.
            string hints = string.Join(". ", _regions
                .Where((region, index) => index != current && targets[index] == null)
                .Select(region => region.EmptyHint?.Invoke())
                .Where(hint => !string.IsNullOrEmpty(hint)));
            Announcer.Announce(_window, hints);
        }

        private int? RegionWithFocus()
        {
            int index = _regions.FindIndex(region => region.Root.IsKeyboardFocusWithin);
            return index < 0 ? null : index;
        }

        // Where the user left off in the region, else where it starts, else its first stop.
        private UIElement? TargetIn(FocusRegion region)
        {
            if (_lastFocused.TryGetValue(region, out var last) && CanTakeFocus(last, region))
            {
                return last;
            }

            if (region.Start?.Invoke() is { } start && CanTakeFocus(start, region))
            {
                return start;
            }

            return FocusStops.In(region.Root).FirstOrDefault();
        }

        // Usable, and still part of the region: a card that was rebuilt is a new element.
        private static bool CanTakeFocus(UIElement element, FocusRegion region)
        {
            return element.Focusable && FocusStops.IsUsable(element) && region.Root.IsAncestorOf(element);
        }
    }
}
