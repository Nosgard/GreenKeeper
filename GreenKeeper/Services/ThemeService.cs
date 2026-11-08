using GreenKeeper.Models.Enums;
using GreenKeeper.Views;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GreenKeeper.Services
{
    /// <summary>
    /// Fades the app to another theme. The first change puts a copy of the loaded theme with
    /// animatable brushes into Application.Resources; later changes only fade the colors of
    /// those brushes.
    /// </summary>
    public class ThemeService : IThemeService
    {
        // 200 ms reads as a fade without keeping the user waiting.
        private const int TransitionMilliseconds = 200;

        private static readonly Dictionary<Theme, string> ThemePaths = new()
        {
            [Theme.Dark] = "/Resources/Styles/Themes/DarkTheme.xaml",
            [Theme.Bright] = "/Resources/Styles/Themes/BrightTheme.xaml"
        };

        // Only read for their target colors, so one instance per theme is enough.
        private readonly Dictionary<Theme, ResourceDictionary> _themeCache = new();

        // The copy in Application.Resources; null until ApplyTheme first runs.
        private ResourceDictionary? _liveTheme;

        public Theme CurrentTheme { get; private set; } = Theme.Bright;

        public void ApplyTheme(Theme theme)
        {
            if (theme == CurrentTheme && _liveTheme != null)
            {
                return;
            }

            var target = LoadTheme(theme);
            var isFirstChange = _liveTheme == null;
            var live = _liveTheme ??= CloneBrushes(FindLoadedTheme() ?? LoadTheme(CurrentTheme));

            // Set before animating, so IsDarkTheme and the toggle icon with it flip at once
            // instead of after the fade.
            CurrentTheme = theme;

            // The dark title bar takes the window background, not the Windows accent color.
            TitleBarTheme.Apply(theme == Theme.Dark ? ((SolidColorBrush)target["WindowBackground"]).Color : null);

            foreach (var key in live.Keys.Cast<object>().ToList())
            {
                if (live[key] is not SolidColorBrush brush || target[key] is not SolidColorBrush targetBrush)
                {
                    continue;
                }

                AnimateTo(live, key, brush, targetBrush.Color);
            }

            if (isFirstChange)
            {
                // Only now that the animations run (see the note in AnimateTo).
                SwapIntoApplicationResources(live);
            }
        }

        /// <summary>
        /// Fades one brush to its new color. Everything that takes it through DynamicResource
        /// fades along. An element with StaticResource keeps the brush it got, and its color no
        /// longer changes once this service puts another brush in that place (see
        /// SwapIntoApplicationResources).
        /// </summary>
        private static void AnimateTo(ResourceDictionary live, object key, SolidColorBrush brush, Color targetColor)
        {
            var animation = new ColorAnimation
            {
                To = targetColor,
                Duration = TimeSpan.FromMilliseconds(TransitionMilliseconds),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            // Keep this animation, also after the fade. A brush in Application.Resources
            // without one fails its next fade with "the object is sealed or frozen".
            if (!brush.IsFrozen)
            {
                // Replaces a running animation and picks up from its current
                // color, so rapid toggling stays smooth.
                brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                return;
            }

            // If a brush cannot fade anyway, an animating clone takes its place in the theme.
            // The color still fades, and everything that takes it through DynamicResource
            // follows.
            var replacement = brush.CloneCurrentValue();
            replacement.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            live[key] = replacement;
        }

        /// <summary>
        /// Copies a theme dictionary with unfrozen brush clones. Same colors, so
        /// swapping the copy in for the original is invisible.
        /// </summary>
        private static ResourceDictionary CloneBrushes(ResourceDictionary source)
        {
            var clone = new ResourceDictionary();

            foreach (var key in source.Keys.Cast<object>())
            {
                clone[key] = source[key] is SolidColorBrush brush
                    ? brush.CloneCurrentValue()
                    : source[key];
            }

            return clone;
        }

        /// <summary>
        /// The theme dictionary App.xaml loaded, found by its source path. An index would shift
        /// if App.xaml were reordered.
        /// </summary>
        private static ResourceDictionary? FindLoadedTheme()
        {
            return Application.Current.Resources.MergedDictionaries.FirstOrDefault(d =>
                d.Source != null &&
                (d.Source.OriginalString.EndsWith("DarkTheme.xaml", StringComparison.OrdinalIgnoreCase) ||
                 d.Source.OriginalString.EndsWith("BrightTheme.xaml", StringComparison.OrdinalIgnoreCase)));
        }

        private static void SwapIntoApplicationResources(ResourceDictionary animatable)
        {
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            var loaded = FindLoadedTheme();

            if (loaded == null)
            {
                dictionaries.Insert(0, animatable);
                return;
            }

            // Keep the old position, so the theme still loads before the styles
            // that depend on it, and insert before removing, so no frame is left
            // without a theme.
            int index = dictionaries.IndexOf(loaded);
            dictionaries.Insert(index, animatable);
            dictionaries.Remove(loaded);
        }

        private ResourceDictionary LoadTheme(Theme theme)
        {
            if (!_themeCache.TryGetValue(theme, out var dictionary))
            {
                dictionary = new ResourceDictionary
                {
                    Source = new Uri(ThemePaths[theme], UriKind.Relative)
                };

                _themeCache[theme] = dictionary;
            }

            return dictionary;
        }
    }
}
