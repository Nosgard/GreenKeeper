using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace GreenKeeper.Views
{
    /// <summary>
    /// Lets the title bar Windows draws follow the theme ThemeService applies. Without this it
    /// follows the Windows setting, so a window in the dark theme can get a light title bar.
    /// </summary>
    public static class TitleBarTheme
    {
        private const int UseImmersiveDarkMode = 20;
        private const int CaptionColor = 35;

        // Tells Windows to draw the caption in its own color again.
        private const int DefaultColor = unchecked((int)0xFFFFFFFF);

        private static int _caption = DefaultColor;

        /// <summary>
        /// Switches the title bars of all open windows, and of every window opened
        /// from now on, to dark or light.
        /// </summary>
        /// <param name="darkCaption">The color of the dark title bar, null for a light one</param>
        public static void Apply(Color? darkCaption)
        {
            _caption = darkCaption is Color color ? color.R | (color.G << 8) | (color.B << 16) : DefaultColor;

            foreach (Window window in Application.Current.Windows)
            {
                Paint(window);
            }
        }

        /// <summary>
        /// Paints the window's title bar in the current theme as soon as Windows has
        /// created it. Called once per window, from its constructor.
        /// </summary>
        public static void Attach(Window window)
        {
            window.SourceInitialized += (_, _) => Paint(window);
        }

        private static void Paint(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;

            if (handle == IntPtr.Zero)
            {
                return;
            }

            // On older Windows versions the title bar keeps the Windows setting.
            int isDark = _caption != DefaultColor ? 1 : 0;
            DwmSetWindowAttribute(handle, UseImmersiveDarkMode, ref isDark, sizeof(int));
            DwmSetWindowAttribute(handle, CaptionColor, ref _caption, sizeof(int));
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);
    }
}
