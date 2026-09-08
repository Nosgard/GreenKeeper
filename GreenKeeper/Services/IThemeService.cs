using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Services
{
    public interface IThemeService
    {
        /// <summary>
        /// The theme currently applied to the application.
        /// </summary>
        Theme CurrentTheme { get; }

        /// <summary>
        /// Swaps the active theme dictionary in the application's merged
        /// dictionaries. Because every style references its colors via
        /// DynamicResource, the whole UI updates immediately - no windows
        /// need to be recreated or reopened.
        /// </summary>
        void ApplyTheme(Theme theme);
    }
}
