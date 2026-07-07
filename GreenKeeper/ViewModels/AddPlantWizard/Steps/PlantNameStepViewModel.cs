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
            }
        }

        // Mandatory: Only active, when there is no empty name
        public bool CanProceed => !string.IsNullOrWhiteSpace(PlantName);

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
