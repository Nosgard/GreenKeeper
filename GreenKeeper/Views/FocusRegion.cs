using System.Windows;

namespace GreenKeeper.Views
{
    /// <summary>
    /// One region of a window that F6 moves the focus to (see FocusRegions).
    /// </summary>
    /// <param name="root">The element the region consists of</param>
    /// <param name="start">Where the focus goes on the first visit; without it, the first
    /// element Tab would reach in the region</param>
    /// <param name="emptyHint">What a screen reader is told when F6 cannot go anywhere
    /// because this region has nothing to focus</param>
    public sealed class FocusRegion(FrameworkElement root, Func<UIElement?>? start = null, Func<string>? emptyHint = null)
    {
        public FrameworkElement Root { get; } = root;

        public Func<UIElement?>? Start { get; } = start;

        public Func<string>? EmptyHint { get; } = emptyHint;
    }
}
