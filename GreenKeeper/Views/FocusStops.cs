using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GreenKeeper.Views
{
    /// <summary>
    /// The elements of a region the keyboard focus stops at: what Tab visits, and what
    /// the arrow keys (ArrowNavigation) and F6 (FocusRegions) walk through.
    /// </summary>
    public static class FocusStops
    {
        /// <summary>
        /// The stops of the region, in the order of Tab. A list and a drop-down are one
        /// stop each, whatever they are built from.
        /// </summary>
        public static List<Control> In(FrameworkElement region)
        {
            return VisualTree.Descendants<Control>(region)
                .Where(control => IsUsable(control) && IsStop(control) && !IsPartOfAStop(control, region))
                .ToList();
        }

        /// <summary>
        /// Whether the element is on screen and can be used right now. An element that was
        /// rebuilt in the meantime, a status card or a plant entry, is not.
        /// </summary>
        public static bool IsUsable(UIElement element)
        {
            return element.IsVisible && element.IsEnabled && PresentationSource.FromVisual(element) != null;
        }

        /// <summary>
        /// The entry of a list that takes the focus.
        /// </summary>
        /// <param name="choice">Why the list is entered: at an edge, or without a direction</param>
        /// <param name="chosenIndex">The entry the user chose, -1 for none</param>
        /// <param name="count">The number of entries, at least one</param>
        public static int EntryToFocus(EntryChoice choice, int chosenIndex, int count)
        {
            return choice switch
            {
                EntryChoice.First => 0,
                EntryChoice.Last => count - 1,

                // Nothing chosen yet: a list starts at its first entry.
                _ => Math.Max(chosenIndex, 0)
            };
        }

        /// <summary>
        /// Puts the focus on the stop. A list is one stop, so the focus goes to one of its
        /// entries, depending on how the list is entered, and that entry becomes the
        /// chosen one.
        /// </summary>
        public static void Focus(UIElement stop, EntryChoice choice = EntryChoice.Chosen)
        {
            // A list chooses an entry on its own only while it moves the focus itself, from
            // one entry to the next. A focus put on it from outside (an arrow key step, F6
            // coming back, a rebuilt list) would leave the ring on one plant while the
            // dashboard shows another. So the list is told which entry it is on, never the
            // entry itself: an entry that marks itself leaves the selection of the list
            // behind, and the two then name different plants.
            if (stop is ListBox { HasItems: true } list)
            {
                list.SelectedIndex = EntryToFocus(choice, list.SelectedIndex, list.Items.Count);
                (VisualTree.EntryOf(list, list.SelectedItem) ?? list).Focus();
                return;
            }

            if (stop is ListBoxItem entry && ItemsControl.ItemsControlFromItemContainer(entry) is ListBox owner)
            {
                owner.SelectedItem = entry.DataContext;
            }

            stop.Focus();
        }

        // A list is no tab stop itself. Tab goes to one of its entries, but here it
        // stands for them, as long as it has any.
        private static bool IsStop(Control control)
        {
            return control is ListBox list ? list.HasItems : control.Focusable && control.IsTabStop;
        }

        // An entry of a list or a part of a drop-down: their list or drop-down is the stop.
        private static bool IsPartOfAStop(Control control, FrameworkElement region)
        {
            var parent = VisualTreeHelper.GetParent(control);

            while (parent != null && parent != region)
            {
                if (parent is ListBox or ComboBox)
                {
                    return true;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return false;
        }
    }
}
