using GreenKeeper.Models;
using GreenKeeper.Scheduling;
using GreenKeeper.Services;
using GreenKeeper.ViewModels.Wizards.AddScheduleWizard;

namespace GreenKeeper.Views.Wizards.AddScheduleWizard
{
    /// <summary>
    /// Interaction logic for AddScheduleWizardView.xaml.
    /// </summary>
    public partial class AddScheduleWizardView : DialogWindow
    {
        private readonly AddScheduleWizardViewModel _addScheduleWizardViewModel;

        public AddScheduleWizardView(Plant plant, IDialogService dialogService)
        {
            InitializeComponent();
            _addScheduleWizardViewModel = new AddScheduleWizardViewModel(plant, dialogService);
            Attach(_addScheduleWizardViewModel);
        }

        // Exposes the wizard's result to the caller (MainWindow), analogous to
        // AddPlantWizardView.CreatedPlant - persistence itself happens outside this View, in MainViewModel.
        public ScheduleInput? Result => _addScheduleWizardViewModel.Result;
    }
}
