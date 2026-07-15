using GreenKeeper.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.CareStatuses.EditOption
{
    /// <summary>
    /// Base of the Edit-Button for the Sunlight-Status.
    /// It's standalone instead of inheriting from SunlightStepViewModel, since the semantics
    /// differ here. It's mandatory and pre-filled with the existing Sunlight-Requirement.
    /// No PreviewText needed, since Sunlight has no due date
    /// </summary>
    public class EditSunlightViewModel : INotifyPropertyChanged
    {
        private static readonly Dictionary<SunlightPeriod, int> MaxHoursByPeriod = new()
        {
            { SunlightPeriod.Day, 24 },
            { SunlightPeriod.Week, 168 },
            { SunlightPeriod.Month, 744 },
            { SunlightPeriod.Year, 8760 },
        };

        // Pre-fill the fields with the current Sunlight-Requirement
        public EditSunlightViewModel(int? initialHours, SunlightPeriod initialPeriod)
        {
            if (initialHours.HasValue)
            {
                AmountText = initialHours.Value.ToString();
            }
            SelectedPeriod = initialPeriod;
        }

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
                OnPropertyChanged(nameof(HasValidAmount));
            }
        }

        private SunlightPeriod _selectedPeriod;
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
                OnPropertyChanged(nameof(HasValidAmount));
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

        public int MaxAmount => MaxHoursByPeriod[SelectedPeriod];

        // Unlike SunlightStepViewModel in the Wizard, there is no option to skip.
        // the user is actively editing an existing value, so a valid amount is always required
        public bool HasValidAmount =>
            int.TryParse(AmountText, out int hours) && hours >= 1 && hours <= MaxAmount;

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
