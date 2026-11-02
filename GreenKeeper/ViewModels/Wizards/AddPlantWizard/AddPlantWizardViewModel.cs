using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps;
using GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps.Active;
using GreenKeeper.ViewModels.Wizards.AddPlantWizard.Steps.Passive;
using GreenKeeper.ViewModels.Wizards.Base;

namespace GreenKeeper.ViewModels.Wizards.AddPlantWizard
{
    /// <summary>
    /// Five fixed steps (name, watering, fertilizing, sunlight, summary). Finishing the
    /// wizard turns their entries into a plant.
    /// </summary>
    public class AddPlantWizardViewModel : WizardViewModel
    {
        private readonly PlantNameStepViewModel _plantNameStep = new();
        private readonly WateringStepViewModel _wateringStep = new();
        private readonly FertilizingStepViewModel _fertilizingStep = new();
        private readonly SunlightStepViewModel _sunlightStep = new();

        private readonly List<IWizardStepViewModel> _steps;
        private int _currentStepIndex;

        // Null if the wizard was canceled.
        public Plant? CreatedPlant { get; private set; }

        public AddPlantWizardViewModel()
        {
            _steps = new List<IWizardStepViewModel>
            {
                _plantNameStep,
                _wateringStep,
                _fertilizingStep,
                _sunlightStep,
                new SummaryStepViewModel(_plantNameStep, _wateringStep, _fertilizingStep, _sunlightStep),
            };

            CurrentStep = _steps[_currentStepIndex];
        }

        protected override bool CanGoBack => _currentStepIndex > 0;

        protected override void GoNext()
        {
            if (_currentStepIndex == _steps.Count - 1)
            {
                CreatedPlant = BuildPlant();
                Close(true);
                return;
            }

            _currentStepIndex++;
            CurrentStep = _steps[_currentStepIndex];
        }

        protected override void GoBack()
        {
            if (!CanGoBack)
            {
                return;
            }

            _currentStepIndex--;
            CurrentStep = _steps[_currentStepIndex];
        }

        // The wizard only collects the intervals; PlantListViewModel sets the due dates when it saves the plant.
        private Plant BuildPlant()
        {
            var plant = new Plant { Name = Plant.NormalizeName(_plantNameStep.PlantName) };

            // Watering: Mandatory field, so no further check is needed.
            plant.CareSchedules.Add(_wateringStep.ToCareSchedule(CareType.Watering));

            if (_fertilizingStep.HasValidAmount)
            {
                plant.CareSchedules.Add(_fertilizingStep.ToCareSchedule(CareType.Fertilizing));
            }

            if (_sunlightStep.HasValidAmount)
            {
                plant.SunlightRequirement = _sunlightStep.ToSunlightRequirement();
            }

            return plant;
        }
    }
}
