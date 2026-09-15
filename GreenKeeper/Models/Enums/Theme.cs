using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Models.Enums
{
    /// <summary>
    /// The available color themes. Unlike CareType, these values are never
    /// persisted as integers in the database - the user's choice is stored
    /// as text in a settings file - so the numeric values carry no meaning
    /// and can be reordered safely.
    /// </summary>
    public enum Theme
    {
        Dark,
        Bright
    }
}
