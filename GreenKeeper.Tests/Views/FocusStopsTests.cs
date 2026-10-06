using GreenKeeper.Views;

namespace GreenKeeper.Tests.Views
{
    /// <summary>
    /// Which entry takes the focus when a list as a whole gets it: the entry at the
    /// edge an arrow key step comes in from, or the chosen one when the focus arrives
    /// without a step.
    /// </summary>
    public class FocusStopsTests
    {
        // A list of five entries, the third one chosen.
        private const int Count = 5, Chosen = 2, Nothing = -1;

        [Theory]
        [InlineData(EntryChoice.First, 0)]
        [InlineData(EntryChoice.Last, 4)]
        public void EntryToFocus_GivenAStepIntoTheList_TakesTheEntryAtThatEdge(EntryChoice edge, int expected)
        {
            // Given: a list whose third entry is chosen

            // When: a step of the arrow keys enters it at one of its edges
            int entry = FocusStops.EntryToFocus(edge, Chosen, Count);

            // Then: the focus lands on the entry at that edge. Walking down through the
            // sidebar must not jump over the entries above the chosen one
            Assert.Equal(expected, entry);
        }

        [Fact]
        public void EntryToFocus_GivenTheListIsEnteredWithoutAStep_TakesTheChosenEntry()
        {
            // Given: a list whose third entry is chosen

            // When: the focus arrives without a direction, as with F6 into the sidebar
            int entry = FocusStops.EntryToFocus(EntryChoice.Chosen, Chosen, Count);

            // Then: it lands on the chosen entry, so the user is where they were
            Assert.Equal(Chosen, entry);
        }

        [Fact]
        public void EntryToFocus_GivenNothingIsChosen_TakesTheFirstEntry()
        {
            // Given: a list in which no entry is chosen

            // When: the focus arrives without a direction
            int entry = FocusStops.EntryToFocus(EntryChoice.Chosen, Nothing, Count);

            // Then: the list starts at its first entry
            Assert.Equal(0, entry);
        }
    }
}
