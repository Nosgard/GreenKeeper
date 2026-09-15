using GreenKeeper.Models;
using GreenKeeper.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Tests.Fakes
{
    /// <summary>
    /// Keeps the settings in memory instead of on disk, so tests never touch
    /// the user's real settings.json.
    /// </summary>
    public class FakeSettingsService : ISettingsService
    {
        public AppSettings Settings { get; set; } = new AppSettings();
        public int SaveCallCount { get; private set; }

        public AppSettings Load() => Settings;

        public void Save(AppSettings settings)
        {
            SaveCallCount++;
            Settings = settings;
        }
    }
}
