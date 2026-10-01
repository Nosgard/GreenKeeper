using GreenKeeper.Models;
using GreenKeeper.ViewModels.Base;
using GreenKeeper.ViewModels.Wizards.Base;

namespace GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps
{
    public class PlantNameStepViewModel : ObservableObject, IWizardStepViewModel
    {
        private string _plantName = string.Empty;

        public string PlantName
        {
            get => _plantName;
            set
            {
                if (SetProperty(ref _plantName, value))
                {
                    // The Next button and the live counter for the remaining
                    // characters both depend on the name.
                    OnPropertyChanged(nameof(CanProceed));
                    OnPropertyChanged(nameof(CharactersRemaining));
                }
            }
        }

        // Helper that is used to show the remaining characters in the UI.
        public int CharactersRemaining => Plant.MaxNameLength - PlantName.Length;

        // Mandatory: Only active when the name is not empty and is within
        // the maximum length.
        public bool CanProceed => Plant.IsValidName(PlantName);

        // Because the step is mandatory, the button always shows "Next".
        public string NextButtonLabel => "Next";
    }
}
