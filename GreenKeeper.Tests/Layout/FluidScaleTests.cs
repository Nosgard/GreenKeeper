using GreenKeeper.Layout;
using System.Windows;

namespace GreenKeeper.Tests.Layout
{
    /// <summary>
    /// The scale the UI is drawn at: 1 at the design size, following larger and
    /// smaller windows at their own rates, never beyond the limits.
    /// </summary>
    public class FluidScaleTests
    {
        private static readonly Size DesignSize = new(900, 650);

        [Fact]
        public void FactorFor_GivenTheDesignSize_DrawsTheUiAsDesigned()
        {
            // Given: a scale that follows larger and smaller windows
            var scale = new FluidScale { Minimum = 0.5, Maximum = 2, GrowRate = 0.5, ShrinkRate = 0.75 };

            // When: the factor for a window of exactly the design size is calculated
            var factor = scale.FactorFor(DesignSize, DesignSize);

            // Then: the UI is drawn at scale 1
            Assert.Equal(1, factor, precision: 10);
        }

        [Fact]
        public void FactorFor_GivenLargerWindow_FollowsTheDifferenceAtTheGrowRate()
        {
            // Given: a scale that follows a larger window at a rate of 0.5
            var scale = new FluidScale { Minimum = 0.5, Maximum = 2, GrowRate = 0.5, ShrinkRate = 0.75 };
            var twiceTheDesignSize = new Size(1800, 1300);

            // When: the factor for a window of twice the design size is calculated
            var factor = scale.FactorFor(DesignSize, twiceTheDesignSize);

            // Then: half of the difference to the design size makes the UI larger
            Assert.Equal(1.5, factor, precision: 10);
        }

        [Fact]
        public void FactorFor_GivenSmallerWindow_FollowsTheDifferenceAtTheShrinkRate()
        {
            // Given: a scale that follows a smaller window at a rate of 0.75
            var scale = new FluidScale { Minimum = 0.5, Maximum = 2, GrowRate = 0.5, ShrinkRate = 0.75 };
            var halfTheDesignSize = new Size(450, 325);

            // When: the factor for a window of half the design size is calculated
            var factor = scale.FactorFor(DesignSize, halfTheDesignSize);

            // Then: three quarters of the missing half make the UI smaller
            Assert.Equal(0.625, factor, precision: 10);
        }

        [Theory]
        [InlineData(1800, 650)]   // twice as wide, but no higher
        [InlineData(900, 1300)]   // twice as high, but no wider
        public void FactorFor_GivenWindowGrownOnOneSide_LetsTheTighterSideDecide(double width, double height)
        {
            // Given: a scale that follows a larger window at a rate of 0.5
            var scale = new FluidScale { Minimum = 0.5, Maximum = 2, GrowRate = 0.5, ShrinkRate = 0.75 };

            // When: the factor for a window that grew on one side only is calculated
            var factor = scale.FactorFor(DesignSize, new Size(width, height));

            // Then: the UI stays as designed, so it still fits the side that did not grow
            Assert.Equal(1, factor, precision: 10);
        }

        [Theory]
        [InlineData(9000, 6500, 1.5)]   // far larger than the design size
        [InlineData(90, 65, 0.78)]      // far smaller than the design size
        public void FactorFor_GivenExtremeWindowSize_StaysWithinTheLimits(double width, double height, double expected)
        {
            // Given: a scale limited to 0.78 at the bottom and 1.5 at the top
            var scale = new FluidScale { Minimum = 0.78, Maximum = 1.5, GrowRate = 0.5, ShrinkRate = 0.75 };

            // When: the factor for the extreme window size is calculated
            var factor = scale.FactorFor(DesignSize, new Size(width, height));

            // Then: the limit is used instead of the calculated factor
            Assert.Equal(expected, factor);
        }
    }
}
