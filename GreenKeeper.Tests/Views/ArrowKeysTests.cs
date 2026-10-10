using GreenKeeper.Views;
using System.Windows.Input;

namespace GreenKeeper.Tests.Views
{
    /// <summary>
    /// What the arrow keys do inside a region: they step through its elements in a
    /// circle, except where an element needs an arrow key for itself.
    /// </summary>
    public class ArrowKeysTests
    {
        // The steps a key can take: one element back, one on, or the key stays with the element.
        private const int Back = -1, On = 1, Stays = 0;

        [Theory]
        [InlineData(Key.Up, Back)]
        [InlineData(Key.Left, Back)]
        [InlineData(Key.Down, On)]
        [InlineData(Key.Right, On)]
        public void StepFor_GivenAButton_StepsWithEveryArrowKey(Key key, int expected)
        {
            // Given: the focus on a button, which has no use for the arrow keys itself
            var kind = FocusStopKind.Button;

            // When: an arrow key is pressed
            int step = ArrowKeys.StepFor(key, kind);

            // Then: up and left go one element back, down and right one on
            Assert.Equal(expected, step);
        }

        [Theory]
        [InlineData(Key.Left, Stays)]
        [InlineData(Key.Right, Stays)]
        [InlineData(Key.Up, Back)]
        [InlineData(Key.Down, On)]
        public void StepFor_GivenATextLine_LeavesLeftAndRightToTheCursor(Key key, int expected)
        {
            // Given: the focus in a text field of one line
            var kind = FocusStopKind.TextLine;

            // When: an arrow key is pressed
            int step = ArrowKeys.StepFor(key, kind);

            // Then: left and right move the cursor in the text, up and down leave the field
            Assert.Equal(expected, step);
        }

        [Theory]
        [InlineData(Key.Up, Back)]
        [InlineData(Key.Left, Back)]
        [InlineData(Key.Down, On)]
        [InlineData(Key.Right, On)]
        public void StepFor_GivenAClosedSelection_StepsWithoutChangingItsValue(Key key, int expected)
        {
            // Given: the focus on a closed drop-down
            var kind = FocusStopKind.Selection;

            // When: an arrow key is pressed
            int step = ArrowKeys.StepFor(key, kind);

            // Then: every arrow key steps to a neighbor. Up and down would otherwise change
            // the value of the drop-down while the user walks through the region
            Assert.Equal(expected, step);
        }

        [Theory]
        [InlineData(Key.Up)]
        [InlineData(Key.Down)]
        [InlineData(Key.Left)]
        [InlineData(Key.Right)]
        public void StepFor_GivenAnElementThatNeedsAllArrowKeys_NeverSteps(Key key)
        {
            // Given: the focus in the notes, a text of several lines, or in an open drop-down
            var kind = FocusStopKind.KeepsAllArrows;

            // When: an arrow key is pressed
            int step = ArrowKeys.StepFor(key, kind);

            // Then: the key stays with the element; Tab leaves it
            Assert.Equal(Stays, step);
        }

        [Theory]
        [InlineData(Key.Up, 1, 3, Stays)]      // in the middle: the list moves its selection
        [InlineData(Key.Down, 1, 3, Stays)]
        [InlineData(Key.Up, 0, 3, Back)]       // on the first entry: up leaves the list
        [InlineData(Key.Down, 0, 3, Stays)]
        [InlineData(Key.Down, 2, 3, On)]       // on the last entry: down leaves the list
        [InlineData(Key.Up, 2, 3, Stays)]
        [InlineData(Key.Up, 0, 1, Back)]       // a single entry is first and last at once
        [InlineData(Key.Down, 0, 1, On)]
        [InlineData(Key.Left, 1, 3, Back)]     // left and right always leave the list
        [InlineData(Key.Right, 1, 3, On)]
        public void StepForEntry_GivenAListEntry_LeavesTheListOnlyAtItsEnds(Key key, int entryIndex, int entryCount, int expected)
        {
            // Given: the focus on an entry of the plant list

            // When: an arrow key is pressed
            int step = ArrowKeys.StepForEntry(key, entryIndex, entryCount);

            // Then: up and down belong to the list until its end. There they step on to the
            // neighbor of the list, or the search box above it could not be reached
            Assert.Equal(expected, step);
        }

        [Theory]
        [InlineData(0, On, 0)]      // the window itself has the focus: on to the first element
        [InlineData(0, Back, 3)]    // and back, around the end, to the last one
        [InlineData(2, On, 2)]      // between the second and the third element: on to the third
        [InlineData(2, Back, 1)]    // and back to the second
        [InlineData(4, On, 0)]      // behind the last element: around to the first
        public void NeighborOfGap_GivenTheFocusBetweenTheElements_IsTheElementOnThatSide(int elementsBefore, int step, int expected)
        {
            // Given: a region of four elements, and the focus on something the walk does not
            // stop at: the window right after the start, or a list that lost its entries
            int count = 4;

            // When: a step is taken from there
            int target = ArrowKeys.NeighborOfGap(elementsBefore, step, count);

            // Then: the walk continues with the element next to that place
            Assert.Equal(expected, target);
        }

        [Theory]
        [InlineData(1, On, 2)]
        [InlineData(1, Back, 0)]
        [InlineData(3, On, 0)]      // on from the last element: around to the first
        [InlineData(0, Back, 3)]    // back from the first element: around to the last
        public void NeighborOf_GivenAStep_WalksInACircle(int index, int step, int expected)
        {
            // Given: a region of four elements
            int count = 4;

            // When: a step is taken from one of them
            int target = ArrowKeys.NeighborOf(index, step, count);

            // Then: the walk goes around the end, so every element is reached in both directions
            Assert.Equal(expected, target);
        }
    }
}
