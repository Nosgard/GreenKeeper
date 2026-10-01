using GreenKeeper.Models;
using GreenKeeper.Models.Enums;

namespace GreenKeeper.ViewModels.Base
{
    /// <summary>
    /// The "hours + period" input shared by every place where a sunlight
    /// requirement is entered: both wizards and the edit dialog. The passive
    /// counterpart of AmountAndUnitInputViewModel.
    /// </summary>
    public class HoursPerPeriodInputViewModel : ObservableObject
    {
        /// <summary>
        /// Every available period has a maximum amount to prevent misuse.
        /// The limits are declared in hours per period and can be set from here.
        /// </summary>
        private static readonly Dictionary<SunlightPeriod, int> MaxHoursByPeriod = new()
        {
            { SunlightPeriod.Day, 24 },
            { SunlightPeriod.Week, 168 },       // 7 * 24
            { SunlightPeriod.Month, 744 },      // 31 * 24
            { SunlightPeriod.Year, 8760 },      // 365 * 24
        };

        /// <summary>
        /// Options for the ComboBox.
        ///
        /// Key: Actual enum value (will be bound)
        /// Value: Text that the user sees.
        /// </summary>
        public static IReadOnlyList<KeyValuePair<SunlightPeriod, string>> AvailablePeriods { get; } =
            new List<KeyValuePair<SunlightPeriod, string>>
            {
                new(SunlightPeriod.Day, "/ Day"),
                new(SunlightPeriod.Week, "/ Week"),
                new(SunlightPeriod.Month, "/ Month"),
                new(SunlightPeriod.Year, "/ Year"),
            };

        private string _amountText = string.Empty;
        private SunlightPeriod _selectedPeriod = SunlightPeriod.Day;

        public string AmountText
        {
            get => _amountText;
            set
            {
                if (SetProperty(ref _amountText, value))
                {
                    OnHoursOrPeriodChanged();
                }
            }
        }

        public SunlightPeriod SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                if (SetProperty(ref _selectedPeriod, value))
                {
                    // An entered amount can be invalid with the new period
                    // (e.g. 100 is valid for weeks but not for days).
                    OnPropertyChanged(nameof(MaxAmount));
                    OnHoursOrPeriodChanged();
                }
            }
        }

        public int MaxAmount => MaxHoursByPeriod[SelectedPeriod];

        // The entered text as a number, or null while it is not a number at all.
        public int? Hours => int.TryParse(AmountText, out int hours) ? hours : null;

        // A positive number of hours that fits into the selected period.
        public bool HasValidAmount => Hours is > 0 && Hours <= MaxAmount;

        // The entered hours as a requirement. Only meaningful while HasValidAmount is true.
        public SunlightRequirement ToSunlightRequirement()
        {
            return new SunlightRequirement { Hours = Hours!.Value, Period = SelectedPeriod };
        }

        /// <summary>
        /// Hook for subclasses that need to react whenever AmountText or SelectedPeriod
        /// changes (e.g. to re-raise OnPropertyChanged for CanProceed/NextButtonLabel).
        /// Default implementation remains empty.
        /// </summary>
        protected virtual void OnHoursOrPeriodChanged() { }
    }
}
