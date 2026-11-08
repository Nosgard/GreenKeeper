using GreenKeeper.Models;
using GreenKeeper.Scheduling;
using GreenKeeper.Services;
using GreenKeeper.ViewModels.Wizards.AddScheduleWizard;

namespace GreenKeeper.Views.Wizards.AddScheduleWizard
{
    public partial class AddScheduleWizardView : WizardWindow
    {
        private readonly AddScheduleWizardViewModel _addScheduleWizardViewModel;

        public AddScheduleWizardView(Plant plant, IDialogService dialogService)
        {
            InitializeComponent();
            _addScheduleWizardViewModel = new AddScheduleWizardViewModel(plant, dialogService);
            Attach(_addScheduleWizardViewModel);
        }

        // The wizard's result for MainWindow, which saves it through DashboardViewModel.
        public ScheduleInput? Result => _addScheduleWizardViewModel.Result;
    }
}
