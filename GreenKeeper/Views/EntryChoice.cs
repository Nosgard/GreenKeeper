namespace GreenKeeper.Views
{
    /// <summary>
    /// Which entry of a list takes the keyboard focus when the list as a whole gets it
    /// (see FocusStops.EntryToFocus).
    /// </summary>
    public enum EntryChoice
    {
        /// <summary>The entry the user chose, so the focus comes back to where they were.</summary>
        Chosen,

        /// <summary>The first entry: a step of the arrow keys enters the list from above.</summary>
        First,

        /// <summary>The last entry: a step of the arrow keys enters the list from below.</summary>
        Last
    }
}
