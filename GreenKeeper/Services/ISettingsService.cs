using GreenKeeper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Services
{
    public interface ISettingsService
    {
        /// <summary>
        /// Reads the stored preferences. Never throws: a missing, empty or damaged
        /// file yields the defaults, so a first start and a corrupted file behave
        /// the same way from the caller's point of view.
        /// </summary>
        AppSettings Load();

        /// <summary>
        /// Writes the preferences to disk. Never throws either - the user loses a
        /// preference on the next start, which beats taking the app down over it.
        /// </summary>
        void Save(AppSettings settings);
    }
}
