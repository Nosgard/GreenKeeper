using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.Base;
using GreenKeeper.ViewModels.Wizards.Base;

namespace GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps
{
    /// <summary>
    /// The first step of the Add Schedule wizard: the care type to add.
    /// AddScheduleWizardViewModel picks the next step from it.
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
