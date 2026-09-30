using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.Models.Enums
{
    /// <summary>
    /// All possible types of care schedules. The values are numbered explicitly because EF Core stores
    /// enums as integers - those numbers are part of the persisted data format.
    /// 
    /// Warning:
    /// Reordering or renumbering members would silently re-map existing user records to the wrong
    /// care type. Renaming a member is safe, as long as its number stays the same.
    /// </summary>
    public enum CareType
    {
        Watering = 0,
        Fertilizing = 1,
        Sunlight = 2
    }
}
