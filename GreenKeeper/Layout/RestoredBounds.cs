using GreenKeeper.Models;
using System.Windows;

namespace GreenKeeper.Layout
{
    /// <summary>
    /// Where the main window opens after a restart: at its stored place, as far as the
    /// current screens still have it. A screen may have been unplugged or changed its
    /// resolution in the meantime, so the window never exceeds the screen area and
    /// never lies partly outside of it.
    /// </summary>
    public static class RestoredBounds
    {
        public static Rect For(WindowPlacement stored, Rect screenArea)
        {
            // A size below zero can only come from a file that was changed by hand.
            double width = Math.Clamp(stored.Width, 0, screenArea.Width);
            double height = Math.Clamp(stored.Height, 0, screenArea.Height);

            return new Rect(
                Math.Clamp(stored.Left, screenArea.Left, screenArea.Right - width),
                Math.Clamp(stored.Top, screenArea.Top, screenArea.Bottom - height),
                width,
                height);
        }
    }
}
