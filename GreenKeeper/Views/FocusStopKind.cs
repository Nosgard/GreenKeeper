namespace GreenKeeper.Views
{
    /// <summary>
    /// What kind of element has the focus, seen from the arrow keys: which of them the
    /// element needs for itself (see ArrowKeys). A list entry is no kind of its own. Its
    /// place in the list decides (see ArrowKeys.StepForEntry).
    /// </summary>
    public enum FocusStopKind
    {
        /// <summary>Needs no arrow key for itself.</summary>
        Button,

        /// <summary>A text field of one line: left and right move its cursor.</summary>
        TextLine,

        /// <summary>A closed drop-down: needs none, its value is chosen while it is open.</summary>
        Selection,

        /// <summary>A text of several lines, an open drop-down, a list that has the focus
        /// itself rather than one of its entries: needs all four.</summary>
        KeepsAllArrows
    }
}
