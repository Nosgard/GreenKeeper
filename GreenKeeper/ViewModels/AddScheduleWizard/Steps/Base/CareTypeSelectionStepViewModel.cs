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
    /// Choose, which CareType you want to add.
    /// Depending on what you chose, the next step will be made ready.
    /// 
    /// Watering/Fertilizing: Open the related active step (similar to the step in the AddPlantWizard)
    /// 
    /// Sunlight: Open the passive step for the sunlight requirement
    /// </summary>
    public class CareTypeSelectionStepViewModel : IWizardStepViewModel
    {
        private CareType _selectedCareType = CareType.Water;
        public CareType SelectedCareType
        {
            get => _selectedCareType;
            set
            {
                if (_selectedCareType == value)
                {
                    return;
                }
                _selectedCareType = value;
                OnPropertyChanged(nameof(SelectedCareType));
            }
        }

        public IReadOnlyList<KeyValuePair<CareType, string>> AvailableCareTypes { get; } =
            new List<KeyValuePair<CareType, string>>
            {
                new(CareType.Water, "Watering"),
                new(CareType.Nutrients, "Fertilizing"),
                new(CareType.Sunlight, "Sunlight"),
            };

        // Always a valid standard selection means that you can always proceed
        public bool CanProceed => true;

        public string NextButtonLabel => "Next";

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
