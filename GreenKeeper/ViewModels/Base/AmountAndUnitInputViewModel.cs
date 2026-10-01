using GreenKeeper.Models;
using GreenKeeper.Models.Enums;

namespace GreenKeeper.ViewModels.Base
{
    /// <summary>
    /// The "amount + time unit" input shared by every place where a care
    /// interval is entered: the wizard steps and the edit dialog.
    /// </summary>
    public class AmountAndUnitInputViewModel : ObservableObject
    {
        /// <summary>
        /// Every available unit has a maximum amount to prevent misuse.
        /// The limits for all active steps (for watering and fertilizing)
        /// can be set from here.
        /// </summary>
        private static readonly Dictionary<TimeUnit, int> MaxAmountsByUnit = new()
        {
            { TimeUnit.Days, 365 },
            { TimeUnit.Weeks, 52 },
            { TimeUnit.Months, 24 },
            { TimeUnit.Years, 10 },
        };

        /// <summary>
        /// Options for the ComboBox.
        ///
        /// Key: Actual enum value (will be bound)
        /// Value: Text that the user sees.
        /// </summary>
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
                    OnAmountOrUnitChanged();
                }
            }
        }

        public int MaxAmount => MaxAmountsByUnit[SelectedUnit];

        // The entered text as a number, or null while it is not a number at all.
        public int? Amount => int.TryParse(AmountText, out int amount) ? amount : null;

        /// <summary>
        /// The amount needs to be a positive number and stay below the maximum of the selected unit.
        /// This is a pure data-validity check - it says nothing about whether the value is mandatory or optional
        /// for the surrounding context (wizard step or edit dialog); that decision is left to the classes that
        /// use this property.
        /// </summary>
        public bool HasValidAmount => Amount is > 0 && Amount <= MaxAmount;

        // The entered interval as a schedule of the given care type. Only meaningful while HasValidAmount is true.
        public CareSchedule ToCareSchedule(CareType care)
        {
            return new CareSchedule { Care = care, IntervalAmount = Amount, IntervalUnit = SelectedUnit };
        }

        /// <summary>
        /// Hook for subclasses that need to react whenever AmountText or SelectedUnit changes
        /// (e.g. to re-raise OnPropertyChanged for derived properties like CanProceed/NextButtonLabel
        /// in ActiveStepViewModel, or PreviewText in EditActiveScheduleViewModel).
        /// Default implementation remains empty.
        /// </summary>
        protected virtual void OnAmountOrUnitChanged() { }
    }
}
