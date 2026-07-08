using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.AddPlantWizard.Steps.Passive
{
    public class SunlightStepViewModel : IWizardStepViewModel
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

        private SunlightPeriod _selectedPeriod = SunlightPeriod.Day;
        public SunlightPeriod SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                if (_selectedPeriod == value)
                {
                    return;
                }
                _selectedPeriod = value;
                OnPropertyChanged(nameof(SelectedPeriod));
                OnPropertyChanged(nameof(MaxAmount));

                // An entered amount can be invalid with the new period (e.g. 100 is valid for weeks but not for days).
                // Therefore CanProceed is indispensable when changing the unit
                OnPropertyChanged(nameof(CanProceed));
            }
        }

        /// <summary>
        /// Options for the ComboBox
        /// 
        /// Key: Actual Enum-Value (will be bound)
        /// Value: Text that the user sees
        /// </summary>
        public IReadOnlyList<KeyValuePair<SunlightPeriod, string>> AvailablePeriods { get; } =
            new List<KeyValuePair<SunlightPeriod, string>>
            {
                new(SunlightPeriod.Day, "/ Day"),
                new(SunlightPeriod.Week, "/ Week"),
                new(SunlightPeriod.Month, "/ Month"),
                new(SunlightPeriod.Year, "/ Year"),
            };

        /// <summary>
        /// Every available unit has a maximum amount to prevent misuse.
        /// The limits are declared in hours by the given period and
        /// can be set from here
        /// </summary>
        private static readonly Dictionary<SunlightPeriod, int> MaxHoursByPeriod = new()
        {
            { SunlightPeriod.Day, 24 },
            { SunlightPeriod.Week, 168 },       // 7 * 24
            { SunlightPeriod.Month, 744 },      // 31 * 24
            { SunlightPeriod.Year, 8760 },      // 365 * 24
        };

        // Set the limit for the selected period
        public int MaxAmount => MaxHoursByPeriod[SelectedPeriod];

        // Special case for sunlighting, which is optional and the last step.
        // The user must enter a valid number of hours or keep it empty to finish
        public bool CanProceed =>
            string.IsNullOrWhiteSpace(AmountText) ||
            (int.TryParse(AmountText, out int hours) && hours >= 1 && hours <= MaxAmount);

        // Last step in the Wizard, so "Finish" in the Next-Button is without exception
        public string NextButtonLabel => "Finish";

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
