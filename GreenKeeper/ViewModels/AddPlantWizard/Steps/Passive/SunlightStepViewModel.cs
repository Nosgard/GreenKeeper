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
                OnPropertyChanged(nameof(NextButtonLabel));
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
                OnPropertyChanged(nameof(NextButtonLabel));
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

        /// <summary>
        /// The amount needs to have a positive number.
        /// Sunlight is a special case because it's passive.
        /// You enter a positive amount of hours by period
        /// that is underneath the maximum
        /// </summary>
        public bool HasValidAmount =>
            int.TryParse(AmountText, out int hours) && hours >= 1 && hours <= MaxAmount;

        // Basically the same as for the active steps (watering and fertilizing).
        // The only difference is that you enter a positive amount of hours by period (as mentioned above) or keep it empty
        public bool CanProceed => true;

        // Depending on the entered amount of hours by period show "Next" or "Skip"
        public string NextButtonLabel => HasValidAmount ? "Next" : "Skip";

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
