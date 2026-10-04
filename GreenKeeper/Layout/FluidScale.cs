using System.Windows;

namespace GreenKeeper.Layout
{
    /// <summary>
    /// The scale a window draws its UI at. At the design size the factor is 1. Any
    /// other size is followed at a rate: at 0.5, half of the difference goes into the
    /// size of the UI and half into the room around it. A smaller window has its own
    /// rate, because there the room runs out first. The factor stays between Minimum
    /// and Maximum.
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
