using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.Base;
using GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps.Active;
using GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps.Passive;
using GreenKeeper.ViewModels.Wizards.Base;
using GreenKeeper.ViewModels.Wizards.Base.Abstract;
using System.ComponentModel;

namespace GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps
{
    /// <summary>
    /// The last step, which repeats what was entered in the steps before. It reads the
    /// other steps directly and listens to them, so a value changed after going back
    /// shows up here.
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

        // Watering is mandatory, so it always has a value to show.
        public string Watering =>
            $"{_wateringStepViewModel.AmountText} {GetUnit(_wateringStepViewModel)}";

        // Fertilizing is optional: the summary shows it only if a value was entered.
        public bool HasFertilizing => _fertilizingStepViewModel.HasValidAmount;
        public string Fertilizing =>
            $"{_fertilizingStepViewModel.AmountText} {GetUnit(_fertilizingStepViewModel)}";

        // Sunlight is optional like Fertilizing, with a value per period.
        public bool HasSunlight => _sunlightStepViewModel.HasValidAmount;
        public string Sunlight =>
            $"{_sunlightStepViewModel.AmountText} Hours {GetPeriod(_sunlightStepViewModel)}";

        // The sunlight line the way a screen reader should say it ("6 hours per day").
        public string SpokenSunlight =>
            $"Sunlight: {_sunlightStepViewModel.SelectedPeriod.SpokenHours(_sunlightStepViewModel.Hours ?? 0)}";

        public bool CanProceed => true;

        public string NextButtonLabel => "Finish";

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
            OnPropertyChanged(nameof(SpokenSunlight));
        }
    }
}
