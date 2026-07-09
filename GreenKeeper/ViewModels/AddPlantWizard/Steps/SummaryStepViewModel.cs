using GreenKeeper.ViewModels.AddPlantWizard.Steps.Active;
using GreenKeeper.ViewModels.AddPlantWizard.Steps.Passive;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenKeeper.ViewModels.AddPlantWizard.Steps
{
    public class SummaryStepViewModel : IWizardStepViewModel
    {
        private readonly PlantNameStepViewModel _nameStepViewModel;
        private readonly WateringStepViewModel _wateringStepViewModel;
        private readonly FertilizingStepViewModel _fertilizingStepViewModel;
        private readonly SunlightStepViewModel _sunlightStepViewModel;

        public SummaryStepViewModel(PlantNameStepViewModel nameStepViewModel,
            WateringStepViewModel wateringStepViewModel,
            FertilizingStepViewModel fertilizingStepViewModel,
            SunlightStepViewModel sunlightStepViewModel)
        {
            _nameStepViewModel = nameStepViewModel;
            _wateringStepViewModel = wateringStepViewModel;
            _fertilizingStepViewModel = fertilizingStepViewModel;
            _sunlightStepViewModel = sunlightStepViewModel;
        }

        public string PlantName => _nameStepViewModel.PlantName;

        // Watering is mandatory - it always has a value to show
        public string Watering =>
            $"{_wateringStepViewModel.AmountText} {GetUnit(_wateringStepViewModel)}";

        // Fertilizing is optional - only show a value if a value was entered (not skipped)
        public bool HasFertilizing => !string.IsNullOrEmpty(_fertilizingStepViewModel.AmountText);
        public string Fertilizing =>
            $"{_fertilizingStepViewModel.AmountText} {GetUnit(_fertilizingStepViewModel)}";

        // Sunlight is optional - the same principle as Fertilizing but with a value by period
        public bool HasSunlight => !string.IsNullOrEmpty(_sunlightStepViewModel.AmountText);
        public string Sunlight =>
            $"{_sunlightStepViewModel.AmountText} Hours {GetPeriod(_sunlightStepViewModel)}";

        // Dissolves the text of the selected TimeUnit via AvailableUnits (e.g. TimeUnit.Days -> "Days")
        private static string GetUnit(ActiveStepViewModel step) =>
            step.AvailableUnits.First(u => u.Key == step.SelectedUnit).Value;

        private static string GetPeriod(SunlightStepViewModel step) =>
            step.AvailablePeriods.First(p => p.Key == step.SelectedPeriod).Value;

        // The summary step is the last step, so proceeding is always possible
        public bool CanProceed => true;

        // Last step -> always "Finish"
        public string NextButtonLabel => "Finish";

        /// <summary>
        /// Will be called by the Wizard, once this step (the summary step) is called.
        /// This is necessary because all values of Watering/Fertilizing/Sunlight can get
        /// changed in the meantime (e.g. by using the Back-Button + entering a new value).
        /// The Summary-Properties don't have their own Backing-Field. Instead they actively
        /// read from the other steps
        /// </summary>
        public void Refresh()
        {
            OnPropertyChanged(nameof(PlantName));
            OnPropertyChanged(nameof(Watering));
            OnPropertyChanged(nameof(HasFertilizing));
            OnPropertyChanged(nameof(Fertilizing));
            OnPropertyChanged(nameof(HasSunlight));
            OnPropertyChanged(nameof(Sunlight));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
