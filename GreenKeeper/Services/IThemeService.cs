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
        /// Fades the application over to the given theme. Because every style
        /// references its colors via DynamicResource, the whole UI transitions
        /// along with it - no windows need to be recreated or reopened.
        /// </summary>
        void ApplyTheme(Theme theme);
    }
}
