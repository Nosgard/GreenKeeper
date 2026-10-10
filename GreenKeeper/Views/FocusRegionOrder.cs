namespace GreenKeeper.Views
{
    /// <summary>
    /// The order in which F6 visits the regions of a window: on to the next one, or back
    /// to the previous one with Shift, around the end, and past every region that has
    /// nothing to focus at the moment.
    /// </summary>
    public static class FocusRegionOrder
    {
        /// <summary>
        /// The region F6 moves the focus to, or null when no other region can take it.
        /// </summary>
        /// <param name="current">The region that has the focus, null for none</param>
        /// <param name="canTakeFocus">Per region, in order, whether it has something to focus</param>
        /// <param name="backwards">True for Shift+F6</param>
        public static int? Next(int? current, IReadOnlyList<bool> canTakeFocus, bool backwards)
        {
            int count = canTakeFocus.Count;
            int step = backwards ? count - 1 : 1;

            // From no region, the first step lands on the first region or on the last one backwards.
            int position = current ?? (backwards ? 0 : count - 1);

            for (int visited = 0; visited < count; visited++)
            {
                position = (position + step) % count;

                if (position == current)
                {
                    return null;
                }

                if (canTakeFocus[position])
                {
                    return position;
                }
            }

            return null;
        }
    }
}
