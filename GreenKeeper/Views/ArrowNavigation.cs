using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GreenKeeper.Views
{
    /// <summary>
    /// Lets the arrow keys step through everything in a region of a window, as ArrowKeys
    /// describes it. WPF's own arrow key handling goes by the position on screen and
    /// leaves traps: a scroll area and a list keep the focus, a text field lets it go
    /// nowhere, and a step can end in another region. Here every element is one step
    /// from its neighbor in the order of Tab, and only F6 crosses to another region. A
    /// closed drop-down gives its arrow keys to that walk; Space opens it, and inside
    /// the arrow keys choose again.
    /// </summary>
    public sealed class ArrowNavigation
    {
        private readonly IReadOnlyList<FrameworkElement> _regions;

        private ArrowNavigation(IReadOnlyList<FrameworkElement> regions)
        {
            _regions = regions;
        }

        /// <summary>
        /// Lets the arrow keys step through each of the regions; without regions, the
        /// whole window is one. The key event of the window holds the navigation, so it
        /// lives as long as the window.
        /// </summary>
        public static void Attach(Window window, params FrameworkElement[] regions)
        {
            var navigation = new ArrowNavigation(regions.Length == 0 ? [window] : regions);
            window.PreviewKeyDown += navigation.Window_PreviewKeyDown;
        }

        // Plain keys only: with Shift the arrow keys select text, with Ctrl they jump by word.
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None || Keyboard.FocusedElement is not UIElement focused)
            {
                return;
            }

            bool wasUsed = e.Key switch
            {
                Key.Space => TryOpenSelection(focused),
                Key.Up or Key.Down or Key.Left or Key.Right => TryStep(focused, e.Key),
                _ => false
            };

            // Only ever set: another handler of the window may have used the key already.
            if (wasUsed)
            {
                e.Handled = true;
            }
        }

        private static bool TryOpenSelection(UIElement focused)
        {
            if (focused is not ComboBox { IsDropDownOpen: false } selection)
            {
                return false;
            }

            selection.IsDropDownOpen = true;
            return true;
        }

        private bool TryStep(UIElement focused, Key key)
        {
            // An open drop-down or menu is drawn outside of every region and keeps its keys.
            if (RegionOf(focused) is not { } region)
            {
                return false;
            }

            var stops = FocusStops.In(region);

            if (stops.Count == 0)
            {
                return false;
            }

            int index = stops.FindIndex(stop => Holds(stop, focused));

            // The window itself or a list without entries: in a gap, every arrow key steps.
            bool isInAGap = index < 0;
            int step = isInAGap ? ArrowKeys.StepFor(key, FocusStopKind.Button) : StepAt(stops[index], focused, key);

            if (step == ArrowKeys.Stays)
            {
                return false;
            }

            int target = isInAGap
                ? ArrowKeys.NeighborOfGap(StopsBefore(focused, stops, region), step, stops.Count)
                : ArrowKeys.NeighborOf(index, step, stops.Count);

            FocusStops.Focus(stops[target], step == ArrowKeys.Back ? EntryChoice.Last : EntryChoice.First);
            return true;
        }

        // The window itself has the focus right after the start: the first region is the
        // one the arrow keys lead into.
        private FrameworkElement? RegionOf(UIElement focused)
        {
            return _regions.FirstOrDefault(region => Holds(region, focused))
                ?? (focused is Window ? _regions[0] : null);
        }

        // The step the key takes from the stop that holds the focus.
        private static int StepAt(Control stop, UIElement focused, Key key)
        {
            if (stop is ListBox list && focused is ListBoxItem entry)
            {
                return ArrowKeys.StepForEntry(key, list.ItemContainerGenerator.IndexFromContainer(entry), list.Items.Count);
            }

            return ArrowKeys.StepFor(key, KindOf(stop));
        }

        private static FocusStopKind KindOf(Control stop)
        {
            return stop switch
            {
                // The list itself has the focus only until one of its entries takes it.
                ListBox => FocusStopKind.KeepsAllArrows,
                TextBox { AcceptsReturn: true } => FocusStopKind.KeepsAllArrows,
                TextBox => FocusStopKind.TextLine,
                ComboBox { IsDropDownOpen: true } => FocusStopKind.KeepsAllArrows,
                ComboBox => FocusStopKind.Selection,
                _ => FocusStopKind.Button
            };
        }

        // How many stops come before the gap the focus is in. Everything in a region is
        // drawn in the order of Tab.
        private static int StopsBefore(UIElement focused, List<Control> stops, FrameworkElement region)
        {
            var drawn = VisualTree.Descendants<UIElement>(region).ToList();
            int place = drawn.IndexOf(focused);

            return stops.Count(stop => drawn.IndexOf(stop) < place);
        }

        private static bool Holds(FrameworkElement element, UIElement focused)
        {
            return ReferenceEquals(element, focused) || element.IsAncestorOf(focused);
        }
    }
}
