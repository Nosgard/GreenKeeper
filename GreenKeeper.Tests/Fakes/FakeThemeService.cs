using GreenKeeper.Models.Enums;
using GreenKeeper.Services;

namespace GreenKeeper.Tests.Fakes
{
    public class FakeThemeService : IThemeService
    {
        public Theme CurrentTheme { get; private set; } = Theme.Bright;
        public int ApplyThemeCallCount { get; private set; }

        public void ApplyTheme(Theme theme)
        {
            ApplyThemeCallCount++;
            CurrentTheme = theme;
        }
    }
}
