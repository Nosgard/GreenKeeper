using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.AddPlantWizard.Steps.Active
{
    public abstract class ActiveStepViewModel
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
        /// The amount needs to have a positive number.
        /// In the context of watering it has the same meaning as CanProceed (mandatory).
        /// Fertilizing is optional is used as a help value (for more info go to NextButtonLabel)
        /// </summary>
        protected bool HasValidAmount => int.TryParse(AmountText, out int amount) && amount > 0;

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
