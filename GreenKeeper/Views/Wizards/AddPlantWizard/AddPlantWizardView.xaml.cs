using GreenKeeper.Models;
using GreenKeeper.ViewModels.Wizards.AddPlantWizard;

namespace GreenKeeper.Views.Wizards.AddPlantWizard
{
    public partial class AddPlantWizardView : WizardWindow
    {
        private readonly AddPlantWizardViewModel _addPlantWizardViewModel;

        public AddPlantWizardView()
        {
            InitializeComponent();
            _addPlantWizardViewModel = new AddPlantWizardViewModel();
            Attach(_addPlantWizardViewModel);
        }

        // Null if the wizard was canceled.
        public Plant? CreatedPlant => _addPlantWizardViewModel.CreatedPlant;
    }
}
