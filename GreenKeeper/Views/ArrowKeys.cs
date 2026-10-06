using System.Windows.Input;

namespace GreenKeeper.Views
{
    /// <summary>
    /// What an arrow key does inside a region of a window: it steps to the previous or
    /// the next element in the order of Tab, around the end, so the arrow keys alone
    /// reach everything in the region. An element that needs an arrow key for itself
    /// keeps it: the cursor of a text field, the selection of a list, an open
    /// drop-down. A closed drop-down does not, or its value would change while the
    /// user walks through the region.
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
        /// The step an arrow key takes from an entry of a list. Up and down belong to the
        /// list, which moves its selection with them, until its end: there they step on
        /// to the neighbor of the list, or nothing above or below it could be reached.
        /// Left and right always step.
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
        /// The element next to another one on the side of the step, around the end of
        /// the region.
        /// </summary>
        /// <param name="index">The element the step starts from</param>
        /// <param name="step">Back or On</param>
        /// <param name="count">The number of elements in the region, at least one</param>
        public static int NeighborOf(int index, int step, int count)
        {
            return (index + step + count) % count;
        }

        /// <summary>
        /// The element next to a gap between the elements, on the side of the step. The
        /// focus is in a gap while it is on none of them: on the window right after the
        /// start, or on a list that just lost its last entry.
        /// </summary>
        /// <param name="elementsBefore">How many elements of the region come before the gap</param>
        /// <param name="step">Back or On</param>
        /// <param name="count">The number of elements in the region, at least one</param>
        public static int NeighborOfGap(int elementsBefore, int step, int count)
        {
            // The first element after the gap is the one a step on arrives at.
            return step == On ? elementsBefore % count : NeighborOf(elementsBefore, Back, count);
        }

        // Up and left go back, down and right on, where nothing keeps the key.
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
