using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GreenKeeper.Services
{
    /// <summary>
    /// Concrete implementation that manipulates WPF's application-level
    /// resource dictionaries. ViewModels only ever see IThemeService, so
    /// they stay free of WPF-specific types and remain testable with a fake.
    /// </summary>
    public class ThemeService : IThemeService
    {
        private const string DarkThemePath = "/Resources/Styles/Themes/DarkTheme.xaml";
        private const string BrightThemePath = "/Resources/Styles/Themes/BrightTheme.xaml";

        public Theme CurrentTheme { get; private set; } = Theme.Bright;

        public void ApplyTheme(Theme theme)
        {
            var dictionaries = Application.Current.Resources.MergedDictionaries;

            // Find whichever theme dictionary is currently loaded. Identified
            // by its source path rather than by index, since the position in
            // the list isn't guaranteed and could shift if the order in
            // App.xaml ever changes.
            var existingTheme = dictionaries.FirstOrDefault(d =>
                d.Source != null &&
                (d.Source.OriginalString.EndsWith("DarkTheme.xaml", StringComparison.OrdinalIgnoreCase) ||
                 d.Source.OriginalString.EndsWith("BrightTheme.xaml", StringComparison.OrdinalIgnoreCase)));

            var newTheme = new ResourceDictionary
            {
                Source = new Uri(theme == Theme.Dark ? DarkThemePath : BrightThemePath, UriKind.Relative)
            };

            // Insert the new dictionary at the same position the old one had,
            // so it keeps being loaded BEFORE the style files that depend on
            // it. Adding it at the end would still work for DynamicResource
            // lookups, but keeping the order intact avoids surprises.
            if (existingTheme != null)
            {
                int index = dictionaries.IndexOf(existingTheme);
                dictionaries.Insert(index, newTheme);
                dictionaries.Remove(existingTheme);
            }
            else
            {
                dictionaries.Insert(0, newTheme);
            }

            CurrentTheme = theme;
        }
    }
}
