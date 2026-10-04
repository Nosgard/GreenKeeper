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
        /// How many digits an amount field accepts: one more than the longest maximum. The
        /// content stays fully visible, and an amount above the limit still shows as too
        /// large instead of being cut down to a valid one.
        /// </summary>
        public static int FieldLengthFor(IEnumerable<int> maximums)
        {
            return maximums.Max().ToString(CultureInfo.InvariantCulture).Length + 1;
        }
    }
}
