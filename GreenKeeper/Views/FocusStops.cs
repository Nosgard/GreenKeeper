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
            // A focus put on a list from outside selects no entry, so the ring would sit on
            // one plant while the dashboard shows another. Setting the list's SelectedIndex
            // keeps the two together. An entry's own IsSelected would not: it only adds the
            // entry to the selection, and the dashboard stays on the old plant.
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

        // Here a list stands for its entries, as long as it has any.
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
