using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Models
{
    /// <summary>
    /// Everything the app remembers between sessions that does not belong in the
    /// database - user preferences rather than data. Every property carries a
    /// default, so a file written by an older version stays readable: properties
    /// it does not contain simply keep their default instead of failing the read.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// The theme to restore on the next start. Serialized as text
        /// ("Bright"/"Dark") rather than as a number - see SettingsService.
        /// </summary>
        public Theme Theme { get; set; } = Theme.Bright;
    }
}
