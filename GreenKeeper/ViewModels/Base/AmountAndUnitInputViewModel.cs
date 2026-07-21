using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.Base
{
    public class AmountAndUnitInputViewModel : INotifyPropertyChanged
    {
        private string _amountText = string.Empty;
        public string AmountText
        {
            get => _amountText;
            set
            {
                if (_amountText == value)
                {
                    return;
                }
                _amountText = value;
                OnPropertyChanged(nameof(AmountText));
                OnPropertyChanged(nameof(MaxAmount));
                OnAmountOrUnitChanged();

            }
        }

        private TimeUnit _selectedUnit = TimeUnit.Days;
        public TimeUnit SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                if (_selectedUnit == value)
                {
                    return;
                }
                _selectedUnit = value;
                OnPropertyChanged(nameof(SelectedUnit));
                OnPropertyChanged(nameof(MaxAmount));
                OnAmountOrUnitChanged();
            }
        }

        /// <summary>
        /// Options for the ComboBox
        /// 
        /// Key: Actual Enum-Value (will be bound)
        /// Value: Text that the user sees
        /// </summary>
        public IReadOnlyList<KeyValuePair<TimeUnit, string>> AvailableUnits { get; } =
            new List<KeyValuePair<TimeUnit, string>>
            {
                new(TimeUnit.Days, "Days"),
                new(TimeUnit.Weeks, "Weeks"),
                new(TimeUnit.Months, "Months"),
                new(TimeUnit.Years, "Years"),
            };

        /// <summary>
        /// Every available unit has a maximum amount to prevent misuse.
        /// The limits for all active steps (for watering and fertilizing)
        /// can be set from here
        /// </summary>
        private static readonly Dictionary<TimeUnit, int> MaxAmountsByUnit = new()
        {
            { TimeUnit.Days, 365 },
            { TimeUnit.Weeks, 52 },
            { TimeUnit.Months, 24 },
            { TimeUnit.Years, 10 },
        };

        public int MaxAmount => MaxAmountsByUnit[SelectedUnit];

        /// <summary>
        /// The amount needs to have a positive number and stay underneath the maximum of the selected unit.
        /// This is a pure data-validity check - it says nothing about whether the value is mandatory or optional
        /// for the surrounding context (Wizard-Step or Edit-Dialog), that decision is left to the classes that
        /// use this property
        /// </summary>
        public bool HasValidAmount =>
            int.TryParse(AmountText, out int amount) && amount > 0 && amount <= MaxAmount;

        /// <summary>
        /// Hook for subclasses that need to react whenever AmountText or SelectedUnit changes
        /// (e.g. to re-raise OnPropertyChanged for derived properties like CanProceed/NextButtonLabel
        /// in ActiveStepViewModel, or PreviewText in EditActiveScheduleViewModel).
        /// Default implementation remains empty
        /// </summary>
        protected virtual void OnAmountOrUnitChanged() { }

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
