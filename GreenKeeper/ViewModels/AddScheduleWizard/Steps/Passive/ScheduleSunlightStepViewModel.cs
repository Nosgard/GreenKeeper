using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.AddPlantWizard.Steps;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.AddScheduleWizard
{
    /// <summary>
    /// Passive step just like in the AddPlantWizard, but mandatory
    /// and with an explicit "Finish" on the Next-Button, because the user
    /// selected the status on purpose
    /// </summary>
    public class ScheduleSunlightStepViewModel : IWizardStepViewModel
    {

        private static readonly Dictionary<SunlightPeriod, int> MAxHoursByPeriod = new()
        {
            { SunlightPeriod.Day, 24 },
            { SunlightPeriod.Week, 168 },
            { SunlightPeriod.Month, 744 },
            { SunlightPeriod.Year, 87760 },
        };

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
                OnPropertyChanged(nameof(CanProceed));
            }
        }

        public IReadOnlyList<KeyValuePair<SunlightPeriod, string>> AvailablePeriods { get; } =
            new List<KeyValuePair<SunlightPeriod, string>>
            {
                new(SunlightPeriod.Day, "/ Day"),
                new(SunlightPeriod.Week, "/ Week"),
                new(SunlightPeriod.Month, "/ Month"),
                new(SunlightPeriod.Year, "/ Year"),
            };

        public int MaxAmount => MAxHoursByPeriod[SelectedPeriod];

        // Different from SunlightStepViewModel: Mandatory!
        public bool CanProceed =>
            int.TryParse(AmountText, out int hours) && hours >= 1 && hours <= MaxAmount;

        public string NextButtonLabel => "Finish";

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
