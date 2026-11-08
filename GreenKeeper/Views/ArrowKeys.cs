using System.Windows.Input;

namespace GreenKeeper.Views
{
    /// <summary>
    /// An arrow key steps to the previous or next element of a region in Tab order,
    /// around the end, so the arrow keys alone reach everything in it. An element that
    /// needs the key keeps it: a text field, a list, an open drop-down. A closed
    /// drop-down does not, or its value would change while the user walks past.
    /// </summary>
    public static class ArrowKeys
    {
        /// <summary>The step to the previous element.</summary>
        public const int Back = -1;

        /// <summary>The step to the next element.</summary>
        public const int On = 1;

        /// <summary>No step: the key stays with the element that has the focus.</summary>
        public const int Stays = 0;

        /// <summary>
        /// The step an arrow key takes from an element of the kind: Back, On or Stays.
        /// </summary>
        /// <param name="key">One of the four arrow keys</param>
        /// <param name="kind">The kind of element that has the focus</param>
        public static int StepFor(Key key, FocusStopKind kind)
        {
            return kind switch
            {
                FocusStopKind.Button or FocusStopKind.Selection => PlainStep(key),
                FocusStopKind.TextLine => IsVertical(key) ? PlainStep(key) : Stays,

                // The rest needs every arrow key for itself.
                _ => Stays
            };
        }

        /// <summary>
        /// The step an arrow key takes from an entry of a list. Up and down move the list's
        /// selection until its end and then step out of the list, which keeps the elements
        /// beyond it reachable. Left and right always step.
        /// </summary>
        /// <param name="key">One of the four arrow keys</param>
        /// <param name="entryIndex">The entry that has the focus</param>
        /// <param name="entryCount">The number of entries in the list</param>
        public static int StepForEntry(Key key, int entryIndex, int entryCount)
        {
            int step = PlainStep(key);
            bool isAtTheEnd = step == Back ? entryIndex == 0 : entryIndex == entryCount - 1;

            return IsVertical(key) && !isAtTheEnd ? Stays : step;
        }

        /// <summary>
        /// The element one step away from another one, around the end of the region.
        /// </summary>
        /// <param name="index">The element the step starts from</param>
        /// <param name="step">Back or On</param>
        /// <param name="count">The number of elements in the region, at least one</param>
        public static int NeighborOf(int index, int step, int count)
        {
            return (index + step + count) % count;
        }

        /// <summary>
        /// The element one step away from a gap between the elements. The focus is in a
        /// gap while it is on none of them: on the window right after the start, or on a
        /// list that has lost its last entry.
        /// </summary>
        /// <param name="elementsBefore">How many elements of the region come before the gap</param>
        /// <param name="step">Back or On</param>
        /// <param name="count">The number of elements in the region, at least one</param>
        public static int NeighborOfGap(int elementsBefore, int step, int count)
        {
            // Stepping on arrives at the first element after the gap.
            return step == On ? elementsBefore % count : NeighborOf(elementsBefore, Back, count);
        }

        private static int PlainStep(Key key)
        {
            return key is Key.Up or Key.Left ? Back : On;
        }

        private static bool IsVertical(Key key)
        {
            return key is Key.Up or Key.Down;
        }
    }
}
