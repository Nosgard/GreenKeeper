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
                    OnPropertyChanged(nameof(CanProceed));
                    OnPropertyChanged(nameof(CharactersRemaining));
                    OnPropertyChanged(nameof(CharactersRemainingText));
                }
            }
        }

        public int CharactersRemaining => Plant.MaxNameLength - PlantName.Length;

        public string CharactersRemainingText => NameLengthCounter.TextFor(CharactersRemaining);

        public bool CanProceed => Plant.IsValidName(PlantName);

        public string NextButtonLabel => "Next";
    }
}
