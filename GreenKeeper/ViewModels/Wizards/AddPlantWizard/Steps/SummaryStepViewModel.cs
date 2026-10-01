using GreenKeeper.ViewModels.Base;
using GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps.Active;
using GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps.Passive;
using GreenKeeper.ViewModels.Wizards.Base;
using GreenKeeper.ViewModels.Wizards.Base.Abstract;
using System.ComponentModel;

namespace GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps
{
    /// <summary>
    /// The last step, which repeats what was entered in the steps before. Its
    /// properties have no backing fields but read the other steps directly, and
    /// it listens to them, so going back and changing a value shows up here
    /// without anyone having to refresh the summary by hand.
    /// </summary>
    public class SummaryStepViewModel : ObservableObject, IWizardStepViewModel
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

            nameStepViewModel.PropertyChanged += (_, _) => OnPropertyChanged(nameof(PlantName));
            wateringStepViewModel.PropertyChanged += (_, _) => OnPropertyChanged(nameof(Watering));
            fertilizingStepViewModel.PropertyChanged += OnFertilizingChanged;
            sunlightStepViewModel.PropertyChanged += OnSunlightChanged;
        }

        public string PlantName => _nameStepViewModel.PlantName;

        // Watering is mandatory - it always has a value to show.
        public string Watering =>
            $"{_wateringStepViewModel.AmountText} {GetUnit(_wateringStepViewModel)}";

        // Fertilizing is optional - only show a value if a value was entered (not skipped).
        public bool HasFertilizing => _fertilizingStepViewModel.HasValidAmount;
        public string Fertilizing =>
            $"{_fertilizingStepViewModel.AmountText} {GetUnit(_fertilizingStepViewModel)}";

        // Sunlight is optional - the same principle as Fertilizing but with a value per period.
        public bool HasSunlight => _sunlightStepViewModel.HasValidAmount;
        public string Sunlight =>
            $"{_sunlightStepViewModel.AmountText} Hours {GetPeriod(_sunlightStepViewModel)}";

        // The summary step is the last step, so proceeding is always possible.
        public bool CanProceed => true;

        // Last step -> always "Finish".
        public string NextButtonLabel => "Finish";

        // Resolves the text of the selected TimeUnit via AvailableUnits (e.g. TimeUnit.Days -> "Days").
        private static string GetUnit(ActiveStepViewModel step) =>
            AmountAndUnitInputViewModel.AvailableUnits.First(u => u.Key == step.SelectedUnit).Value;

        private static string GetPeriod(SunlightStepViewModel step) =>
            HoursPerPeriodInputViewModel.AvailablePeriods.First(p => p.Key == step.SelectedPeriod).Value;

        private void OnFertilizingChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasFertilizing));
            OnPropertyChanged(nameof(Fertilizing));
        }

        private void OnSunlightChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasSunlight));
            OnPropertyChanged(nameof(Sunlight));
        }
    }
}
