using System.Windows;

namespace GreenKeeper.Layout
{
    /// <summary>
    /// The size the main window opens at: the design size on the screen it is meant
    /// for and on any smaller one, on a larger screen larger in the same proportion as
    /// the screen. It never exceeds the work area, the part of the screen the taskbar
    /// leaves free.
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
