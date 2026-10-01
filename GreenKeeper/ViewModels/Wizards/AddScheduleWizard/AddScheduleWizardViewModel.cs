using GreenKeeper.Models;
using GreenKeeper.Models.Enums;
using GreenKeeper.Scheduling;
using GreenKeeper.Services;
using GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps;
using GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps.Active;
using GreenKeeper.ViewModels.Wizards.AddScheduleWizard.Steps.Passive;
using GreenKeeper.ViewModels.Wizards.Base;

namespace GreenKeeper.ViewModels.Wizards.AddScheduleWizard
{
    /// <summary>
    /// Two steps: the user picks a care type, then enters its values. Unlike the
    /// Add Plant wizard there is no fixed list of steps - the second one depends
    /// on the choice made in the first.
    /// </summary>
    public class AddScheduleWizardViewModel : WizardViewModel
    {
        private readonly Plant _plant;
        private readonly IDialogService _dialogService;
        private readonly CareTypeSelectionStepViewModel _selectionStep = new();

        // Created once the user has made their decision on the first step.
        private IScheduleWizardStep? _detailStep;

        // Holds the finished result. Null until the wizard has been finished.
        public ScheduleInput? Result { get; private set; }

        public AddScheduleWizardViewModel(Plant plant, IDialogService dialogService)
        {
            _plant = plant;
            _dialogService = dialogService;

            CurrentStep = _selectionStep;
        }

        protected override bool CanGoBack => CurrentStep != _selectionStep;

        protected override void GoNext()
        {
            if (CurrentStep == _selectionStep)
            {
                _detailStep = CreateDetailStep(_selectionStep.SelectedCareType);
                CurrentStep = _detailStep;
                return;
            }

            // "Finish" tries to apply the entry, but can abort without closing the
            // wizard, e.g. when the user answers "No" to the overwrite warning.
            if (TryApply())
            {
                Close(true);
            }
        }

        // Back to the first step: the detail step is discarded. An entered value is not
        // kept, as that is not worth the effort for only two steps.
        protected override void GoBack()
        {
            CurrentStep = _selectionStep;
        }

        // The one place that has to know which step belongs to which care type.
        private static IScheduleWizardStep CreateDetailStep(CareType careType)
        {
            return careType switch
            {
                CareType.Watering or CareType.Fertilizing => new ScheduleActiveStepViewModel(careType),
                CareType.Sunlight => new ScheduleSunlightStepViewModel(),
                _ => throw new ArgumentOutOfRangeException(nameof(careType), careType, null)
            };
        }

        // Keeps the wizard open when the user refuses to overwrite an existing entry.
        private bool TryApply()
        {
            var input = _detailStep!.CreateInput();

            if (input.ExistsOn(_plant) && !_dialogService.Confirm(input.OverwriteQuestion, input.OverwriteTitle))
            {
                return false;
            }

            Result = input;
            return true;
        }
    }
}
