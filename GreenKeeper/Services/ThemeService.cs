using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GreenKeeper.Services
{
    /// <summary>
    /// Concrete implementation that manipulates WPF's application-level
    /// resource dictionaries. ViewModels only ever see IThemeService, so
    /// they stay free of WPF-specific types and remain testable with a fake.
    /// </summary>
    public class ThemeService : IThemeService
    {
        // Fast enough to feel responsive, slow enough to read as a transition.
        private const int TransitionMilliseconds = 200;

        private static readonly Dictionary<Theme, string> ThemePaths = new()
        {
            [Theme.Dark] = "/Resources/Styles/Themes/DarkTheme.xaml",
            [Theme.Bright] = "/Resources/Styles/Themes/BrightTheme.xaml"
        };

        // Only ever read for their target colors, so one instance per theme is enough.
        private readonly Dictionary<Theme, ResourceDictionary> _themeCache = new();

        // The dictionary that ends up in Application.Resources. Its brushes are
        // unfrozen clones, so their Color can be animated in place. Built lazily
        // on the first theme change.
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

            // Set before animating, so IsDarkTheme - and the toggle icon with it -
            // flips right away instead of trailing the fade.
            CurrentTheme = theme;

            foreach (var key in live.Keys.Cast<object>().ToList())
            {
                if (live[key] is not SolidColorBrush brush) continue;
                if (target[key] is not SolidColorBrush targetBrush) continue;

                AnimateTo(live, key, brush, targetBrush.Color);
            }

            if (isFirstChange)
            {
                // Only now, with the animations already running - an animating
                // brush cannot be sealed. See the note in AnimateTo.
                SwapIntoApplicationResources(live);
            }
        }

        /// <summary>
        /// Fades one brush to its new color. DynamicResource resolves to the brush
        /// object, so every control using it follows along without a resource lookup.
        /// </summary>
        private void AnimateTo(ResourceDictionary live, object key, SolidColorBrush brush, Color targetColor)
        {
            var animation = new ColorAnimation
            {
                To = targetColor,
                Duration = TimeSpan.FromMilliseconds(TransitionMilliseconds),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            // Never release this clock. WPF seals Freezables that live in
            // Application.Resources, and a sealed brush rejects any animation -
            // but a brush that is animating cannot be sealed. Clearing the clock
            // (to write the color as a base value, say) brings back "the object
            // is sealed or frozen" on the next theme change.
            if (!brush.IsFrozen)
            {
                // Replaces a running animation and picks up from its current
                // color, so rapid toggling stays smooth.
                brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                return;
            }

            // Should a brush end up sealed anyway, swap in an animating clone
            // instead of throwing - consumers follow it through the dictionary.
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
        /// The theme dictionary App.xaml loaded, found by source path rather than
        /// by index - the position would shift if App.xaml is ever reordered.
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
