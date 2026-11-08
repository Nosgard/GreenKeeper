using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GreenKeeper.Layout
{
    /// <summary>
    /// Draws a window at the scale factor set on it. Everything inside the frame keeps
    /// its designed sizes and is enlarged or reduced as a whole.
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

        // Scales the window, plus context menus and tooltips, which would otherwise keep
        // their designed size. Every other element only passes the factor on.
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
            // A dialog is scaled before it is shown. Without this call its content would stay
            // unscaled.
            window.ApplyTemplate();

            return VisualTreeHelper.GetChildrenCount(window) > 0
                ? VisualTreeHelper.GetChild(window, 0) as FrameworkElement
                : null;
        }
    }
}
