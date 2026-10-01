using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.Base;
using GreenKeeper.ViewModels.Wizards.Base;

namespace GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps
{
    /// <summary>
    /// Choose which CareType you want to add.
    /// Depending on what you choose, the next step will be made ready.
    /// 
    /// Watering/Fertilizing: Open the related active step (similar to the step in the AddPlantWizard).
    /// 
    /// Sunlight: Open the passive step for the sunlight requirement.
    /// </summary>
    public class CareTypeSelectionStepViewModel : ObservableObject, IWizardStepViewModel
    {
        private CareType _selectedCareType = CareType.Watering;

        public CareType SelectedCareType
        {
            get => _selectedCareType;
            set => SetProperty(ref _selectedCareType, value);
        }

        // Options for the ComboBox: the enum value to bind and the text the user sees.
        public static IReadOnlyList<KeyValuePair<CareType, string>> AvailableCareTypes { get; } =
            Enum.GetValues<CareType>()
                .Select(careType => new KeyValuePair<CareType, string>(careType, careType.DisplayName()))
                .ToList();

        // There is always a valid default selection, so you can always proceed.
        public bool CanProceed => true;

        public string NextButtonLabel => "Next";
    }
}
