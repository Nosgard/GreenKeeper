using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GreenKeeper.Layout
{
    /// <summary>
    /// Draws a window at a scale factor. Everything inside the frame keeps its designed
    /// sizes and is enlarged or reduced as a whole. The factor is set on the window and
    /// inherited by all its elements.
    /// </summary>
    public static class UiScale
    {
        public static readonly DependencyProperty FactorProperty = DependencyProperty.RegisterAttached(
            "Factor",
            typeof(double),
            typeof(UiScale),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.Inherits, OnFactorChanged));

        public static double GetFactor(DependencyObject element)
        {
            return (double)element.GetValue(FactorProperty);
        }

        public static void SetFactor(DependencyObject element, double factor)
        {
            element.SetValue(FactorProperty, factor);
        }

        // Runs for every element the factor reaches, but scales only two kinds: the window
        // itself, and a context menu or tooltip, which opens in a window of its own and
        // takes the factor over then.
        private static void OnFactorChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            var scaled = element switch
            {
                Window window => RootInsideFrame(window),
                ContextMenu or ToolTip => (FrameworkElement)element,
                _ => null
            };

            if (scaled != null)
            {
                double factor = (double)e.NewValue;
                scaled.LayoutTransform = new ScaleTransform(factor, factor);
            }
        }

        // The root of the window's template rather than its Content: a margin around the
        // content would otherwise keep its size.
        private static FrameworkElement? RootInsideFrame(Window window)
        {
            // A dialog is scaled before it is shown, when its template is not applied yet.
            window.ApplyTemplate();

            return VisualTreeHelper.GetChildrenCount(window) > 0
                ? VisualTreeHelper.GetChild(window, 0) as FrameworkElement
                : null;
        }
    }
}
