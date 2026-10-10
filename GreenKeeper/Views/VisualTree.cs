using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GreenKeeper.Views
{
    /// <summary>
    /// Searches the elements a window is drawn from, for what XAML does not name: the
    /// buttons inside a status card, the fields of a wizard step.
    /// </summary>
    public static class VisualTree
    {
        // Every element of the type below the parent, in the order the XAML declares them.
        // Lazy: the search ends with the first match it is asked for.
        public static IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T match)
                {
                    yield return match;
                }

                foreach (var below in Descendants<T>(child))
                {
                    yield return below;
                }
            }
        }

        // The entry a list shows the item in. A list only builds the entries in view,
        // so the item is scrolled into view first.
        public static UIElement? EntryOf(ListBox list, object item)
        {
            list.ScrollIntoView(item);
            list.UpdateLayout();

            return list.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
        }
    }
}
