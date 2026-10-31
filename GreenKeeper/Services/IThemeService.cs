using GreenKeeper.Models.Enums;

namespace GreenKeeper.Services
{
    public interface IThemeService
    {
        /// <summary>
        /// The theme the app shows. During a fade it is already the new one.
        /// </summary>
        Theme CurrentTheme { get; }

        /// <summary>
        /// Fades the app to the given theme. Open windows follow without being recreated.
        /// </summary>
        void ApplyTheme(Theme theme);
    }
}
