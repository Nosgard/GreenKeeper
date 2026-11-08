using GreenKeeper.Models;
using GreenKeeper.Models.Enums;

namespace GreenKeeper.ViewModels.Base
{
    /// <summary>
    /// The "amount + time unit" input shared by every place where the interval of a care
    /// schedule is entered: the wizard steps and the edit dialog.
    /// </summary>
    public class AmountAndUnitInputViewModel : ObservableObject
    {
        /// <summary>
        /// The largest amount each unit accepts, for the watering and fertilizing inputs.
        /// </summary>
        private static readonly Dictionary<TimeUnit, int> MaxAmountsByUnit = new()
        {
            { TimeUnit.Days, 365 },
            { TimeUnit.Weeks, 52 },
            { TimeUnit.Months, 24 },
            { TimeUnit.Years, 10 },
        };

        public static int MaxAmountLength { get; } = AmountLimit.FieldLengthFor(MaxAmountsByUnit.Values);

        public static IReadOnlyList<KeyValuePair<TimeUnit, string>> AvailableUnits { get; } =
            new List<KeyValuePair<TimeUnit, string>>
            {
                new(TimeUnit.Days, "Days"),
                new(TimeUnit.Weeks, "Weeks"),
                new(TimeUnit.Months, "Months"),
                new(TimeUnit.Years, "Years"),
            };

        private string _amountText = string.Empty;
        private TimeUnit _selectedUnit = TimeUnit.Days;

        public string AmountText
        {
            get => _amountText;
            set
            {
                if (SetProperty(ref _amountText, value))
                {
                    OnPropertyChanged(nameof(IsAmountTooLarge));
                    OnPropertyChanged(nameof(AmountHint));
                    OnAmountOrUnitChanged();
                }
            }
        }

        public TimeUnit SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                if (SetProperty(ref _selectedUnit, value))
                {
                    OnPropertyChanged(nameof(MaxAmount));
                    OnPropertyChanged(nameof(IsAmountTooLarge));
                    OnPropertyChanged(nameof(AmountHint));
                    OnAmountOrUnitChanged();
                }
            }
        }

        public int MaxAmount => MaxAmountsByUnit[SelectedUnit];

        public bool IsAmountTooLarge => Amount > MaxAmount;

        public string AmountHint => AmountLimit.HintFor(MaxAmount, IsAmountTooLarge);

        public int? Amount => int.TryParse(AmountText, out int amount) ? amount : null;

        /// <summary>
        /// A positive amount within the maximum of the selected unit. Whether the field may stay
        /// empty is up to the wizard step or the edit dialog.
        /// </summary>
        public bool HasValidAmount => Amount is > 0 && Amount <= MaxAmount;

        // Only meaningful while HasValidAmount is true.
        public CareSchedule ToCareSchedule(CareType care)
        {
            return new CareSchedule { Care = care, IntervalAmount = Amount, IntervalUnit = SelectedUnit };
        }

        /// <summary>
        /// Called after AmountText or SelectedUnit changed, for subclasses whose properties
        /// depend on them (CanProceed, NextButtonLabel, PreviewText).
        /// </summary>
        protected virtual void OnAmountOrUnitChanged() { }
    }
}
