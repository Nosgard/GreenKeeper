using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.AddPlantWizard.Steps
{
    public class PlantNameStepViewModel : IWizardStepViewModel
    {
        private const int MAXNAMELENGTH = 50;

        private string _plantName = string.Empty;

        public string PlantName {
            get => _plantName;
            set
            {
                if (_plantName == value)
                {
                    return;
                }
                _plantName = value;
                OnPropertyChanged(nameof(PlantName));

                // CanProceed depends on PlantName.
                // Notify so that the Next-Button can be activated/deactivated
                OnPropertyChanged(nameof(CanProceed));

                // Update the UI on every change so that a Live-Counter
                // for the remaining characters can follow along
                OnPropertyChanged(nameof(CharactersRemaining));


            }
        }

        // Helper that is used to show the remaining characters in the UI
        public int CharactersRemaining => MAXNAMELENGTH - PlantName.Length;


        // Mandatory: Only active, when the name is not empty and is underneath
        // the maximum length
        public bool CanProceed => !string.IsNullOrWhiteSpace(PlantName) && PlantName.Length <= MAXNAMELENGTH;

        // Because the step is mandatory, the button always shows "Next"
        public string NextButtonLabel => "Next";

        // Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
