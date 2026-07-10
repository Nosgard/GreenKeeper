using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.AddPlantWizard.Steps.Active
{
    public abstract class ActiveStepViewModel : IWizardStepViewModel
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
                OnPropertyChanged(nameof(CanProceed));
                OnPropertyChanged(nameof(NextButtonLabel));
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

                // An entered amount can be invalid with the new unit (e.g 300 for days is valid but not for years).
                // Therefore CanProceed and NextButtonLabel are indespensable when changing the unit
                OnPropertyChanged(nameof(CanProceed));
                OnPropertyChanged(nameof(NextButtonLabel));
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
                new(TimeUnit.Hours, "Hours"),
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
            { TimeUnit.Hours, 168 },    // 168 Hours = 1 Week
            { TimeUnit.Days, 365 },     // 365 Days = 1 Year
            { TimeUnit.Weeks, 52 },     // 52 Weeks = 1 Year
            { TimeUnit.Months, 24 },    // 24 Months = 2 Years
            { TimeUnit.Years, 10 },
        };

        // Set the limit for the selected unit
        public int MaxAmount => MaxAmountsByUnit[SelectedUnit];

        /// <summary>
        /// The amount needs to have a positive number.
        /// In the context of watering it has the same meaning as CanProceed (mandatory).
        /// Fertilizing is optional is used as a help value (for more info go to NextButtonLabel)
        /// 
        /// Important!
        /// The amount is only valid, when it's positive (as mentioned before) and
        /// is underneath the maximum of the selected unit (for more go to MaxAmountsByUnit)
        /// </summary>
        public bool HasValidAmount => int.TryParse(AmountText, out int amount) && amount > 0 && amount <= MaxAmount;

        // Mandatory for watering + optional for fertilizing.
        // Will be implemented by the affected class
        public abstract bool CanProceed { get; }

        // Standard: "Next"
        // In the context of fertilizing: "Skip" or "Next"
        public virtual string NextButtonLabel => "Next";


        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
