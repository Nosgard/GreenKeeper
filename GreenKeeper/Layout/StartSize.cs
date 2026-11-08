using System.Windows;

namespace GreenKeeper.Layout
{
    /// <summary>
    /// The size the main window opens at without a stored window placement: the design
    /// size, enlarged in proportion when the work area is larger than the reference screen
    /// on both sides. It never exceeds the work area.
    /// </summary>
    public static class StartSize
    {
        public static Size For(Size designSize, Size referenceScreen, Size workArea)
        {
            // The tighter side decides, so the window keeps its proportions while it grows.
            double growth = Math.Max(1, Math.Min(workArea.Width / referenceScreen.Width, workArea.Height / referenceScreen.Height));

            return new Size(
                Math.Min(designSize.Width * growth, workArea.Width),
                Math.Min(designSize.Height * growth, workArea.Height));
        }
    }
}
