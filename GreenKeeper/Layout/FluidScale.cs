using System.Windows;

namespace GreenKeeper.Layout
{
    /// <summary>
    /// The factor a window scales its UI by, 1 at the design size. GrowRate and
    /// ShrinkRate set how much of a size difference goes into the UI; the rest goes
    /// into the room around it. A smaller window has its own rate because the room
    /// runs out first there.
    /// </summary>
    public class FluidScale
    {
        // Both limits start at 1, so a scale without limits draws the UI as designed.
        public double Minimum { get; set; } = 1;

        public double Maximum { get; set; } = 1;

        public double GrowRate { get; set; }

        public double ShrinkRate { get; set; }

        public double FactorFor(Size designSize, Size windowSize)
        {
            // The tighter side decides, so the scaled UI still fits the other one.
            double fit = Math.Min(windowSize.Width / designSize.Width, windowSize.Height / designSize.Height);
            double rate = fit < 1 ? ShrinkRate : GrowRate;

            return Math.Clamp(1 + ((fit - 1) * rate), Minimum, Maximum);
        }
    }
}
