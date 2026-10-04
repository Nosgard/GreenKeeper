using System.Globalization;

namespace GreenKeeper.ViewModels.Base
{
    /// <summary>
    /// The limit of an amount field, the same for the interval input
    /// (AmountAndUnitInputViewModel) and the sunlight input (HoursPerPeriodInputViewModel):
    /// how many digits the field takes.
    /// </summary>
    public static class AmountLimit
    {
        /// <summary>
        /// How many characters an amount field accepts: as many digits as the longest
        /// maximum has. Every valid amount can be typed, and nothing longer fits in.
        /// </summary>
        public static int FieldLengthFor(IEnumerable<int> maximums)
        {
            return maximums.Max().ToString(CultureInfo.InvariantCulture).Length;
        }
    }
}
