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
        private static readonly Dictionary<SunlightPeriod, int> MaxHoursByPeriod = new()
        {
            { SunlightPeriod.Day, 24 },
            { SunlightPeriod.Week, 168 },       // 7 * 24
            { SunlightPeriod.Month, 744 },      // 31 * 24
            { SunlightPeriod.Year, 8760 },      // 365 * 24
        };

        public static int MaxAmountLength { get; } = AmountLimit.FieldLengthFor(MaxHoursByPeriod.Values);

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
                    OnPropertyChanged(nameof(IsAmountTooLarge));
                    OnPropertyChanged(nameof(AmountHint));
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
                    OnPropertyChanged(nameof(IsAmountTooLarge));
                    OnPropertyChanged(nameof(AmountHint));
                    OnHoursOrPeriodChanged();
                }
            }
        }

        public int MaxAmount => MaxHoursByPeriod[SelectedPeriod];

        public bool IsAmountTooLarge => Hours > MaxAmount;

        public string AmountHint => AmountLimit.HintFor(MaxAmount, IsAmountTooLarge);

        public int? Hours => int.TryParse(AmountText, out int hours) ? hours : null;

        public bool HasValidAmount => Hours is > 0 && Hours <= MaxAmount;

        // Only meaningful while HasValidAmount is true.
        public SunlightRequirement ToSunlightRequirement()
        {
            return new SunlightRequirement { Hours = Hours!.Value, Period = SelectedPeriod };
        }

        /// <summary>
        /// Called after AmountText or SelectedPeriod changed, for subclasses whose properties
        /// depend on them (CanProceed, NextButtonLabel).
        /// </summary>
        protected virtual void OnHoursOrPeriodChanged() { }
    }
}
