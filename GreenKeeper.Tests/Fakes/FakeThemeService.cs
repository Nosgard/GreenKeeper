using GreenKeeper.Models.Enums;
using GreenKeeper.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Tests.Fakes
{
    public class FakeThemeService : IThemeService
    {
        public Theme CurrentTheme { get; private set; } = Theme.Dark;
        public int ApplyThemeCallCount { get; private set; }

        public void ApplyTheme(Theme theme)
        {
            ApplyThemeCallCount++;
            CurrentTheme = theme;
        }
    }
}
